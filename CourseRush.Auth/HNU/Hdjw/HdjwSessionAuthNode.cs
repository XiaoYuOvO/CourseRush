using System.Text.Json.Nodes;
using JWT;
using JWT.Serializers;
using Resultful;
using WebClient = CourseRush.Core.Network.WebClient;

namespace CourseRush.Auth.HNU.Hdjw;
using static HNUAuthData;
public class HdjwSessionAuthNode(params AuthNode[] requires) : AuthNode(new AuthConvention()
    .Requires(PC0, PF0, PV0, SID, SID_SIG, SID_LEGACY, SID_LEGACY_SIG)
    .Provides(SESSION, SDP_APP_SESSION_80, TOKEN), requires)
{
    private const string HdjwIndex = "http://hdjw.hnu.edu.cn/";

    private const string CasLoginHdjw = "http://cas.hnu.edu.cn/cas/login?service=http%3A%2F%2Fhdjw.hnu.edu.cn%2Fgld%2Fsso.jsp";

    internal override VoidResult<AuthError> Auth(AuthDataTable table, WebClient client)
    {
        return client.Get(new Uri(HdjwIndex))
            .Map(response => response.ReadString())
            .Bind(indexResponse =>
            {
                var startIndex = indexResponse.IndexOf("https://", StringComparison.Ordinal);
                var length = indexResponse.LastIndexOf("\");", StringComparison.Ordinal) - startIndex;
                var redirectedUri = indexResponse.Substring(startIndex, length);
                return client.GetRedirectedUri(new Uri(redirectedUri))
                    .Bind(response => response.RedirectUri)
                    .Bind(uri => client.Get(uri));
            }).MapError(error => new AuthError("Failed to read hdjw index page", this, error))
            .Bind(_ => client.GetCookie("sdp_app_session-80")
                .MapError(error => new AuthError("SDP app session is missing after verification", this, error))
                .Map(cookie => cookie.Value))
            .Tee(sdpAppSession => Console.WriteLine($"SDP app session: {sdpAppSession}"))
            .Bind(sdpAppSession => client.GetRedirectedUri(new Uri(CasLoginHdjw))
                .Bind(response => response.RedirectUri
                    .Bind(uri => client.GetRedirectedUri(uri)))
                .MapError(error => new AuthError("Cannot find redirection url in cas login", this, error))
                .Bind(_ => client.GetCookie("token")
                    .MapError(error => new AuthError("Token is missing after hdjw cas login", this, error))
                    .Map(cookie => cookie.Value)
                    .Bind(token => (JsonNode.Parse(
                                        new JwtDecoder(new DefaultJsonSerializerFactory().Create(), new JwtBase64UrlEncoder())
                                            .Decode(token, false))?["sid"]?.GetValue<string?>()?.Ok<string, AuthError>()
                                    ?? new AuthError($"The token jwt doesn't contains the session id {token}", this)
                                        .Fail<string, AuthError>())
                        .Tee(decodedSession =>
                        {
                            table.UpdateData(SDP_APP_SESSION_80, sdpAppSession);
                            table.UpdateData(SESSION, decodedSession);
                            table.UpdateData(TOKEN, token);
                        })))).DiscardValue();
    }

    protected override string NodeName => "HdjwCasLogin";
}