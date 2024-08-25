using System.Text.Json.Nodes;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Core.Task;
using System.Threading.Tasks;

public class PrioritizedSelectionTask<TError, TCourse>(IReadOnlyList<SelectionTask<TError, TCourse>> subTasks, ITaskLogger? logger = null)
    : AggregatedSelectionTask<TError, TCourse>(TaskTypes<TError, TCourse>.PrioritizedSelectionTask, subTasks, logger), 
        IJsonSerializable<PrioritizedSelectionTask<TError, TCourse>, TaskSerializationError>
    where TError : BasicError, ICombinableError<TError>, ISelectionError
    where TCourse : ICourse, IJsonSerializable<TCourse, TError>
{
    public override string Name => string.Format(Language.task_prioritized_tasks, SubTasks.Count);
    private int _currentTaskIndex;
    protected internal override async Task<VoidResult<TError>> DoSelectionTask(ICourseSelector<TError, TCourse> selector)
    {
        if (SubTasks.Count == 0) return Result.Ok<TError>();
        var currentTask = SubTasks[_currentTaskIndex];
        currentTask.Status = TaskStatus.Running;
        await currentTask.DoSelectionTask(selector).ContinueWith(task => task.Result.TeeError(error =>
        {
            currentTask.Status = TaskStatus.Failed;
            Status = TaskStatus.Next;
            Interlocked.Increment(ref _currentTaskIndex);
            Logger?.LogError(currentTask, Language.task_log_failed, error);
            LogWarn(string.Format(Language.task_info_prioritized_next, currentTask.Name, _currentTaskIndex + 1));
        }).Tee(_ =>
        {
            Logger?.LogInfo(currentTask, Language.task_log_complete);
            currentTask.Status = TaskStatus.Completed;
        }));
        return Result.Ok<TError>();
    }

    public override void ApplyPostSelectionCourseConflict(TCourse course)
    {
        for (var index = 0; index < SubTasks.Count; index++)
        {
            if (index == _currentTaskIndex) continue;
            SubTasks[index].UndoCourseConflict(course);
        }
    }

    public override JsonObject ToJson()
    {
        var jsonObject = base.ToJson();
        jsonObject["current"] = _currentTaskIndex;
        return jsonObject;
    }

    public static Result<PrioritizedSelectionTask<TError, TCourse>, TaskSerializationError> FromJson(JsonObject jsonObject)
    {
        return ReadSubTasksFromJson(jsonObject)
            .Map(tasks => new PrioritizedSelectionTask<TError, TCourse>(tasks))
            .BindAction(task => task.ReadFromJson(jsonObject))
            .Bind<PrioritizedSelectionTask<TError, TCourse>>(task =>
            {
                if (jsonObject["current"]?.GetValue<int>() is not { } current)
                    return new TaskSerializationError("Cannot find current task index in json", jsonObject);
                task._currentTaskIndex = current;
                return task;
            });
    }
}