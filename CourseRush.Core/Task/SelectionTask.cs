using System.Text.Json.Nodes;
using Resultful;

namespace CourseRush.Core.Task;
using System.Threading.Tasks;

public interface ISelectionTask
{
    public event Action<TaskStatus>? StatusChanged;
    public TaskStatus Status { get; }
    public string Name { get; }
    public ITaskType Type { get; }
}

public abstract class SelectionTask<TError, TCourse> : ISelectionTask
    where TError : BasicError, IError<TError>, ISelectionError where TCourse : ICourse
{
    public event Action<TaskStatus>? StatusChanged;
    public RetryOption RetryOption { get; set; } = RetryOption.AlwaysRetry;
    
    public int RetryInterval { get; set; } = 1000;

    public bool IsPaused { get; private set; }

    public virtual ITaskLogger? Logger { protected get; set; }

    public TaskStatus Status {
        get => _status;
        protected internal set
        {
            _status = value;
            StatusChanged?.Invoke(value);
        }
    }

    public abstract string Name { get; }
    public ITaskType Type { get; }

    private TaskStatus _status = TaskStatus.Uninitialized;
    private readonly CancellationTokenSource _cts = new();
    private TaskCompletionSource<bool> _pauseTcs = new();
    private int _retryCount;

    protected SelectionTask(ITaskType type, ITaskLogger? logger = null)
    {
        _pauseTcs.SetResult(true);// Initially not paused
        Type = type;
        Logger = logger;
    }

    public virtual async Task<VoidResult<TError>> InitializeAndStartTask(ICourseSelector<TError, TCourse> selector)
    {
        LogInfo(Language.task_info_launched);
        while (true)
        {
            await _pauseTcs.Task;  // Wait for resume
            if (_cts.Token.IsCancellationRequested)
            {
                Status = TaskStatus.Cancelled;
                LogInfo(Language.task_log_cancelled);
                return Result.Ok<TError>();
            }
            Status = TaskStatus.Running;
            try
            {
                var result = await DoSelectionTask(selector);
                if (result.Match(_ =>
                    {
                        if (Status == TaskStatus.Next) return false;
                        Status = TaskStatus.Completed;
                        LogInfo(Language.task_log_complete);
                        return true;
                    }, error =>
                    {
                        if (RetryOption.ShouldRetry(error))
                        {
                            Status = TaskStatus.Waiting;
                            LogError(Language.task_log_failed, error);
                            _retryCount++;
                            return false;
                        }
                        Status = TaskStatus.Failed;
                        LogError(Language.task_log_failed, error);
                        return true;
                    }))
                {
                    return result.ToOneOf().TryPickT1(out var error ,out _) ? error : Result.Ok<TError>();
                }
            }
            catch (Exception e)
            {
                Status = TaskStatus.Failed;
                var selectionError = TError.Create(e.ToString());
                LogError(Language.task_log_failed, selectionError);
                return selectionError;
            }
            if (RetryInterval <= 0 || Status == TaskStatus.Next) continue;
            Status = TaskStatus.Waiting;
            LogWarn(string.Format(Language.task_log_retry, _retryCount));
            await Task.Delay(RetryInterval);
        }
    }

    protected internal abstract Task<VoidResult<TError>> DoSelectionTask(ICourseSelector<TError, TCourse> selector);
    public abstract void ApplyCourseConflict(TCourse course);
    public abstract void UndoCourseConflict(TCourse course);

    public virtual void ApplyPostSelectionCourseConflict(TCourse course)
    {
    }

    public virtual void Pause()
    {
        if (IsPaused || _status == TaskStatus.Uninitialized || _status.IsFinished()) return;
        _pauseTcs = new TaskCompletionSource<bool>();
        IsPaused = true;
        Status = TaskStatus.Paused;
        LogInfo(Language.task_log_paused);
    }

    public virtual void Resume()
    {
        if (!IsPaused || _status.IsFinished()) return;
        _pauseTcs.SetResult(true);
        IsPaused = false;
        LogInfo(Language.task_log_resumed);
    }

    public virtual void Cancel()
    {
        _cts.Cancel();
        LogInfo(Language.task_log_cancelled);
        Status = TaskStatus.Cancelled;
    }
    
    
    protected void LogInfo(string message) { Logger?.LogInfo(this, message);}
    protected void LogWarn(string message) { Logger?.LogWarn(this, message);}
    protected void LogError(string message) { Logger?.LogError(this, message);}
    protected void LogError(string message, BasicError error) { Logger?.LogError(this, message, error);}

    protected VoidResult<TaskSerializationError> ReadFromJson(JsonObject jsonObject)
    {
        if (jsonObject["retry_option"]?.GetValue<string>() is not { } retryOption)
            return new TaskSerializationError("Cannot deserialize the selection task, because retry_option is not found", jsonObject);
        if (jsonObject["retry_interval"]?.GetValue<int>() is not { } retryInterval)
            return new TaskSerializationError("Cannot deserialize the selection task, because retry_interval is not found", jsonObject);
        if (jsonObject["status"]?.GetValue<string>() is not { } status)
            return new TaskSerializationError("Cannot deserialize the selection task, because status is not found", jsonObject);
        if (jsonObject["paused"]?.GetValue<bool>() is not { } paused)
            return new TaskSerializationError("Cannot deserialize the selection task, because paused is not found", jsonObject);
        RetryOption = RetryOption.ByName(retryOption);
        RetryInterval = retryInterval;
        Status = TaskStatus.ByName(status);
        if (paused) Pause();
        return Result.Ok<TaskSerializationError>();
    }

    public virtual JsonObject ToJson()
    {
        return new JsonObject
        {
            ["retry_option"] = RetryOption.Name,
            ["retry_interval"] = RetryInterval,
            ["status"] = Status.Name,
            ["paused"] = IsPaused
        };
    }
}