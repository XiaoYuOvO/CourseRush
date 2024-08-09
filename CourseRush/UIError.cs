using CourseRush.Core;

namespace CourseRush;

public class UiError : BasicError
{
    public UiError(string message, params BasicError[] suppressedErrors) : base(message, suppressedErrors)
    {
    }
}