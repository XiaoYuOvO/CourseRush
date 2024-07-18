namespace CourseRush.Auth;

public abstract class AuthNode
{
    internal AuthConvention AuthConvention
    {
        get;
    }

    protected AuthNode(AuthConvention convention, params AuthNode[] requires)
    {
        AuthConvention = convention;
        Requires = requires;
    }
    
    internal readonly AuthNode[] Requires;
    internal abstract void Auth(AuthDataTable table, AuthClient client);
    protected abstract string NodeName { get; }

    public AuthChain<TResult> Terminate<TResult>(Func<AuthDataTable, TResult> resultFactory) where TResult : AuthResult
    {
        return new AuthChain<TResult>(this, resultFactory);
    }

    public override string ToString()
    {
        return NodeName;
    }
}