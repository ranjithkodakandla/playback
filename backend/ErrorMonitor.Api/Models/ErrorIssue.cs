using System.ComponentModel.DataAnnotations;

namespace ErrorMonitor.Api.Models;

public class ErrorIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(10000)]
    public string StackTrace { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Browser { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Fingerprint { get; set; } = string.Empty;

    public int OccurrenceCount { get; set; } = 1;

    public DateTime FirstSeenAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(128)]
    public string? Release { get; set; }

    public ICollection<ErrorEvent> Events { get; set; } = new List<ErrorEvent>();
}
