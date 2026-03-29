using System.ComponentModel.DataAnnotations;

namespace ErrorMonitor.Api.Models;

public class ErrorEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ErrorIssueId { get; set; }

    public ErrorIssue? ErrorIssue { get; set; }

    [MaxLength(10000)]
    public string StackTrace { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Browser { get; set; } = string.Empty;

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(128)]
    public string? UserId { get; set; }

    [MaxLength(128)]
    public string? Release { get; set; }

    [MaxLength(4000)]
    public string ContextJson { get; set; } = "{}";
}
