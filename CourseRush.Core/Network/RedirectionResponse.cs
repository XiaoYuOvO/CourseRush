using Resultful;

namespace CourseRush.Core.Network;

public class RedirectionResponse : WebResponse
{
    public RedirectionResponse(HttpResponseMessage response, HttpClientHandler handler) : base(response, handler)
    {
    }

    public Result<Uri, WebError> RedirectUri => Response.Headers.Location?.Ok<Uri, WebError>() ?? new WebError("Invalid redirect response message, Location Header is missing");
}