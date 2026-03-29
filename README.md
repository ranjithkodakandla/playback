# Self-Hosted Frontend Error Monitor (MVP)

A production-minded MVP for capturing frontend JavaScript errors, grouping them on a .NET backend, storing events in PostgreSQL, and exploring issues in a React dashboard.

## Components

- `sdk/`: lightweight browser SDK (TypeScript)
- `backend/ErrorMonitor.Api/`: .NET 8 Web API ingestion and query service
- `dashboard/`: React + Vite dashboard UI
- `docker-compose.yml`: API + Postgres + dashboard local stack

## Features

- Browser capture hooks:
  - `window.onerror`
  - `window.onunhandledrejection`
  - click breadcrumbs
  - console breadcrumbs (`console.log`, `console.error`)
  - `fetch` failure tracking
- Error grouping via fingerprint (`message + normalized stack` hash)
- Duplicate handling via `occurrenceCount`
- Individual event storage for timeline and browser analytics
- API filtering by browser, URL, date range
- Source map upload endpoint (MVP decode annotation path)
- Basic per-endpoint rate limiting
- Sensitive data masking and configurable field exclusion
- Async ingestion queue so API requests return quickly

## Local development

### 1) Run with Docker Compose

```bash
cp .env.example .env
docker compose up --build
```

Services:
- API: `http://localhost:8080`
- Dashboard: `http://localhost:5173`
- Postgres: `localhost:5432`

### 2) Run backend directly

```bash
cd backend/ErrorMonitor.Api
dotnet run
```

### 3) Run dashboard directly

```bash
cd dashboard
npm install
npm run dev
```

### 4) Build SDK

```bash
cd sdk
npm install
npm run build
```

## Frontend SDK integration snippet

```ts
import { initErrorMonitor } from '@playback/error-monitor-sdk';

const monitor = initErrorMonitor({
  endpoint: 'http://localhost:8080/api/errors',
  userId: 'user-123',
  release: 'web@1.0.0',
  flushIntervalMs: 3000,
  batchSize: 10,
  disabledFields: ['requestBody']
});

// Optional manual capture
monitor.capture('Custom error marker', undefined, { feature: 'checkout' });
```

## API endpoints

- `POST /api/errors`
- `GET /api/errors?browser=Chrome&url=/checkout&startUtc=...&endUtc=...`
- `GET /api/errors/{id}`
- `POST /api/sourcemaps`

Source map upload payload:

```json
{
  "release": "web@1.0.0",
  "minifiedFileUrl": "https://cdn.example.com/app.min.js",
  "sourceMapJson": "{...}"
}
```

## Environment variables

See `.env.example` for defaults.

Backend key settings:
- `ConnectionStrings__DefaultConnection`
- `RateLimiting__PermitLimit`
- `RateLimiting__WindowSeconds`
- `Privacy__SensitiveKeys__0=password` (and others)
- `Privacy__DisableFieldCapture__0=requestBody`

Dashboard setting:
- `VITE_API_BASE_URL`

## Notes on source maps (MVP)

The source map endpoint and persistence are fully wired. For MVP simplicity, decode currently annotates traces when a matching release+file source map exists, leaving full VLQ mapping logic as an extension point in `SourceMapService`.

## Extensibility roadmap

- full source-map symbolication
- release tagging in UI filters
- email spike alerts
- charts and aggregations persisted in rollup tables
- auth / project-level API keys
