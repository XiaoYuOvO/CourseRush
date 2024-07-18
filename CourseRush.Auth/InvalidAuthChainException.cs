namespace CourseRush.Auth;

public class InvalidAuthChainException : Exception
{
    public InvalidAuthChainException(string message) : base(message)
    {
    }
}