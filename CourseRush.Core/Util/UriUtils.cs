namespace CourseRush.Core.Util;

public static class UriUtils
{
    public static Uri ApplyWebForms(this Uri uri, Dictionary<string, string> webForms)
    {
        if (uri.Query.Length != 0)
        {
            return new Uri(uri.GetComponents(UriComponents.Path | UriComponents.SchemeAndServer, UriFormat.UriEscaped) +
                           uri.Query +
                           
                           webForms.Aggregate("&", (current, kvp) => current + $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}&")
                               .TrimEnd('&'));
        }

        return new Uri(uri.GetComponents(UriComponents.Path | UriComponents.SchemeAndServer, UriFormat.UriEscaped) +
                       webForms.Aggregate("?", (current, kvp) => current + $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}&")
                           .TrimEnd('&'));
    }
}