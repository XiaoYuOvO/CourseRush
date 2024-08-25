using System.Text.Json.Nodes;
using Resultful;

namespace CourseRush.Core.Task;

public abstract class
    SingleSelectionTask<TError, TCourse>(ITaskType type, TCourse targetCourse, ITaskLogger? logger = null)
    : SelectionTask<TError, TCourse>(type, logger) where TError : BasicError, ISelectionError, ICombinableError<TError>
    where TCourse : ICourse, IJsonSerializable<TCourse, TError>
{
    public TCourse TargetCourse { get; } = targetCourse;

    public override JsonObject ToJson()
    {
        var jsonObject = base.ToJson();
        jsonObject["target_course"] = TargetCourse.ToJson();
        return jsonObject;
    }

    protected static Result<TCourse, TaskSerializationError> ReadCourseFromJson(JsonObject jsonObject)
    {
        if (jsonObject["target_course"] is not JsonObject courseObject) return new TaskSerializationError("Cannot find target course of single selection task", jsonObject);
        return TCourse.FromJson(courseObject).MapError(error => new TaskSerializationError("Cannot deserialize target course of single selection task", jsonObject, error));
    }
}