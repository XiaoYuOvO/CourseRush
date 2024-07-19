using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core.Network;

namespace CourseRush.HNU;

public class HdjwClient
{
    private readonly HdjwAuthResult _authResult;
    private WebClient _client;

    public HdjwClient(HdjwAuthResult authResult)
    {
        _authResult = authResult;
    }
}