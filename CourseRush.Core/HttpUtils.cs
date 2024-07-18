using System.Net;
using System.Text.Json;

namespace CourseRush.Core;

public class HttpUtils
{

    private static readonly HttpClientHandler Handler = new();
    private static readonly HttpClient HttpClient = new(Handler);
    
    private static JsonDocument GetJson(Uri uri)
    {
        var httpRequestMessage = HttpRequestMessage(uri,"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8", "gzip, deflate, br");
        return JsonDocument.Parse(HttpClient.Send(httpRequestMessage).EnsureSuccessStatusCode().Content.ReadAsStream());
    }

    public static string GetString(Uri uri)
    {
        var httpRequestMessage = HttpRequestMessage(uri, "text/html");
        var readAsStringAsync = HttpClient.Send(httpRequestMessage).EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        readAsStringAsync.Wait();
        return readAsStringAsync.Result;
    }

    private static HttpRequestMessage HttpRequestMessage(Uri uri, string accept, string encoding = "br")
    {
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.RequestUri = uri;
        httpRequestMessage.Headers.Host = uri.Host;
        httpRequestMessage.Headers.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:120.0) Gecko/20100101 Firefox/120.0");
        httpRequestMessage.Headers.Add("Accept", accept);
        httpRequestMessage.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
        httpRequestMessage.Headers.Add("Accept-Encoding", encoding);
        return httpRequestMessage;
    }
}