using CourseRush.Auth.Crypto;
using CourseRush.Core.Network;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Auth.HNU;

public class CASAuthNode(params AuthNode[] requires) : AuthNode(
    new AuthConvention().Requires(CommonDataKey.UserName, CommonDataKey.Password, HNUAuthData.BZB_NJW).Provides(HNUAuthData.PC0,
        HNUAuthData.PF0, HNUAuthData.PV0, HNUAuthData.JSESSIONID, HNUAuthData.CAS_AUTH_REDIRECT_URL), requires)
{
    private const string PubKey = "http://cas.hnu.edu.cn/cas/v2/getPubKey?sf_request_type=ajax";

    protected virtual string CASLoginUrl => "http://cas.web.hnu.edu.cn/cas/login?service=http%3A%2F%2Fhdjw.hnu.edu.cn%2Fgld%2Fsso.jsp";
    private const string SendSmsUrl = "http://cas.hnu.edu.cn/cas/syz/services/sedsms?reloginType=reloginPhone";
    internal override Task<VoidResult<AuthError>> Auth(AuthDataTable table, WebClient client)
    {
        return client.Get(new Uri(CASLoginUrl), accept: MediaType.Html)
            .MapError(webError => new AuthError("Failed to read cas login web page", this, webError))
            .Bind<string>(loginWebResponse => client.GetCookie(HNUAuthData.JSESSIONID.KeyName)
                .Tee(cookie => table.UpdateData(HNUAuthData.JSESSIONID, cookie.Value))
                .MapError(webError => new AuthError("Cannot get JSESSIONID from cas login", this, webError))
                .Bind<string>(_ => loginWebResponse.ReadHtml().DocumentNode.SelectSingleNode("//input[@name='execution']").GetAttributeValue("value", "")))
            .BindAsync(execution => client.Get(new Uri(PubKey), accept: MediaType.Json)
                .Bind(response => response.ReadJsonObject())
                .MapError(webError => new AuthError("Failed to read public key info", this, webError))
                .BindAsync(pubKeyElement => (pubKeyElement["modulus"]?.GetValue<string?>()?.Ok<string, AuthError>()
                                        ?? new AuthError("Cannot find modulus in pubKey response", this).Fail<string, AuthError>())
                    .BindAsync(modulus => (pubKeyElement["exponent"]?.GetValue<string?>()?.Ok<string, AuthError>()
                                      ?? new AuthError("Cannot find exponent in pubKey response", this).Fail<string, AuthError>())
                        .BindAsync(exponent => table.RequireData(CommonDataKey.Password)
                            .Map(password => RsaWebEncryptor.Encrypt(modulus, exponent, new string(password.Reverse().ToArray())))
                            .BindAsync(encryptedPassword => table.RequireData(CommonDataKey.UserName)
                                .BindAsync(username => client.GetRedirectedUriOrNormal(new Uri(CASLoginUrl),
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
                                    }).MapError(error => new AuthError("Cannot post login auth data", this, error))
                                    .BindAsync(response => 
                                        response.Match(
                                            redirectionResponse => Task.FromResult(redirectionResponse.RedirectUri.MapError(error => new AuthError("Cannot read login auth redirection url", this, error))),
                                            webResponse => ParseTwoFactorAuthOrAuthError(webResponse, client, table))
                                            )))))))
            .Tee(url => table.UpdateData(HNUAuthData.CAS_AUTH_REDIRECT_URL, url)).DiscardValue();
    }

    private const string SmsReloginUrl = "http://cas.hnu.edu.cn/cas/login";
    private const string AllowLoginTime = "allowLoginTime = '";
    private async Task<Result<Uri, AuthError>> ParseTwoFactorAuthOrAuthError(WebResponse loginResponse, WebClient client, AuthDataTable table)
    {
        var htmlDocument = loginResponse.ReadHtml();
        var msgElement = htmlDocument.GetElementbyId("msg");
        if (msgElement != null) return  new AuthError(msgElement.InnerText);
        //403 QPM reached
        if (htmlDocument.Text.Contains("allowLoginTime"))
        {
            var substr = htmlDocument.Text[htmlDocument.Text.IndexOf(AllowLoginTime, StringComparison.Ordinal)..];
            return new AuthError($"403 Forbidden, Next request available on: {substr[AllowLoginTime.Length..substr.IndexOf("';\r\n", StringComparison.Ordinal)]}", this);
        }
        //2FA required
        var phone = htmlDocument.GetElementbyId("phone").GetAttributeValue("value", "");
        var smsResult = await table.Interactive.RequestActionWithPayload(
            new NamedAction(
                () => client.Get(new Uri(SendSmsUrl)).Tee(response =>
                    table.Interactive.ShowInfo(TranslatableText.Of("ui.message.sms_send_status",response.ReadString()))), "ui.button.send_sms"), 
            "ui.label.2fa_title",
            "ui.label.sms_2fa_tip");
        var execution = htmlDocument.DocumentNode.SelectSingleNode("//input[@name='execution']")
            .GetAttributeValue("value", "");
        if ((phone is null or "") || (execution is null or ""))
            return new AuthError($"Invalid phone number: {phone}, or empty execution: {execution}");
        //Post recode
        return smsResult.Bind(smsCode => client.GetRedirectedUri(new Uri(SmsReloginUrl), configurator: request =>
            {
                request.Method = HttpMethod.Post;
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "username", phone },
                    { "recode", smsCode },
                    { "reloginType", "reloginPhone" },
                    { "execution", execution },
                    { "_eventId", "submit" }
                });
            })
            .Bind(response => response.RedirectUri).MapError(error =>
                new AuthError("Cannot read login auth redirection url", this, error)));
    }

    protected override string NodeName => "CasAuth";
} 