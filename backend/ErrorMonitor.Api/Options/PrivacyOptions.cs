namespace ErrorMonitor.Api.Options;

public class PrivacyOptions
{
    public List<string> SensitiveKeys { get; set; } = [];
    public List<string> DisableFieldCapture { get; set; } = [];
}
