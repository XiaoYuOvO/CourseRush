using System.Text.Json.Nodes;
using Resultful;

namespace CourseRush.Core;

public class Codec<T, TError> where TError : BasicError
{
    public Codec(Func<T, JsonObject> toJson, Func<JsonObject, Result<T, TError>> fromJson, Func<IList<TError>, TError> errorCombinator)
    {
        ToJson = toJson;
        FromJson = fromJson;
        ErrorCombinator = errorCombinator;
    }

    public Func<T, JsonObject> ToJson { get; }
    public Func<JsonObject, Result<T, TError>> FromJson { get; }
    public Func<IList<TError>, TError> ErrorCombinator { get; }
}