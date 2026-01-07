using System.Net;
using Resultful;

namespace CourseRush.Auth.HNU.Hdjw;

public class HdjwAuthResult : AuthResult
{
    public static readonly Func<AuthDataTable, Result<HdjwAuthResult, AuthError>> Hdjw = CreateResult;
    public static readonly Func<AuthDataTable, Result<HdjwAuthResult, AuthError>> Debug = CreateDebugResult;
    private readonly string _session, _token, _authcode;

    protected HdjwAuthResult(
        string session,
        string token,
        string authcode)
    {
        _session = session;
        _token = token;
        _authcode = authcode;
    }

    private static Result<HdjwAuthResult, AuthError> CreateResult(AuthDataTable dataTable)
    {
        return dataTable.RequireData(HNUAuthData.SESSION).Bind(session =>
                dataTable.RequireData(HNUAuthData.TOKEN).Bind(token =>
                    dataTable.RequireData(CommonDataKey.UserName).Map(username =>
                        new HdjwAuthResult(session, token, username))));
    }


    private static Result<HdjwAuthResult, AuthError> CreateDebugResult(AuthDataTable dataTable)
    {
        return new HdjwAuthResult("","","");
    }

    internal override void InjectAuthInfo(HttpRequestMessage request)
    {
        request.Headers.Add("TOKEN", _token);
    }

    internal override void InjectAuthInfo(HttpClientHandler handler)
    {
        // handler.CookieContainer.Add(new Cookie("sdp_app_session-80", _sdpAppSession, "/", "hdjw.hnu.edu.cn"));
        handler.CookieContainer.Add(new Cookie("SESSION", _session, "/", "hdjw.hnu.edu.cn"));
        handler.CookieContainer.Add(new Cookie("authcode", _authcode, "/", "hdjw.hnu.edu.cn"));
    }


    public override string ToString()
    {
        return $"TOKEN: {_token}; SESSION: {_session}; authcode: {_authcode}";
    }
}