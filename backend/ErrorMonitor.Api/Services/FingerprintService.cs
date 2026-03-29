using System.Security.Cryptography;
using System.Text;

namespace ErrorMonitor.Api.Services;

public interface IFingerprintService
{
    string Generate(string message, string stackTrace);
}

public class FingerprintService : IFingerprintService
{
    public string Generate(string message, string stackTrace)
    {
        var normalized = $"{message.Trim().ToLowerInvariant()}::{NormalizeStack(stackTrace)}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string NormalizeStack(string stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace)) return string.Empty;
        return string.Join('\n', stackTrace
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(6)
            .Select(x => x.ToLowerInvariant()));
    }
}
