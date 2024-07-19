using CourseRush.Core.Network;

namespace CourseRush.Auth;

public class AuthClient<TAuthResult> : WebClient where TAuthResult : AuthResult
{
    private readonly TAuthResult _auth;
    public AuthClient(TAuthResult auth)
    {
        _auth = auth;
        auth.InjectAuthInfo(Handler);
    }

    protected override HttpRequestMessage CreateRequest(Uri uri, MediaType accept, string encoding = "gzip, deflate, br", RequestConfigurator? configurator = null)
    {
        var request = base.CreateRequest(uri, accept, encoding, configurator);
        _auth.InjectAuthInfo(request);
        return request;
    }
}