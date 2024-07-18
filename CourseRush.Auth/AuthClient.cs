using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using HtmlAgilityPack;

namespace CourseRush.Auth;

public class AuthClient
{
    private readonly HttpClientHandler _handler = new();
    private readonly HttpClient _client;

    private const string AcceptAll =
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/png,image/svg+xml,*/*;q=0.8";
    public delegate void RequestConfigurator(HttpRequestMessage request);
    public AuthClient()
    {
        // _handler.Proxy = new WebProxy("127.0.0.1",8888);
        _handler.UseProxy = true;
        _handler.AutomaticDecompression = DecompressionMethods.All;
        _handler.UseCookies = true;
        _handler.CheckCertificateRevocationList = false;
        _handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        _handler.AllowAutoRedirect = false;
        _client = new HttpClient(_handler);
    }
    
    public AuthResponse GetAny(Uri uri,RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = HttpRequestMessage(uri, AcceptAll, "gzip, deflate, br", configurator);
        return MakeResponse(_client.Send(httpRequestMessage).EnsureSuccessStatusCode());
    }

    private AuthResponse MakeResponse(HttpResponseMessage message)
    {
        return new AuthResponse(message, _handler);
    }

    public AuthResponse GetLiteral(Uri uri,RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = HttpRequestMessage(uri, "text/html",configurator:configurator);
        return MakeResponse(_client.Send(httpRequestMessage).EnsureSuccessStatusCode());
    }

    public RedirectedAuthResponse GetRedirectedUri(Uri uri, RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = HttpRequestMessage(uri, AcceptAll, configurator:configurator);
        var httpResponseMessage = _client.Send(httpRequestMessage);
        if (httpResponseMessage.StatusCode == HttpStatusCode.Redirect)
        {
            return new RedirectedAuthResponse(httpResponseMessage, _handler);
        }
        throw new InvalidDataException("The result of request is not a redirect response");
    }

    public AuthResponse GetWithWebForm(Uri uri, string content, RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = HttpRequestMessage(uri, "text/html", configurator:configurator);
        httpRequestMessage.Content = new StringContent(content);
        httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        return MakeResponse(_client.Send(httpRequestMessage).EnsureSuccessStatusCode());
    }

    public AuthResponse Post(Uri uri, string? content = null, RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = HttpRequestMessage(uri, AcceptAll, configurator:configurator);
        httpRequestMessage.Method = HttpMethod.Post;
        if (content != null)
        {
            httpRequestMessage.Content = new StringContent(content);
            httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }
        return MakeResponse(_client.Send(httpRequestMessage).EnsureSuccessStatusCode());
    }

    public Cookie? GetCookie(string name)
    {
        return _handler.CookieContainer.GetAllCookies()[name];
    }

    private static HttpRequestMessage HttpRequestMessage(Uri uri, string accept, string encoding = "br", RequestConfigurator? configurator = null)
    {
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.RequestUri = uri;
        httpRequestMessage.Headers.Host = uri.Host;
        httpRequestMessage.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");
        httpRequestMessage.Headers.Add("Accept", accept);
        httpRequestMessage.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
        httpRequestMessage.Headers.Add("Accept-Encoding", encoding);
        configurator?.Invoke(httpRequestMessage);
        return httpRequestMessage;
    }
}