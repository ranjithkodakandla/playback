namespace ErrorMonitor.Api.Contracts;

public record ErrorIssueResponse(
    Guid Id,
    string Message,
    string StackTrace,
    string Url,
    string Browser,
    string Fingerprint,
    int OccurrenceCount,
    DateTime FirstSeenAtUtc,
    DateTime LastSeenAtUtc,
    string? Release
);

public record ErrorIssueDetailResponse(
    ErrorIssueResponse Issue,
    List<ErrorEventResponse> Events,
    List<TimelinePointResponse> Timeline,
    List<BrowserBreakdownResponse> BrowserBreakdown
);

public record ErrorEventResponse(
    Guid Id,
    DateTime TimestampUtc,
    string Browser,
    string Url,
    string StackTrace,
    string? UserId,
    string ContextJson
);

public record TimelinePointResponse(DateTime BucketUtc, int Count);

public record BrowserBreakdownResponse(string Browser, int Count);
