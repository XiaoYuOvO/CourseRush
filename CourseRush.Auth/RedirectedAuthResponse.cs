namespace CourseRush.Auth;

public class RedirectedAuthResponse : AuthResponse
{
    public RedirectedAuthResponse(HttpResponseMessage response, HttpClientHandler handler) : base(response, handler)
    {
    }

    public static implicit operator Uri(RedirectedAuthResponse response) => response.RedirectUri;

    private Uri RedirectUri => Response.Headers.Location ?? throw new InvalidOperationException("Invalid redirect response message, Location Header is missing");
}