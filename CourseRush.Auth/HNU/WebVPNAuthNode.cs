using System.Net;
using CourseRush.Core;
using CourseRush.Core.Network;
using CourseRush.Core.Util;
using Resultful;
using WebClient = CourseRush.Core.Network.WebClient;
using WebResponse = CourseRush.Core.Network.WebResponse;

namespace CourseRush.Auth.HNU;
using static HNUAuthData;
public class WebVpnAuthNode : AuthNode
{
    private const string AuthConfig = "https://webvpn2.hnu.edu.cn/passport/v1/public/authConfig?clientType=SDPBrowserClient&platform=Windows&lang=zh-CN&mod=1&needTicket=1";
    private const string AccessCheck = "https://webvpn2.hnu.edu.cn/passport/v1/auth/accessCheck?clientType=SDPBrowserClient&platform=Windows&lang=zh-CN";
    public WebVpnAuthNode(params AuthNode[] requires) : base(new AuthConvention()
        .Requires(CAS_AUTH_REDIRECT_URL)
        .Provides(SID, SID_SIG, SID_LEGACY, SID_LEGACY_SIG), requires)
    {
    }

    internal override VoidResult<AuthError> Auth(AuthDataTable table, WebClient client)
    {
        table.RequireData<AuthDataKey<Uri>, Uri>(CAS_AUTH_REDIRECT_URL)
            .Bind<WebResponse>(uri => client.GetRedirectedUri(uri)
                .Bind<WebResponse>(response => response.RedirectUri
                    .Bind(webVpnAuthUri => client.Get(webVpnAuthUri)))
                .MapError(error => new AuthError("Failed to get cas auth redirect url", this, error))).DiscardValue();
        
        //Auth Config
        return client.Get(new Uri(AuthConfig), accept:MediaType.Json).Bind(res => res.ReadJsonObject()).MapError(error => new AuthError("Failed to read AuthConfig json", this, error))
            .Bind<string>(jsonDocument =>
        {
            if (jsonDocument["code"]?.GetValue<int>() != 0)
            {
                return new AuthError($"The result of the Auth Config is not zero {jsonDocument.ToJsonString()}", this);
            }

            return jsonDocument["data"]?["security"]?["csrfToken"]?.GetValue<string?>()?.Ok<string, AuthError>() 
                   ?? new AuthError($"Cannot find data.security.csrfToken in json: {jsonDocument.ToJsonString()}", this);
        })
            .Bind<CookieCollection>(csrfToken =>
        {
            var authResponse = client.Post(new Uri(AccessCheck), accept:MediaType.All, content:new Dictionary<string, string>{
                {"clientType", "SDPBrowserClient"},
                {"platform","Windows"},
                {"lang","zh-CN"}
            }, configurator:request =>
            {
                request.Headers.Add("x-csrf-token", csrfToken);
            }).MapError(error => new AuthError("Failed to check access", this, error));
            
            //Access Check
            return authResponse
                .Bind(res => res.ReadJsonObject().MapError(error => new AuthError("Failed to read access check json", this, error)))
                .Bind<CookieCollection>(accessCheckResult =>
            {
                if (accessCheckResult["code"]?.GetValue<int>() != 0)
                {
                    return new AuthError($"The result of the Access Check is not zero {accessCheckResult.ToJsonString()}");
                }

                return authResponse
                    .Bind<CookieCollection>(res => res.GetCurrentCookies());
            });
        })
            //Store results
            .DiscardValue(cookieCollection =>
            {
                return cookieCollection["sid"].ToOption().AcceptOr(val => table.UpdateData(SID, val.Value),
                         () => new AuthError("sid not found in cookie collection after access check", this))
                    .Bind(_ => cookieCollection["sid.sig"].ToOption().AcceptOr(val => table.UpdateData(SID_SIG, val.Value),
                        () => new AuthError("sid.sig not found in cookie collection after access check", this)))
                    .Bind(_ => cookieCollection["sid-legacy"].ToOption().AcceptOr(val => table.UpdateData(SID_LEGACY, val.Value),
                        () => new AuthError("sid.legacy not found in cookie collection after access check", this)))
                    .Bind(_ => cookieCollection["sid-legacy.sig"].ToOption().AcceptOr(val => table.UpdateData(SID_LEGACY_SIG, val.Value), 
                        () => new AuthError("sid.sig not found in cookie collection after access check")));
        });
    }

    protected override string NodeName => "WebVPNAuth";
}