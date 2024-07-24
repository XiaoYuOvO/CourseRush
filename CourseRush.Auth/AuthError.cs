using CourseRush.Core;

namespace CourseRush.Auth;

public class AuthError : BasicError
{
    private readonly AuthNode? _errorNode;
    public AuthError(string message, AuthNode? errorNode = null, params BasicError[] suppressedErrors) : base(message, suppressedErrors)
    {
        _errorNode = errorNode;
    }

    protected override string BuildMessage(string baseMessage)
    {
        return $"{base.BuildMessage(baseMessage)}\n   In auth node: {_errorNode}";
    }
}