using System.Text.Json;
using ErrorMonitor.Api.Options;
using Microsoft.Extensions.Options;

namespace ErrorMonitor.Api.Services;

public interface IPrivacyService
{
    Dictionary<string, object?> Mask(Dictionary<string, object?>? context);
}

public class PrivacyService(IOptions<PrivacyOptions> options) : IPrivacyService
{
    private readonly PrivacyOptions _options = options.Value;

    public Dictionary<string, object?> Mask(Dictionary<string, object?>? context)
    {
        var sanitized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (context is null) return sanitized;

        foreach (var (key, value) in context)
        {
            if (_options.DisableFieldCapture.Any(x => string.Equals(x, key, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (_options.SensitiveKeys.Any(x => key.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                sanitized[key] = "***";
                continue;
            }

            sanitized[key] = value switch
            {
                JsonElement element => element.ToString(),
                _ => value
            };
        }

        return sanitized;
    }
}
