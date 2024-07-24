using CourseRush.Core.Network;

namespace CourseRush.Auth;

public class AuthClient<TAuthResult> : WebClient where TAuthResult : AuthResult
{
    protected readonly TAuthResult Auth;

    protected AuthClient(TAuthResult auth)
    {
        Auth = auth;
        auth.InjectAuthInfo(Handler);
    }

    protected override HttpRequestMessage CreateRequest(Uri uri, MediaType accept, string encoding = "gzip, deflate, br", RequestConfigurator? configurator = null)
    {
        var request = base.CreateRequest(uri, accept, encoding, configurator);
        Auth.InjectAuthInfo(request);
        return request;
    }
}