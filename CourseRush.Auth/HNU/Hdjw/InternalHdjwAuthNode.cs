using CourseRush.Core.Network;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Auth.HNU.Hdjw;

public class InternalHdjwAuthNode(params AuthNode[] requires) : AuthNode(
    new AuthConvention().Requires(HNUAuthData.CAS_AUTH_REDIRECT_URL)
        .Provides(HNUAuthData.TOKEN, HNUAuthData.SESSION), requires)
{
    internal override VoidResult<AuthError> Auth(AuthDataTable table, WebClient client)
    {
        return table.RequireData(HNUAuthData.CAS_AUTH_REDIRECT_URL)
            .BindAction(redirectUri => 
                client.GetRedirectedUri(redirectUri)
                    .Bind(_ => client.GetCookie("token").Tee(token => table.UpdateData(HNUAuthData.TOKEN, token.Value)))
                    .Bind(_ => client.GetCookie("SESSION").Tee(session => table.UpdateData(HNUAuthData.SESSION, session.Value)))
                .MapError(error => new AuthError("Failed to get token in cas redirect", this, error)).DiscardValue())
            .DiscardValue();
    }

    protected override string NodeName => "InternalHdjwAuthNode";
}