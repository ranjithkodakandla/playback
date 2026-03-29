using Microsoft.AspNetCore.RateLimiting;
using ErrorMonitor.Api.Contracts;
using ErrorMonitor.Api.Data;
using ErrorMonitor.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErrorMonitor.Api.Controllers;

[ApiController]
[Route("api/errors")]
public class ErrorsController(ErrorMonitorDbContext dbContext, IIngestionQueue queue, ILogger<ErrorsController> logger) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("ingestion")]
    public async Task<IActionResult> Ingest([FromBody] ErrorIngestRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            await queue.QueueAsync(new IngestionEnvelope(request), ct);
            return Accepted(new { status = "queued" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to queue error event");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "queue_unavailable" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ErrorIssueResponse>>> List(
        [FromQuery] string? browser,
        [FromQuery] string? url,
        [FromQuery] DateTime? startUtc,
        [FromQuery] DateTime? endUtc,
        CancellationToken ct)
    {
        var query = dbContext.ErrorIssues.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(browser))
        {
            query = query.Where(x => x.Browser == browser);
        }

        if (!string.IsNullOrWhiteSpace(url))
        {
            query = query.Where(x => x.Url.Contains(url));
        }

        if (startUtc.HasValue)
        {
            query = query.Where(x => x.LastSeenAtUtc >= startUtc.Value);
        }

        if (endUtc.HasValue)
        {
            query = query.Where(x => x.LastSeenAtUtc <= endUtc.Value);
        }

        var issues = await query
            .OrderByDescending(x => x.LastSeenAtUtc)
            .Take(500)
            .Select(x => new ErrorIssueResponse(
                x.Id,
                x.Message,
                x.StackTrace,
                x.Url,
                x.Browser,
                x.Fingerprint,
                x.OccurrenceCount,
                x.FirstSeenAtUtc,
                x.LastSeenAtUtc,
                x.Release))
            .ToListAsync(ct);

        return Ok(issues);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ErrorIssueDetailResponse>> GetById(Guid id, CancellationToken ct)
    {
        var issue = await dbContext.ErrorIssues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (issue is null) return NotFound();

        var events = await dbContext.ErrorEvents.AsNoTracking()
            .Where(x => x.ErrorIssueId == id)
            .OrderByDescending(x => x.TimestampUtc)
            .Take(200)
            .Select(x => new ErrorEventResponse(
                x.Id,
                x.TimestampUtc,
                x.Browser,
                x.Url,
                x.StackTrace,
                x.UserId,
                x.ContextJson))
            .ToListAsync(ct);

        var timeline = events
            .GroupBy(x => new DateTime(x.TimestampUtc.Year, x.TimestampUtc.Month, x.TimestampUtc.Day, x.TimestampUtc.Hour, 0, 0, DateTimeKind.Utc))
            .OrderBy(x => x.Key)
            .Select(x => new TimelinePointResponse(x.Key, x.Count()))
            .ToList();

        var breakdown = events
            .GroupBy(x => x.Browser)
            .Select(x => new BrowserBreakdownResponse(x.Key, x.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var response = new ErrorIssueDetailResponse(
            new ErrorIssueResponse(
                issue.Id,
                issue.Message,
                issue.StackTrace,
                issue.Url,
                issue.Browser,
                issue.Fingerprint,
                issue.OccurrenceCount,
                issue.FirstSeenAtUtc,
                issue.LastSeenAtUtc,
                issue.Release),
            events,
            timeline,
            breakdown);

        return Ok(response);
    }
}
