using ErrorMonitor.Api.Contracts;
using ErrorMonitor.Api.Data;
using ErrorMonitor.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErrorMonitor.Api.Controllers;

[ApiController]
[Route("api/sourcemaps")]
public class SourceMapsController(ErrorMonitorDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] SourceMapUploadRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var existing = await dbContext.SourceMaps
            .FirstOrDefaultAsync(x => x.Release == request.Release && x.MinifiedFileUrl == request.MinifiedFileUrl, ct);

        if (existing is null)
        {
            dbContext.SourceMaps.Add(new SourceMapFile
            {
                Release = request.Release,
                MinifiedFileUrl = request.MinifiedFileUrl,
                SourceMapJson = request.SourceMapJson,
                UploadedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.SourceMapJson = request.SourceMapJson;
            existing.UploadedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { status = "uploaded" });
    }
}
