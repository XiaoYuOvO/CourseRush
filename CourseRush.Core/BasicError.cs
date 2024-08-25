using System.Collections.Immutable;
using System.Text;

namespace CourseRush.Core;

public interface ICombinableError<TError> where TError : BasicError
{
    static abstract TError Combine(IEnumerable<TError> errors);
}

public class BasicError : ICombinableError<BasicError>
{
    public string Message
    {
        get
        {
            if (_messageCache != null)
            {
                return _messageCache;
            }

            return _messageCache = BuildMessageInternal(BaseMessage);
        }
    }

    private string? _messageCache;
    private string BaseMessage { get; }

    public IReadOnlyList<BasicError> SuppressedErrors { get; }

    protected BasicError(string message, IReadOnlyList<BasicError> suppressedErrors)
    {
        BaseMessage = message;
        SuppressedErrors = suppressedErrors.ToImmutableList();
    }

    public static BasicError Combine(IEnumerable<BasicError> errors)
    {
        return new BasicError("Combined Error", errors.ToArray());
    }

    public override string ToString()
    {
        return Message;
    }

    private string BuildMessageInternal(string baseMessage)
    {
        var message = new StringBuilder(BuildMessage(baseMessage));
        if (!SuppressedErrors.Any()) return message.ToString();
        
        message.Append("\n    Cause by:");
        var index = 1;
        foreach (var suppressedError in SuppressedErrors)
        {
            message.Append(string.Join($"\n  {index}. ", suppressedError.Message));
            index++;
        }
        return message.ToString();
    }

    protected virtual string BuildMessage(string baseMessage)
    {
        return baseMessage;
    }
}