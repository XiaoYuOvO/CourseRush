using CourseRush.Auth.HNU.Hdjw;

namespace CourseRush.Auth.HNU;

public class HNUAuthChain
{
    public static readonly AuthChain<HdjwAuthResult> HdjwAuth = new HdjwSessionAuthNode(new WebVpnAuthNode(new CASAuthNode(new GldAuthNode()))).Terminate(HdjwAuthResult.Hdjw);
    public static readonly AuthChain<HdjwNewAuthResult> HdjwAuthInternal = new InternalHdjwAuthNode(new InternalCASAuthNode(new GldAuthNode())).Terminate(HdjwNewAuthResult.HdjwNew);
    public static readonly AuthChain<HdjwAuthResult> DebugAuth = new EmptyAuthNode().Terminate(HdjwAuthResult.Debug);
}