using System.Net;

namespace CourseRush.Auth.HNU.Hdjw;

public class HdjwAuthResult : AuthResult
{
    public static readonly Func<AuthDataTable, HdjwAuthResult> Hdjw = result => new HdjwAuthResult(result);
    private readonly string _sdpAppSession, _session, _token;
    private HdjwAuthResult(AuthDataTable dataTable)
    {
        _sdpAppSession = dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.SDP_APP_SESSION_80);
        _session = dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.SESSION);
        _token = dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.TOKEN);
    }

    public override void InjectAuthInfo(HttpWebRequest request)
    {
        request.Headers.Add("TOKEN", _token);
        request.CookieContainer?.Add(new Cookie("sdp_app_session-80", _sdpAppSession));
        request.CookieContainer?.Add(new Cookie("SESSION", _session));
    }

    public override string ToString()
    {
        return $"Token: {_token}; Session: {_session}; sdp_app_session-80: {_sdpAppSession}";
    }
}