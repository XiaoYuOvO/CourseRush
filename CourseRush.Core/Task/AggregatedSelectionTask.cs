using System.Text.Json.Nodes;
using Resultful;

namespace CourseRush.Core.Task;

public abstract class AggregatedSelectionTask<TError, TCourse>(ITaskType type, IReadOnlyList<SelectionTask<TError, TCourse>> subTasks, ITaskLogger? logger = null)
    : SelectionTask<TError, TCourse>(type, logger)
    where TError : BasicError, ICombinableError<TError>, ISelectionError
    where TCourse : ICourse, IJsonSerializable<TCourse, TError>
{
    public override ITaskLogger? Logger {
        protected get => base.Logger;
        set
        {
            foreach (var selectionTask in SubTasks)
            {
                selectionTask.Logger = value;
            }
            base.Logger = value;
        }
    }
    public IReadOnlyList<SelectionTask<TError, TCourse>> SubTasks { get; } = subTasks;
    
    public override void ApplyCourseConflict(TCourse course)
    {
        foreach (var selectionTask in SubTasks)
        {
            selectionTask.ApplyCourseConflict(course);
        }
    }

    public override void UndoCourseConflict(TCourse course)
    {
        foreach (var selectionTask in SubTasks)
        {
            selectionTask.UndoCourseConflict(course);
        }
    }
    
    public override void Pause()
    {
        base.Pause();
        foreach (var selectionTask in SubTasks)
        {
            selectionTask.Pause();
        }
    }

    public override void Resume()
    {
        base.Resume();
        Status = TaskStatus.Running;
        foreach (var selectionTask in SubTasks)
        {
            selectionTask.Resume();
        }
    }

    public override void Cancel()
    {
        base.Cancel();
        foreach (var selectionTask in SubTasks)
        {
            selectionTask.Cancel();
        }
    }

    public override JsonObject ToJson()
    {
        var jsonObject = base.ToJson();
        jsonObject["sub_tasks"] = TaskTypes<TError, TCourse>.SerializeAll(SubTasks);
        return jsonObject;
    }

    protected static Result<IReadOnlyList<SelectionTask<TError, TCourse>>, TaskSerializationError> ReadSubTasksFromJson(JsonObject jsonObject)
    {
        if (jsonObject["sub_tasks"] is not JsonArray taskArray) return new TaskSerializationError("Cannot find sub tasks array in json", jsonObject);
        return TaskTypes<TError, TCourse>.DeserializeAll(taskArray);
    }
}