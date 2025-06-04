using CourseRush.Auth.HNU.Hdjw;

namespace CourseRush.Auth.HNU;

public class HNUAuthChain
{
    public static readonly AuthChain<HdjwAuthResult> HdjwAuth = new HdjwSessionAuthNode(new WebVpnAuthNode(new CASAuthNode())).Terminate(HdjwAuthResult.Hdjw);
    public static readonly AuthChain<HdjwAuthResult> HdjwAuthInternal = new InternalHdjwAuthNode(new InternalCASAuthNode()).Terminate(HdjwAuthResult.Hdjw);
    public static readonly AuthChain<HdjwAuthResult> DebugAuth = new EmptyAuthNode().Terminate(HdjwAuthResult.Debug);
}