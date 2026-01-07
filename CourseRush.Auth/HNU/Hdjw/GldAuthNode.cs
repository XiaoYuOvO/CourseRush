using CourseRush.Core.Network;
using Resultful;

namespace CourseRush.Auth.HNU.Hdjw;

public class GldAuthNode(params AuthNode[] requires) : AuthNode(new AuthConvention().Provides(HNUAuthData.BZB_NJW).Requires(), requires)
{
    private const string GldUrl = "http://hdjw.hnu.edu.cn/gld/";
    internal override VoidResult<AuthError> Auth(AuthDataTable table, WebClient client)
    {
        return client.Get(new Uri(GldUrl), accept: MediaType.Html)
            .Bind(_ => client.GetCookie(HNUAuthData.BZB_NJW.KeyName))
            .MapError(error => new AuthError("Failed to get GLD cookie", this, error))
            .Tee(cookie => table.UpdateData(HNUAuthData.BZB_NJW, cookie.Value)).DiscardValue();
    }

    protected override string NodeName => "Gld";
}