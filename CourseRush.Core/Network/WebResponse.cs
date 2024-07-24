using System.Net;
using System.Text.Json.Nodes;
using CourseRush.Core.Util;
using HtmlAgilityPack;
using Resultful;

namespace CourseRush.Core.Network;

public class WebResponse
{
    protected readonly HttpResponseMessage Response;
    private readonly HttpClientHandler _handler;

    public WebResponse(HttpResponseMessage response, HttpClientHandler handler)
    {
        Response = response;
        _handler = handler;
    }

    public Result<JsonObject, WebError> ReadJsonObject()
    {
        return Response.Content.ReadAsStream().Ok<Stream, WebError>().TryBind<Stream, WebError, JsonObject>(
            stream => JsonNode.Parse(stream)?.AsObject() ??
                      new WebError("The response is not a Json object").Fail<JsonObject, WebError>(), WebError.Wrap);
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