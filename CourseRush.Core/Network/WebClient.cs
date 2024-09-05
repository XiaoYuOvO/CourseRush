using System.Net;
using Resultful;

namespace CourseRush.Core.Network;

public class WebClient
{
    protected readonly HttpClientHandler Handler = new();
    private readonly HttpClient _client;
    public delegate void RequestConfigurator(HttpRequestMessage request);
    public WebClient()
    {
        // _handler.Proxy = new WebProxy("127.0.0.1",8888);
        Handler.UseProxy = true;
        Handler.AutomaticDecompression = DecompressionMethods.All;
        Handler.UseCookies = true;
        Handler.CheckCertificateRevocationList = false;
        Handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        Handler.AllowAutoRedirect = false;
        _client = new HttpClient(Handler);
    }

    protected Result<WebResponse, WebError> EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return new WebResponse(response, Handler);
        }
        return new WebError($"Web request not success: Status={response.StatusCode}, Phrase={response.ReasonPhrase}");
    }

    public Result<RedirectionResponse, WebError> GetRedirectedUri(Uri uri,MediaType accept = MediaType.All, RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = CreateRequest(uri, accept, configurator:configurator);
        var httpResponseMessage = _client.Send(httpRequestMessage);
        if (httpResponseMessage.StatusCode == HttpStatusCode.Redirect)
        {
            return new RedirectionResponse(httpResponseMessage, Handler);
        }
        var readAsStringAsync = httpResponseMessage.Content.ReadAsStringAsync();
        readAsStringAsync.Wait();
        return new WebError($"The result of request is not a redirect response: {readAsStringAsync.Result}");
    }

    public Result<WebResponse, WebError> Get(Uri uri, Dictionary<string, string> content,MediaType accept = MediaType.All, RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = CreateRequest(uri, accept, configurator:configurator);
        httpRequestMessage.Content = new FormUrlEncodedContent(content);
        return EnsureSuccess(_client.Send(httpRequestMessage));
    }
    
    public Result<WebResponse, WebError> Get(Uri uri, HttpContent? content = null, MediaType accept = MediaType.All, RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = CreateRequest(uri, accept, configurator:configurator);
        httpRequestMessage.Content = content;
        return EnsureSuccess(_client.Send(httpRequestMessage));
    }

    public Result<WebResponse, WebError> Post(Uri uri, HttpContent? content = null, MediaType accept = MediaType.All, RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = CreateRequest(uri, accept, configurator:configurator);
        httpRequestMessage.Method = HttpMethod.Post;
        httpRequestMessage.Content = content;
        return EnsureSuccess(_client.Send(httpRequestMessage));
    }
    
    public Result<WebResponse, WebError> Post(Uri uri, Dictionary<string, string> content, MediaType accept = MediaType.All, RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = CreateRequest(uri, accept, configurator:configurator);
        httpRequestMessage.Method = HttpMethod.Post;
        httpRequestMessage.Content = new FormUrlEncodedContent(content);
        return EnsureSuccess(_client.Send(httpRequestMessage));
    }

    public Result<Cookie, WebError> GetCookie(string name)
    {
        return Handler.CookieContainer.GetAllCookies()[name]?.Ok<Cookie, WebError>() ?? new WebError($"Cannot find cookie {name} in current cookie container");
    }

    protected virtual HttpRequestMessage CreateRequest(Uri uri, MediaType accept, string encoding = "gzip, deflate, br", RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.RequestUri = uri;
        httpRequestMessage.Headers.Host = uri.Host;
        httpRequestMessage.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");
        httpRequestMessage.Headers.Add("Accept", accept.ToHeaderString());
        httpRequestMessage.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
        httpRequestMessage.Headers.Add("Accept-Encoding", encoding);
        configurator?.Invoke(httpRequestMessage);
        return httpRequestMessage;
    }
}

public enum MediaType
{
    All,
    Html,
    Json
}

public static class MediaTypeExtensions
{
    internal static string ToHeaderString(this MediaType mediaType)
    {
        return mediaType switch
        {
            MediaType.All => "application/json, text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/png,image/svg+xml,*/*;q=0.8",
            MediaType.Html => "text/html",
            MediaType.Json => "application/json",
            _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null)
        };
    }
}

public static class ConfiguratorExtensions
{
    public static WebClient.RequestConfigurator And(this WebClient.RequestConfigurator configurator, WebClient.RequestConfigurator other)
    {
        return request =>
        {
            configurator.Invoke(request);
            other.Invoke(request);
        };
    }
}