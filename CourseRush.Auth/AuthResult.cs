namespace CourseRush.Auth;

public abstract class AuthResult
{
    internal abstract void InjectAuthInfo(HttpRequestMessage request);

    internal abstract void InjectAuthInfo(HttpClientHandler handler);
}