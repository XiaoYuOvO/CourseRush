using System.Net.Http.Headers;
using CourseRush.Auth.Crypto;
using CourseRush.Core.Network;

namespace CourseRush.Auth.HNU;

public class CASAuthNode : AuthNode
{
    private const string PubKey = "http://cas.web.hnu.edu.cn//cas/v2/getPubKey?sf_request_type=ajax";

    private const string CASLoginWebVpn =
        "http://cas.web.hnu.edu.cn/cas/login?service=https%3A%2F%2Fwebvpn2.hnu.edu.cn%3A443%2Fpassport%2Fv1%2Fauth%2Fcas";
    public CASAuthNode(params AuthNode[] requires) : base(new AuthConvention().
            Requires(CommonDataKey.UserName, CommonDataKey.Password).
            Provides(HNUAuthData.PC0, HNUAuthData.PF0, HNUAuthData.PV0, HNUAuthData.JSESSIONID, HNUAuthData.CAS_AUTH_REDIRECT_URL), requires) {}

    internal override void Auth(AuthDataTable table, WebClient client)
    {
        var htmlDocument = client.Get(new Uri(CASLoginWebVpn), accept:MediaType.Html).ReadHtml();
        table.UpdateData(HNUAuthData.JSESSIONID, client.GetCookie(HNUAuthData.JSESSIONID.KeyName)?.Value ?? throw new InvalidOperationException("Cannot get JSESSIONID from cas login"));
        var execution = htmlDocument.DocumentNode.SelectSingleNode("//input[@name='execution']").GetAttributeValue("value", "");
        var pubKeyElement = client.Get(new Uri(PubKey), accept:MediaType.Json).ReadJsonObject();
        var modulus = pubKeyElement["modulus"]?.GetValue<string>() ?? throw new InvalidOperationException("Cannot find modulus in pubKey response");
        var exponent = pubKeyElement["exponent"]?.GetValue<string>() ?? throw new InvalidOperationException("Cannot find exponent in pubKey response");
        var encryptedPassword = RsaWebEncryptor.Encrypt(modulus, exponent, new string(table.RequireData<AuthDataKey<string>, string>(CommonDataKey.Password).Reverse().ToArray()));
        var uri = new Uri(CASLoginWebVpn);
        table.UpdateData(HNUAuthData.CAS_AUTH_REDIRECT_URL,client.GetRedirectedUri(uri,
            configurator:message =>
            {
                message.Method = HttpMethod.Post;
                message.Content = new StringContent($"username={table.RequireData<AuthDataKey<string>, string>(CommonDataKey.UserName)}&password={encryptedPassword}&authcode=&execution={execution}&_eventId=submit");
                message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            }));
    }

    protected override string NodeName => "CasAuth";
}