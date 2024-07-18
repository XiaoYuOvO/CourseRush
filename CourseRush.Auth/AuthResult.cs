using System.Net;

namespace CourseRush.Auth;

public abstract class AuthResult
{
    public abstract void InjectAuthInfo(HttpWebRequest request);
}