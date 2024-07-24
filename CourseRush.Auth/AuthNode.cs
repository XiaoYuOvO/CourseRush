using CourseRush.Core.Network;
using Resultful;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="table"></param>
    /// <param name="client"></param>
    /// <exception cref="AuthException">
    /// When occurs authentication error from server or server returned invalid data
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// When server returned data with invalid format  
    /// </exception>
    internal abstract VoidResult<AuthError> Auth(AuthDataTable table, WebClient client);
    protected abstract string NodeName { get; }

    public AuthChain<TResult> Terminate<TResult>(Func<AuthDataTable, Result<TResult, AuthError>> resultFactory) where TResult : AuthResult
    {
        return new AuthChain<TResult>(this, resultFactory);
    }

    public override string ToString()
    {
        return NodeName;
    }
}