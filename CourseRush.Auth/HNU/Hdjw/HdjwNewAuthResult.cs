using System.Net;
using Resultful;

namespace CourseRush.Auth.HNU.Hdjw;

public class HdjwNewAuthResult : HdjwAuthResult
{
    public static readonly Func<AuthDataTable, Result<HdjwNewAuthResult, AuthError>> HdjwNew = CreateNewResult;
    private readonly string _bzbJsxsd, _serverId;

    private HdjwNewAuthResult(string bzbJsxsd, string serverId) : base("","","")
    {
        _bzbJsxsd = bzbJsxsd;
        _serverId = serverId;
    }

    private static Result<HdjwNewAuthResult, AuthError> CreateNewResult(AuthDataTable arg)
    {
        return arg.RequireData(HNUAuthData.BZB_JSXSD).Bind(bzbJsxsd =>
            arg.RequireData(HNUAuthData.SERVERID).Map(serverId =>
                new HdjwNewAuthResult(bzbJsxsd, serverId)));
    }

    internal override void InjectAuthInfo(HttpRequestMessage request)
    {
        
    }

    internal override void InjectAuthInfo(HttpClientHandler handler)
    {
        handler.CookieContainer.Add(new Cookie("bzb_jsxsd", _bzbJsxsd, "/", "hdjw.hnu.edu.cn"));
        handler.CookieContainer.Add(new Cookie("SERVERID", _serverId, "/", "hdjw.hnu.edu.cn"));
        handler.CookieContainer.Add(new Cookie("SERVERIDgld", "pc1", "/", "hdjw.hnu.edu.cn"));
    }
}