namespace CourseRush.Core.Network;

public class WebError(string errorMessage) : BasicError(errorMessage, [])
{
    public static WebError Wrap(Exception exception)
    {
        return new WebError(exception.Message);
    }
}