using CourseRush.Core.Network;
using Resultful;

namespace CourseRush.Auth;

public class EmptyAuthNode : AuthNode
{
    public EmptyAuthNode(params AuthNode[] requires) : base(new AuthConvention(), requires)
    {
    }

    internal override VoidResult<AuthError> Auth(AuthDataTable table, WebClient client)
    {
        Thread.Sleep(2000);
        return Result.Ok<AuthError>();
    }

    protected override string NodeName => "Empty";
}