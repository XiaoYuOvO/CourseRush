using CourseRush.Auth.HNU.Hdjw;

namespace CourseRush.Auth.HNU;

public class HNUAuthChain
{
    public static AuthChain<HdjwAuthResult> HdjwAuth = new HdjwSessionAuthNode(new WebVpnAuthNode(new CASAuthNode())).Terminate(HdjwAuthResult.Hdjw);
    public static AuthChain<HdjwAuthResult> HdjwAuthInternal = new InternalHdjwAuthNode(new InternalCASAuthNode()).Terminate(HdjwAuthResult.Hdjw);
    
    public static AuthChain<HdjwAuthResult> DebugAuth = new EmptyAuthNode().Terminate(HdjwAuthResult.Debug);
}