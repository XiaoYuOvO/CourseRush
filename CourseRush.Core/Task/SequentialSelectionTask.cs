using System.Text.Json.Nodes;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Core.Task;

public class SequentialSelectionTask<TError, TCourse>(
    IReadOnlyList<SelectionTask<TError, TCourse>> subTasks,
    ITaskLogger? logger = null)
    : AggregatedSelectionTask<TError, TCourse>(TaskTypes<TError, TCourse>.SequentialSelectionTask, subTasks, logger),
        IJsonSerializable<SequentialSelectionTask<TError, TCourse>, TaskSerializationError>
    where TCourse : ICourse, IJsonSerializable<TCourse, TError>
    where TError : BasicError, ISelectionError, ICombinableError<TError>
{
    public override string Name => string.Format(Language.task_sequential_tasks, SubTasks.Count);
    private int _current;
    
    protected internal override async Task<VoidResult<TError>> DoSelectionTask(ICourseSelector<TError, TCourse> selector)
    {
        if (SubTasks.Count == 0) return Result.Ok<TError>();
        if (_current == SubTasks.Count)
        {
            Status = TaskStatus.Completed;
            return Result.Ok<TError>();
        }
        var currentTask = SubTasks[_current];
        currentTask.Status = TaskStatus.Running;
        return await currentTask.InitializeAndStartTask(selector).ContinueWith(task => task.Result.Tee(_ =>
        {
            currentTask.Status = TaskStatus.Completed;
            Status = TaskStatus.Next;
            _current++;
            LogInfo(string.Format(Language.task_info_sequential_task_next, currentTask.Name, _current + 1));
        }).TeeError(_ => Status = TaskStatus.Failed));
    }

    public override JsonObject ToJson()
    {
        var jsonObject = base.ToJson();
        jsonObject["current"] = _current;
        return jsonObject;
    }

    public static Result<SequentialSelectionTask<TError, TCourse>, TaskSerializationError> FromJson(JsonObject jsonObject)
    {
        return ReadSubTasksFromJson(jsonObject)
            .Map(tasks => new SequentialSelectionTask<TError, TCourse>(tasks))
            .BindAction(task => task.ReadFromJson(jsonObject))
            .Bind<SequentialSelectionTask<TError, TCourse>>(task =>
            {
                if (jsonObject["current"]?.GetValue<int>() is not { } current)
                    return new TaskSerializationError("Cannot find current task index in json", jsonObject);
                task._current = current;
                return task;
            });
    }
}