using System.Collections.Immutable;
using System.Text;

namespace CourseRush.Core;

public class BasicError
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

    public ImmutableList<BasicError> SuppressedErrors { get; }

    protected BasicError(string message, params BasicError[] suppressedErrors)
    {
        BaseMessage = message;
        SuppressedErrors = suppressedErrors.ToImmutableList();
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