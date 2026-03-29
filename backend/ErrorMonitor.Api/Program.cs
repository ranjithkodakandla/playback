using System.Threading.RateLimiting;
using ErrorMonitor.Api.Data;
using ErrorMonitor.Api.Middleware;
using ErrorMonitor.Api.Options;
using ErrorMonitor.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PrivacyOptions>(builder.Configuration.GetSection("Privacy"));

builder.Services.AddDbContext<ErrorMonitorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRateLimiter(options =>
{
    var permitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 200);
    var windowSeconds = builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60);
    var queueLimit = builder.Configuration.GetValue("RateLimiting:QueueLimit", 0);

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("ingestion", o =>
    {
        o.PermitLimit = permitLimit;
        o.Window = TimeSpan.FromSeconds(windowSeconds);
        o.QueueLimit = queueLimit;
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

builder.Services.AddSingleton<IFingerprintService, FingerprintService>();
builder.Services.AddSingleton<IBrowserParser, BrowserParser>();
builder.Services.AddScoped<IPrivacyService, PrivacyService>();
builder.Services.AddScoped<ISourceMapService, SourceMapService>();
builder.Services.AddSingleton<IIngestionQueue, IngestionQueue>();
builder.Services.AddHostedService<ErrorIngestionWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ErrorMonitorDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRateLimiter();

app.MapControllers();

app.Run();
