using CourseRush.Auth.Crypto;
using CourseRush.Core.Network;
using Resultful;

namespace CourseRush.Auth.HNU;

public class CASAuthNode(params AuthNode[] requires) : AuthNode(
    new AuthConvention().Requires(CommonDataKey.UserName, CommonDataKey.Password).Provides(HNUAuthData.PC0,
        HNUAuthData.PF0, HNUAuthData.PV0, HNUAuthData.JSESSIONID, HNUAuthData.CAS_AUTH_REDIRECT_URL), requires)
{
    private const string PubKey = "http://cas.hnu.edu.cn//cas/v2/getPubKey?sf_request_type=ajax";

    protected virtual string CASLoginUrl => "http://cas.web.hnu.edu.cn/cas/login?service=https%3A%2F%2Fwebvpn2.hnu.edu.cn%3A443%2Fpassport%2Fv1%2Fauth%2Fcas";

    internal override VoidResult<AuthError> Auth(AuthDataTable table, WebClient client)
    {
        return client.Get(new Uri(CASLoginUrl), accept: MediaType.Html)
            .MapError(webError => new AuthError("Failed to read cas login web page", this, webError))
            .Bind<string>(loginWebResponse => client.GetCookie(HNUAuthData.JSESSIONID.KeyName)
                .Tee(cookie => table.UpdateData(HNUAuthData.JSESSIONID, cookie.Value))
                .MapError(webError => new AuthError("Cannot get JSESSIONID from cas login", this, webError))
                .Bind<string>(_ => loginWebResponse.ReadHtml().DocumentNode.SelectSingleNode("//input[@name='execution']").GetAttributeValue("value", "")))
            .Bind(execution => client.Get(new Uri(PubKey), accept: MediaType.Json)
                .Bind(response => response.ReadJsonObject())
                .MapError(webError => new AuthError("Failed to read public key info", this, webError))
                .Bind(pubKeyElement => (pubKeyElement["modulus"]?.GetValue<string?>()?.Ok<string, AuthError>()
                                        ?? new AuthError("Cannot find modulus in pubKey response", this).Fail<string, AuthError>())
                    .Bind(modulus => (pubKeyElement["exponent"]?.GetValue<string?>()?.Ok<string, AuthError>()
                                      ?? new AuthError("Cannot find exponent in pubKey response", this).Fail<string, AuthError>())
                        .Bind(exponent => table.RequireData(CommonDataKey.Password)
                            .Map(password => RsaWebEncryptor.Encrypt(modulus, exponent, new string(password.Reverse().ToArray())))
                            .Bind(encryptedPassword => table.RequireData(CommonDataKey.UserName)
                                .Bind(username => client.GetRedirectedUri(new Uri(CASLoginUrl),
                                    configurator: message =>
                                    {
                                        message.Method = HttpMethod.Post;
                                        message.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                                        {
                                            { "username", username },
                                            { "password", encryptedPassword },
                                            { "authcode", "" },
                                            { "execution", execution },
                                            { "_eventId", "submit" }
                                        });
                                    }).MapError(error => new AuthError("Cannot read login auth redirection url", this, error))
                                    .Bind(response => response.RedirectUri.MapError(error => new AuthError("Cannot read login auth redirection url", this, error)))))))))
            .Tee(url => table.UpdateData(HNUAuthData.CAS_AUTH_REDIRECT_URL, url)).DiscardValue();
    }

    protected override string NodeName => "CasAuth";
} 