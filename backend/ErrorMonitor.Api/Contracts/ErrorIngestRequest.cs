using System.ComponentModel.DataAnnotations;

namespace ErrorMonitor.Api.Contracts;

public class ErrorIngestRequest
{
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(10000)]
    public string? StackTrace { get; set; }

    [MaxLength(2048)]
    public string? Url { get; set; }

    [MaxLength(256)]
    public string? UserAgent { get; set; }

    public DateTime? TimestampUtc { get; set; }

    [MaxLength(128)]
    public string? UserId { get; set; }

    [MaxLength(128)]
    public string? Release { get; set; }

    public Dictionary<string, string>? Tags { get; set; }

    public List<BreadcrumbDto>? Breadcrumbs { get; set; }

    public Dictionary<string, object?>? Context { get; set; }
}

public class BreadcrumbDto
{
    [MaxLength(50)]
    public string Type { get; set; } = "log";

    [MaxLength(400)]
    public string Message { get; set; } = string.Empty;

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
