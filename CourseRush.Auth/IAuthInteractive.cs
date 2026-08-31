using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Auth;

public interface IAuthInteractive
{
    Task<Result<string, AuthError>> RequestActionWithPayload(NamedAction? action, TranslatableText title, TranslatableText description);
    
    Task<VoidResult<AuthError>> RequestAction(NamedAction? action, TranslatableText title, TranslatableText description);
    
    void ShowInfo(TranslatableText message);
}