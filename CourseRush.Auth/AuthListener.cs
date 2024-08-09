namespace CourseRush.Auth;

public interface IAuthListener
{
    void OnAuthNode();
    void OnAuthInfo();
    void OnAuthError();
}
