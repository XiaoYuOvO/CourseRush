using System.Net;
using Resultful;

namespace CourseRush.Auth.HNU.Hdjw;

public class HdjwAuthResult : AuthResult
{
    public static readonly Func<AuthDataTable, Result<HdjwAuthResult, AuthError>> Hdjw = CreateResult;
    private readonly string _sdpAppSession, _session, _token, _authcode;
    private HdjwAuthResult(string sdpAppSession,
        string session,
        string token,
        string authcode)
    {
        _sdpAppSession = sdpAppSession;
        _session = session;
        _token = token;
        _authcode = authcode;
    }

    private static Result<HdjwAuthResult, AuthError> CreateResult(AuthDataTable dataTable)
    {
        return dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.SDP_APP_SESSION_80).Bind(appSession =>
            dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.SESSION).Bind(session =>
                dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.TOKEN).Bind(token =>
                    dataTable.RequireData<AuthDataKey<string>, string>(CommonDataKey.UserName).Map(username =>
                        new HdjwAuthResult(appSession, session, token, username)))));
    }

    internal override void InjectAuthInfo(HttpRequestMessage request)
    {
        request.Headers.Add("TOKEN", _token);
    }

    internal override void InjectAuthInfo(HttpClientHandler handler)
    {
        handler.CookieContainer.Add(new Cookie("sdp_app_session-80", _sdpAppSession, "/", "hdjw.hnu.edu.cn"));
        handler.CookieContainer.Add(new Cookie("SESSION", _session, "/", "hdjw.hnu.edu.cn"));
        handler.CookieContainer.Add(new Cookie("authcode", _authcode, "/", "hdjw.hnu.edu.cn"));
    }


    public override string ToString()
    {
        return $"TOKEN: {_token}; SESSION: {_session}; sdp_app_session-80: {_sdpAppSession}; authcode: {_authcode}";
    }
}