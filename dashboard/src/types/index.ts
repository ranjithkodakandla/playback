export interface ErrorIssue {
  id: string;
  message: string;
  stackTrace: string;
  url: string;
  browser: string;
  fingerprint: string;
  occurrenceCount: number;
  firstSeenAtUtc: string;
  lastSeenAtUtc: string;
  release?: string;
}

export interface ErrorEvent {
  id: string;
  timestampUtc: string;
  browser: string;
  url: string;
  stackTrace: string;
  userId?: string;
  contextJson: string;
}

export interface TimelinePoint {
  bucketUtc: string;
  count: number;
}

export interface BrowserBreakdown {
  browser: string;
  count: number;
}

export interface ErrorIssueDetail {
  issue: ErrorIssue;
  events: ErrorEvent[];
  timeline: TimelinePoint[];
  browserBreakdown: BrowserBreakdown[];
}
