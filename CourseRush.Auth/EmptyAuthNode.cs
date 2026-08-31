using CourseRush.Core.Network;
using Resultful;

namespace CourseRush.Auth;

public class EmptyAuthNode : AuthNode
{
    public EmptyAuthNode(params AuthNode[] requires) : base(new AuthConvention(), requires)
    {
    }

    internal override Task<VoidResult<AuthError>> Auth(AuthDataTable table, WebClient client)
    {
        try
        {
            Thread.Sleep(2000);
            return Task.FromResult(Result.Ok<AuthError>());
        }
        catch (Exception exception)
        {
            return Task.FromException<VoidResult<AuthError>>(exception);
        }
    }

    protected override string NodeName => "Empty";
}