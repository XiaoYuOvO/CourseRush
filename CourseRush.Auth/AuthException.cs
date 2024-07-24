namespace CourseRush.Auth;

public class AuthException : ApplicationException
{
    internal AuthException(string message) : base(message)
    {
    }
}