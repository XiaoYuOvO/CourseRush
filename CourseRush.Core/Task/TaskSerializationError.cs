using System.Text.Json.Nodes;

namespace CourseRush.Core.Task;

public class TaskSerializationError(string message, JsonNode? errorNode = null, params BasicError[] suppressedErrors)
    : BasicError(message, suppressedErrors), ICombinableError<TaskSerializationError>
{
    public static TaskSerializationError Combine(IEnumerable<TaskSerializationError> errors)
    {
        return new TaskSerializationError("Combined serialization error", null, errors.Cast<BasicError>().ToArray());
    }

    protected override string BuildMessage(string baseMessage)
    {
        return $"{baseMessage}:    \nError json:{errorNode}";
    }
}