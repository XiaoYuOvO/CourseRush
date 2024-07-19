using System.Net;

namespace CourseRush.Auth.HNU.Hdjw;

public class HdjwAuthResult : AuthResult
{
    public static readonly Func<AuthDataTable, HdjwAuthResult> Hdjw = result => new HdjwAuthResult(result);
    private readonly string _sdpAppSession, _session, _token, _authcode;
    private HdjwAuthResult(AuthDataTable dataTable)
    {
        _sdpAppSession = dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.SDP_APP_SESSION_80);
        _session = dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.SESSION);
        _token = dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.TOKEN);
        _authcode = dataTable.RequireData<AuthDataKey<string>, string>(CommonDataKey.UserName);
    }

    internal override void InjectAuthInfo(HttpRequestMessage request)
    {
        request.Headers.Add("TOKEN", _token);
    }

    internal override void InjectAuthInfo(HttpClientHandler handler)
    {
        handler.CookieContainer.Add(new Cookie("sdp_app_session-80", _sdpAppSession));
        handler.CookieContainer.Add(new Cookie("SESSION", _session));
        handler.CookieContainer.Add(new Cookie("authcode", _authcode));
    }


    public override string ToString()
    {
        return $"TOKEN: {_token}; SESSION: {_session}; sdp_app_session-80: {_sdpAppSession}; authcode: {_authcode}";
    }
}