using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Core.Task;

public static class TaskTypes<TError, TCourse>
    where TCourse : ICourse, IJsonSerializable<TCourse, TError>
    where TError : BasicError, ISelectionError, ICombinableError<TError>
{
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")] 
    private static readonly Dictionary<string, ITaskType> TaskRegistry = [];
    public static readonly TaskType<SubmitSelectionTask<TError, TCourse>> SubmitSelectionTask = Register<SubmitSelectionTask<TError, TCourse>>("submit");
    public static readonly TaskType<RemoveSelectionTask<TError, TCourse>> RemoveSelectionTask = Register<RemoveSelectionTask<TError, TCourse>>("remove");
    public static readonly TaskType<PrioritizedSelectionTask<TError, TCourse>> PrioritizedSelectionTask = Register<PrioritizedSelectionTask<TError, TCourse>>("prioritized");
    public static readonly TaskType<SequentialSelectionTask<TError, TCourse>> SequentialSelectionTask = Register<SequentialSelectionTask<TError, TCourse>>("sequential");
    public static readonly TaskType<ParallelSelectionTask<TError, TCourse>> ParallelSelectionTask = Register<ParallelSelectionTask<TError, TCourse>>("parallel");

    private static TaskType<TTask> Register<TTask>(string name) where TTask : SelectionTask<TError, TCourse>, IJsonSerializable<TTask, TaskSerializationError>
    {
        var taskType = new TaskType<TTask>(name);
        TaskRegistry[name] = taskType;
        return taskType;
    }

    public static JsonArray SerializeAll(IReadOnlyList<SelectionTask<TError, TCourse>> tasks)
    {
        var array = new JsonArray();
        foreach (var selectionTask in tasks)
        {
            array.Add(new JsonObject
            {
                ["type"] = selectionTask.Type.Name,
                ["task"] = selectionTask.ToJson()
            });
        }

        return array;
    }

    public static Result<IReadOnlyList<SelectionTask<TError, TCourse>>, TaskSerializationError> DeserializeAll(JsonArray array)
    {
        return array.Select(jsonNode =>
        {
            if (jsonNode is not JsonObject jsonObject)
                return new TaskSerializationError("Invalid non-object node in array", jsonNode);
            if (!jsonObject.ContainsKey("type") || jsonObject["type"]?.GetValue<string>() is not { } type)
                return new TaskSerializationError("The task json object contains no type", jsonNode);
            if (!jsonObject.ContainsKey("task") || jsonObject["task"] is not JsonObject taskObject)
                return new TaskSerializationError("The task json object contains no task data object", jsonNode);
            return TaskRegistry[type].FromJsonObject(taskObject).Cast<SelectionTask<TError, TCourse>>();
        }).CombineResults();
    }
}

public interface ITaskType
{
    public string Name { get;}
    public Result<ISelectionTask, TaskSerializationError> FromJsonObject(JsonObject jsonObject);
}

public class TaskType<TTask>(string name) : ITaskType where TTask : ISelectionTask, IJsonSerializable<TTask, TaskSerializationError>
{
    public string Name { get; } = name;

    public Result<ISelectionTask, TaskSerializationError> FromJsonObject(JsonObject jsonObject)
    {
        return TTask.FromJson(jsonObject).Cast<ISelectionTask>();
    }
}