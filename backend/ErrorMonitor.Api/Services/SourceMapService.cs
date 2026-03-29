using ErrorMonitor.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ErrorMonitor.Api.Services;

public interface ISourceMapService
{
    Task<string> DecodeStackTraceAsync(string stackTrace, string? release, string? minifiedUrl, CancellationToken ct);
}

public class SourceMapService(ErrorMonitorDbContext dbContext, ILogger<SourceMapService> logger) : ISourceMapService
{
    public async Task<string> DecodeStackTraceAsync(string stackTrace, string? release, string? minifiedUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(stackTrace) || string.IsNullOrWhiteSpace(release) || string.IsNullOrWhiteSpace(minifiedUrl))
        {
            return stackTrace;
        }

        var sourceMap = await dbContext.SourceMaps
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Release == release && x.MinifiedFileUrl == minifiedUrl, ct);

        if (sourceMap is null)
        {
            return stackTrace;
        }

        // MVP behavior: annotate stack with source map availability.
        // This keeps pipeline ready for full VLQ-based decoding in later versions.
        logger.LogDebug("Source map found for {Release} {Url}", release, minifiedUrl);
        return $"[source-map:{release}]\n{stackTrace}";
    }
}
