using System.Text.Json.Nodes;
using Resultful;

namespace CourseRush.Core;

public interface IJsonSerializable<TValue, TError> where TError : BasicError, ICombinableError<TError> where TValue : IJsonSerializable<TValue, TError> 
{
    JsonObject ToJson();
    static abstract Result<TValue, TError> FromJson(JsonObject jsonObject);

    static virtual JsonObject ToJson(TValue value)
    {
        return value.ToJson();
    }
}