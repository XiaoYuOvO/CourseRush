using System.Net;
using System.Text.Json.Nodes;
using HtmlAgilityPack;

namespace CourseRush.Auth;

public class AuthResponse
{
    protected readonly HttpResponseMessage Response;
    private readonly HttpClientHandler _handler;

    public AuthResponse(HttpResponseMessage response, HttpClientHandler handler)
    {
        Response = response;
        _handler = handler;
    }

    public JsonObject ReadJsonObject()
    {
        return JsonNode.Parse(Response.Content.ReadAsStream())?.AsObject() ?? throw new InvalidDataException("The response is not a JSON object");
    }

    public HtmlDocument ReadHtml()
    {
        var str = Response.Content.ReadAsStringAsync();
        var htmlDocument = new HtmlDocument();
        str.Wait();
        htmlDocument.LoadHtml(str.Result);
        return htmlDocument;
    }

    public string ReadString()
    {
        var readAsStringAsync = Response.Content.ReadAsStringAsync();
        readAsStringAsync.Wait();
        return readAsStringAsync.Result;
    }

    public HttpStatusCode GetStatusCode()
    {
        return Response.StatusCode;
    }

    public CookieCollection GetCurrentCookies()
    {
        return _handler.CookieContainer.GetAllCookies();
    }
}