export interface MonitorOptions {
  endpoint: string;
  userId?: string;
  release?: string;
  flushIntervalMs?: number;
  batchSize?: number;
  disabledFields?: string[];
}

interface Breadcrumb {
  type: string;
  message: string;
  timestampUtc: string;
}

interface ErrorPayload {
  message: string;
  stackTrace?: string;
  url: string;
  userAgent: string;
  timestampUtc: string;
  userId?: string;
  release?: string;
  breadcrumbs: Breadcrumb[];
  context?: Record<string, unknown>;
}

const MAX_BREADCRUMBS = 30;

export function initErrorMonitor(options: MonitorOptions) {
  const queue: ErrorPayload[] = [];
  const breadcrumbs: Breadcrumb[] = [];
  const flushIntervalMs = options.flushIntervalMs ?? 3000;
  const batchSize = options.batchSize ?? 10;
  const disabledFields = new Set((options.disabledFields ?? []).map((x) => x.toLowerCase()));

  const baseContext = {
    url: () => window.location.href,
    userAgent: () => navigator.userAgent,
    userId: () => options.userId,
    release: () => options.release
  };

  const pushBreadcrumb = (crumb: Breadcrumb) => {
    breadcrumbs.push(crumb);
    if (breadcrumbs.length > MAX_BREADCRUMBS) breadcrumbs.shift();
  };

  const maskValue = (key: string, value: unknown) => {
    const lower = key.toLowerCase();
    if (lower.includes("password") || lower.includes("token") || lower.includes("secret")) return "***";
    if (disabledFields.has(lower)) return undefined;
    return value;
  };

  const sanitizeContext = (context?: Record<string, unknown>) => {
    if (!context) return undefined;
    return Object.fromEntries(
      Object.entries(context)
        .map(([key, value]) => [key, maskValue(key, value)])
        .filter(([, value]) => value !== undefined)
    );
  };

  const capture = (message: string, stack?: string, context?: Record<string, unknown>) => {
    try {
      queue.push({
        message,
        stackTrace: stack,
        url: baseContext.url(),
        userAgent: baseContext.userAgent(),
        timestampUtc: new Date().toISOString(),
        userId: baseContext.userId(),
        release: baseContext.release(),
        breadcrumbs: [...breadcrumbs],
        context: sanitizeContext(context)
      });

      if (queue.length >= batchSize) {
        void flush();
      }
    } catch {
      // Never break host app.
    }
  };

  const originalConsoleLog = console.log.bind(console);
  const originalConsoleError = console.error.bind(console);

  console.log = (...args: unknown[]) => {
    pushBreadcrumb({ type: "console.log", message: args.map(String).join(" "), timestampUtc: new Date().toISOString() });
    originalConsoleLog(...args);
  };

  console.error = (...args: unknown[]) => {
    pushBreadcrumb({ type: "console.error", message: args.map(String).join(" "), timestampUtc: new Date().toISOString() });
    originalConsoleError(...args);
  };

  document.addEventListener("click", (event) => {
    const target = event.target as HTMLElement | null;
    const label = target?.id || target?.className || target?.tagName || "unknown";
    pushBreadcrumb({ type: "click", message: `Clicked ${label}`, timestampUtc: new Date().toISOString() });
  });

  const originalFetch = window.fetch.bind(window);
  window.fetch = async (...args: Parameters<typeof fetch>) => {
    try {
      const response = await originalFetch(...args);
      if (!response.ok) {
        capture(`Network failure: ${response.status} ${response.statusText}`, undefined, {
          resource: String(args[0]),
          status: response.status
        });
      }
      return response;
    } catch (error) {
      capture("Network request threw", error instanceof Error ? error.stack : String(error), {
        resource: String(args[0])
      });
      throw error;
    }
  };

  window.addEventListener("error", (event) => {
    capture(event.message, event.error?.stack, { filename: event.filename, lineno: event.lineno, colno: event.colno });
  });

  window.addEventListener("unhandledrejection", (event) => {
    const reason = event.reason;
    capture("Unhandled promise rejection", reason?.stack ?? String(reason));
  });

  const flush = async () => {
    if (queue.length === 0) return;

    const payloads = queue.splice(0, batchSize);
    await Promise.allSettled(
      payloads.map((payload) =>
        fetch(options.endpoint, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload),
          keepalive: true
        })
      )
    );
  };

  const interval = window.setInterval(() => {
    void flush();
  }, flushIntervalMs);

  window.addEventListener("beforeunload", () => {
    void flush();
    window.clearInterval(interval);
  });

  return {
    capture,
    flush,
    dispose: () => {
      window.clearInterval(interval);
      window.fetch = originalFetch;
      console.log = originalConsoleLog;
      console.error = originalConsoleError;
    }
  };
}
