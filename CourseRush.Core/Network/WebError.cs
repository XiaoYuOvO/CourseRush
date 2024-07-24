namespace CourseRush.Core.Network;

public class WebError : BasicError
{
    public WebError(string errorMessage) : base(errorMessage) { }

    public static WebError Wrap(Exception exception)
    {
        return new WebError(exception.Message);
    }
}