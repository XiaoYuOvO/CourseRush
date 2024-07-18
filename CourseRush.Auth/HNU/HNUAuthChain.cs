using CourseRush.Auth.HNU.Hdjw;

namespace CourseRush.Auth.HNU;

public class HNUAuthChain
{
    public static AuthChain<HdjwAuthResult> HdjwAuth = new HdjwSessionAuthNode(new WebVpnAuthNode(new CASAuthNode())).Terminate(HdjwAuthResult.Hdjw);
}