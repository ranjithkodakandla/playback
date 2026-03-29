using System.Text.Json;
using ErrorMonitor.Api.Data;
using ErrorMonitor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErrorMonitor.Api.Services;

public class ErrorIngestionWorker(
    IServiceProvider serviceProvider,
    IIngestionQueue queue,
    ILogger<ErrorIngestionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IngestionEnvelope envelope;
            try
            {
                envelope = await queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ErrorMonitorDbContext>();
                var fingerprintService = scope.ServiceProvider.GetRequiredService<IFingerprintService>();
                var parser = scope.ServiceProvider.GetRequiredService<IBrowserParser>();
                var privacy = scope.ServiceProvider.GetRequiredService<IPrivacyService>();
                var sourceMap = scope.ServiceProvider.GetRequiredService<ISourceMapService>();

                var payload = envelope.Payload;
                var stackTrace = payload.StackTrace ?? string.Empty;
                stackTrace = await sourceMap.DecodeStackTraceAsync(stackTrace, payload.Release, payload.Url, stoppingToken);
                var fingerprint = fingerprintService.Generate(payload.Message, stackTrace);

                var issue = await db.ErrorIssues.FirstOrDefaultAsync(x => x.Fingerprint == fingerprint, stoppingToken);
                var now = payload.TimestampUtc ?? DateTime.UtcNow;
                var browser = parser.Parse(payload.UserAgent);

                if (issue is null)
                {
                    issue = new ErrorIssue
                    {
                        Message = payload.Message,
                        StackTrace = stackTrace,
                        Url = payload.Url ?? string.Empty,
                        Browser = browser,
                        Fingerprint = fingerprint,
                        FirstSeenAtUtc = now,
                        LastSeenAtUtc = now,
                        OccurrenceCount = 1,
                        Release = payload.Release
                    };
                    db.ErrorIssues.Add(issue);
                }
                else
                {
                    issue.OccurrenceCount += 1;
                    issue.LastSeenAtUtc = now;
                }

                var context = privacy.Mask(payload.Context);

                db.ErrorEvents.Add(new ErrorEvent
                {
                    ErrorIssue = issue,
                    TimestampUtc = now,
                    Browser = browser,
                    Url = payload.Url ?? string.Empty,
                    StackTrace = stackTrace,
                    UserId = payload.UserId,
                    Release = payload.Release,
                    ContextJson = JsonSerializer.Serialize(new
                    {
                        context,
                        tags = payload.Tags,
                        breadcrumbs = payload.Breadcrumbs
                    })
                });

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process error event.");
            }
        }
    }
}
