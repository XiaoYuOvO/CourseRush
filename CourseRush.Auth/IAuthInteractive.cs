using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Auth;

public interface IAuthInteractive
{
    Task<Result<string, AuthError>> RequestActionWithPayload(NamedAction? action, string title, string description);
    
    Task<VoidResult<AuthError>> RequestAction(NamedAction? action, string title, string description);
    
    void ShowInfo(string message);
}