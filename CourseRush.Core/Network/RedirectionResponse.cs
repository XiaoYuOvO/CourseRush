namespace CourseRush.Core.Network;

public class RedirectionResponse : WebResponse
{
    public RedirectionResponse(HttpResponseMessage response, HttpClientHandler handler) : base(response, handler)
    {
    }

    public static implicit operator Uri(RedirectionResponse response) => response.RedirectUri;

    private Uri RedirectUri => Response.Headers.Location ?? throw new InvalidOperationException("Invalid redirect response message, Location Header is missing");
}