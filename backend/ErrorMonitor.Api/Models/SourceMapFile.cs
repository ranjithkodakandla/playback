using System.ComponentModel.DataAnnotations;

namespace ErrorMonitor.Api.Models;

public class SourceMapFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(128)]
    public string Release { get; set; } = string.Empty;

    [Required]
    [MaxLength(2048)]
    public string MinifiedFileUrl { get; set; } = string.Empty;

    [Required]
    public string SourceMapJson { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
