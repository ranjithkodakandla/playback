namespace ErrorMonitor.Api.Services;

public interface IBrowserParser
{
    string Parse(string? userAgent);
}

public class BrowserParser : IBrowserParser
{
    public string Parse(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";

        var ua = userAgent.ToLowerInvariant();
        if (ua.Contains("edg/")) return "Edge";
        if (ua.Contains("chrome/")) return "Chrome";
        if (ua.Contains("firefox/")) return "Firefox";
        if (ua.Contains("safari/") && !ua.Contains("chrome/")) return "Safari";
        return "Other";
    }
}
