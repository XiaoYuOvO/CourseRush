using System.Net;
using CourseRush.Core.Network;
using CourseRush.Core.Util;
using OneOf;
using Resultful;
using WebClient = CourseRush.Core.Network.WebClient;
using WebResponse = CourseRush.Core.Network.WebResponse;

namespace CourseRush.Auth.HNU.Hdjw;

public class InternalHdjwAuthNode(params AuthNode[] requires) : AuthNode(
    new AuthConvention().Requires(HNUAuthData.CAS_AUTH_REDIRECT_URL)
        .Provides(HNUAuthData.TOKEN, HNUAuthData.SESSION), requires)
{
    private const string LocationReplace = "location.replace(\"";
    private const string WebvpnHost = "http://webvpn2.hnu.edu.cn/";
    internal override VoidResult<AuthError> Auth(AuthDataTable table, WebClient client)
    {
        return table.RequireData(HNUAuthData.CAS_AUTH_REDIRECT_URL)
            .BindAction(redirectUri => 
                client.GetRedirectedUriOrNormal(redirectUri).Bind(response =>
                    {
                        return response.Match<Result<WebResponse,WebError>>(r =>
                        {
                            //For new Hdjw
                            return r.RedirectUri.Bind(uri => client.GetRedirectedUri(uri).Bind(loginToXk =>
                                loginToXk.RedirectUri.Bind(loginToXkUri => client.GetRedirectedUri(loginToXkUri).Cast<WebResponse>())));
                        }, webResponse =>
                        {
                            return client.GetRedirectedUri(new Uri(webResponse.ReadString().Between(LocationReplace, "\");"))).Bind(
                                verifyResponse =>
                                    client.GetRedirectedUri(new Uri(WebvpnHost + verifyResponse.ReadString().Between("href=\"", "\">Found"))).Bind(
                                        redirectionResponse => redirectionResponse.RedirectUri.Bind(uri => client.Get(uri))));
                        });
                    })
                    .Bind(_ => client.GetCookie(HNUAuthData.BZB_JSXSD.KeyName).Tee(token => table.UpdateData(HNUAuthData.BZB_JSXSD, token.Value)))
                    .Bind(_ => client.GetCookie(HNUAuthData.SERVERID.KeyName).Tee(session => table.UpdateData(HNUAuthData.SERVERID, session.Value)))
                .MapError(error => new AuthError("Failed to get token in cas redirect", this, error)).DiscardValue())
            .DiscardValue();
    }

    protected override string NodeName => "InternalHdjwAuthNode";
}