using System.Text;
using System.Text.Json.Nodes;
using CourseRush.Core;
using CourseRush.Core.Network;

namespace CourseRush.HNU;

public class HdjwError(string message, params BasicError[] suppressedErrors)
    : BasicError(message, suppressedErrors), ICombinableError<HdjwError>, ISelectionError
{
    public static HdjwError Wrap(Exception exception)
    {
        return new HdjwExceptionError(exception);
    }
    
    public static HdjwError Wrap(WebError error)
    {
        return new HdjwWebError(error);
    }

    public static HdjwRequestError RequestError(string message, JsonNode requestResult)
    {
        return new HdjwRequestError(message, requestResult);
    }

    public static HdjwJsonError JsonError(string message, JsonNode data)
    {
        return new HdjwJsonError(message, data);
    }
    
    public static HdjwError Combine(IEnumerable<HdjwError> errors)
    {
        // ReSharper disable once CoVariantArrayConversion
        return new HdjwError("Combined Errors", suppressedErrors: errors.ToArray());
    }

    public bool IsStudentLimitsError()
    {
        return this is HdjwRequestError requestError && "eywxt.save.stuLimit.error".Equals(requestError.ErrorCode, StringComparison.Ordinal);
    }

    public new static HdjwError Create(string message)
    {
        return new HdjwError(message);
    }
}

public class HdjwRequestError : HdjwError
{
    private JsonNode RequestResult { get; }
    public string? ErrorCode { get; }

    public HdjwRequestError(string message, JsonNode requestResult) : base(message)
    {
        ErrorCode = requestResult["errorCode"]?.GetValue<string>();
        RequestResult = requestResult;
    }

    protected override string BuildMessage(string baseMessage)
    {
        var result = new StringBuilder(baseMessage);
        if (ErrorCode != null)
            result.Append($"\nHdjw error code: {ErrorCode}");
        if (baseMessage.Length == 0)
            result.Append($"\nRequest result: {RequestResult.ToJsonString()}");
        return result.ToString();
    }
}

public class HdjwJsonError : HdjwError
{
    private JsonNode Data { get; }

    public HdjwJsonError(string message, JsonNode data) : base(message)
    {
        Data = data;
    }

    protected override string BuildMessage(string baseMessage)
    {
        return baseMessage + $"\nRequest result: {Data.ToJsonString()}";
    }
}

public class HdjwExceptionError : HdjwError
{
    public Exception Exception { get; }

    public HdjwExceptionError(Exception exception) : base(exception.Message)
    {
        Exception = exception;
    }
}


public class HdjwWebError : HdjwError
{
    public WebError Inner { get; }

    public HdjwWebError(WebError inner) : base("",inner)
    {
        Inner = inner;
    }
}