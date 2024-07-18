using System.Net.Http.Headers;
using System.Text.Json;

namespace CourseRush.Auth.HNU;
using static HNUAuthData;
public class WebVpnAuthNode : AuthNode
{
    private const string AuthConfig = "https://webvpn.hnu.edu.cn/passport/v1/public/authConfig?clientType=SDPBrowserClient&platform=Windows&lang=zh-CN&mod=1&needTicket=1";
    private const string AccessCheck = "https://webvpn.hnu.edu.cn/passport/v1/auth/accessCheck?clientType=SDPBrowserClient&platform=Windows&lang=zh-CN";
    public WebVpnAuthNode(params AuthNode[] requires) : base(new AuthConvention()
        .Requires(CAS_AUTH_REDIRECT_URL)
        .Provides(SID, SID_SIG, SID_LEGACY, SID_LEGACY_SIG), requires)
    {
    }

    internal override void Auth(AuthDataTable table, AuthClient client)
    {
        var webVpnAuthUri = client.GetRedirectedUri(table.RequireData<AuthDataKey<Uri>, Uri>(CAS_AUTH_REDIRECT_URL));
        client.GetAny(webVpnAuthUri);
        
        //Auth Config
        var jsonDocument = client.GetAny(new Uri(AuthConfig)).ReadJsonObject();
        if (jsonDocument["code"]?.GetValue<int>() != 0)
        {
            throw new HttpRequestException($"The result of the Auth Config is not zero {jsonDocument.ToJsonString()}");
        }

        //Access Check
        var authResponse = client.GetAny(new Uri(AccessCheck), request =>
        {
            request.Method = HttpMethod.Post;
            request.Content = new FormUrlEncodedContent(new []
            {
                KeyValuePair.Create("clientType","SDPBrowserClient"), 
                KeyValuePair.Create("platform","Windows"), 
                KeyValuePair.Create<string, string>("lang","zh-CN"),
            });
            request.Headers.Add("x-csrf-token", jsonDocument["data"]?["security"]?["csrfToken"]?.GetValue<string>());
        });
        var accessCheckResult = authResponse.ReadJsonObject();
        if (accessCheckResult["code"]?.GetValue<int>() != 0)
        {
            throw new HttpRequestException($"The result of the Access Check is not zero {accessCheckResult.ToJsonString()}");
        }

        var cookieCollection = authResponse.GetCurrentCookies();
        table.UpdateData(SID, cookieCollection["sid"]?.Value ?? throw new InvalidOperationException("sid not found in cookie collection after access check"));
        table.UpdateData(SID_SIG, cookieCollection["sid.sig"]?.Value ?? throw new InvalidOperationException("sid.sig not found in cookie collection after access check"));
        table.UpdateData(SID_LEGACY, cookieCollection["sid-legacy"]?.Value ?? throw new InvalidOperationException("sid.legacy not found in cookie collection after access check"));
        table.UpdateData(SID_LEGACY_SIG, cookieCollection["sid-legacy.sig"]?.Value ?? throw new InvalidOperationException("sid.sig not found in cookie collection after access check"));
    }

    protected override string NodeName => "WebVPNAuth";
}