namespace CourseRush.Auth.HNU;

public class InternalCASAuthNode : CASAuthNode
{
    protected override string CASLoginUrl => "https://cas.hnu.edu.cn/cas/login?service=http://hdjw.hnu.edu.cn/caslogin?redirect_url=/Njw2017/index.html";

    protected override string NodeName => "InternalCASAuthNode";
}