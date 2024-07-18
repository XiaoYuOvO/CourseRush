using System.Net;

namespace CourseRush.Auth.HNU.Hdjw;

public class HdjwAuthResult : AuthResult
{
    public static readonly Func<AuthDataTable, HdjwAuthResult> Hdjw = result => new HdjwAuthResult(result);
    private readonly string _sdpUserToken, _session, _token;
    private HdjwAuthResult(AuthDataTable dataTable)
    {
        _sdpUserToken = dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.SDP_USER_TOKEN);
        _session = dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.SESSION);
        _token = dataTable.RequireData<AuthDataKey<string>, string>(HNUAuthData.TOKEN);
    }

    public override void InjectAuthInfo(HttpWebRequest request)
    {
        request.Headers.Add("token", _token);
        request.CookieContainer?.Add(new Cookie("sdp_user_token", _sdpUserToken));
        request.CookieContainer?.Add(new Cookie("SESSION", _session));
    }

    public override string ToString()
    {
        return $"Token: {_token}; Session: {_session}; SdpUserToken: {_sdpUserToken}";
    }
}