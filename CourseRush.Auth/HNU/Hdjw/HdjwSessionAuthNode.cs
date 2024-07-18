using System.Net;
using System.Text.Json.Nodes;
using JWT;
using JWT.Serializers;

namespace CourseRush.Auth.HNU.Hdjw;
using static HNUAuthData;
public class HdjwSessionAuthNode : AuthNode
{
    private const string HdjwIndex = "http://hdjw.hnu.edu.cn/";

    private const string CasLoginHdjw =
        "http://cas.web.hnu.edu.cn/cas/login?service=http://hdjw.hnu.edu.cn/caslogin?redirect_url=/Njw2017/index.html";
    public HdjwSessionAuthNode(params AuthNode[] requires) : base(new AuthConvention()
        .Requires(PC0, PF0, PV0, SID, SID_SIG, SID_LEGACY, SID_LEGACY_SIG)
        .Provides(SESSION, SDP_APP_SESSION_80, TOKEN), requires) {}

    internal override void Auth(AuthDataTable table, AuthClient client)
    {
        var indexResponse = client.GetAny(new Uri(HdjwIndex)).ReadHtml().Text;
        var startIndex = indexResponse.IndexOf("https://", StringComparison.Ordinal);
        var length = indexResponse.LastIndexOf("\");", StringComparison.Ordinal) - startIndex;
        var redirectedUri = indexResponse.Substring(startIndex,length);
        // var cookieCollection = .GetCurrentCookies();
        // cookieCollection.Add(new Cookie("lang","zh-cn"));
        // cookieCollection.Add(new Cookie("language","zh-CN"));
        // cookieCollection.Add(new Cookie("online","1"));
        var verifyResponse = client.GetAny(client.GetRedirectedUri(new Uri(redirectedUri)));
        var sdpAppSession = verifyResponse.GetCurrentCookies()["sdp_app_session-80"]?.Value ?? throw new InvalidDataException("SDP app session is missing after verification");
        Console.WriteLine($"SDP app session: {sdpAppSession}");

        var casLoggedInResponse = client.GetRedirectedUri(new Uri(CasLoginHdjw));
        var loggedInCookies = client.GetRedirectedUri(casLoggedInResponse).GetCurrentCookies();
        var token = loggedInCookies["token"]?.Value ?? throw new InvalidDataException("Token is missing after hdjw cas login");
        // Console.WriteLine($"Token: {token}");
        // Console.WriteLine($"authcode: {loggedInCookies["authcode"]?.Value}");
        table.UpdateData(SDP_APP_SESSION_80, sdpAppSession);
        var decode = JsonNode.Parse(new JwtDecoder(new DefaultJsonSerializerFactory().Create(), new JwtBase64UrlEncoder())
            .Decode(token, false))?["sid"]?.GetValue<string>() 
                     ?? throw new InvalidDataException($"The token jwt doesn't contains the session id {token}");
        table.UpdateData(SESSION, decode);
        table.UpdateData(TOKEN, token);
    }

    protected override string NodeName => "HdjwCasLogin";
}