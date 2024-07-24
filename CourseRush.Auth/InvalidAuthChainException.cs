namespace CourseRush.Auth;

public class InvalidAuthChainException : AuthException
{
    public InvalidAuthChainException(string message) : base(message)
    {
    }
}