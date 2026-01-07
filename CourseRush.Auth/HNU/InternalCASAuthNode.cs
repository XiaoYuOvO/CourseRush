namespace CourseRush.Auth.HNU;

public class InternalCASAuthNode(params AuthNode[] requires) : CASAuthNode(requires)
{
    protected override string CASLoginUrl => "http://cas.hnu.edu.cn/cas/login?service=http%3A%2F%2Fhdjw.hnu.edu.cn%2Fgld%2Fsso.jsp";

    protected override string NodeName => "InternalCASAuthNode";
}