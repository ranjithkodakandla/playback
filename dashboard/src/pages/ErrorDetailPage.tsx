import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { fetchErrorDetail } from '../api/client';
import type { ErrorIssueDetail } from '../types';

export function ErrorDetailPage() {
  const { id } = useParams();
  const [detail, setDetail] = useState<ErrorIssueDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    fetchErrorDetail(id)
      .then(setDetail)
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <p>Loading...</p>;
  if (!detail) return <p>Issue not found.</p>;

  return (
    <div>
      <p><Link to="/">← Back</Link></p>
      <h1>{detail.issue.message}</h1>
      <p><strong>Fingerprint:</strong> {detail.issue.fingerprint}</p>
      <p><strong>Occurrences:</strong> {detail.issue.occurrenceCount}</p>

      <h2>Stack Trace</h2>
      <pre style={{ background: '#111', color: '#eee', padding: 12, borderRadius: 8, overflowX: 'auto' }}>{detail.issue.stackTrace}</pre>

      <h2>Timeline (hourly)</h2>
      <ul>
        {detail.timeline.map((point) => (
          <li key={point.bucketUtc}>{new Date(point.bucketUtc).toLocaleString()} — {point.count}</li>
        ))}
      </ul>

      <h2>Browser Breakdown</h2>
      <ul>
        {detail.browserBreakdown.map((b) => (
          <li key={b.browser}>{b.browser}: {b.count}</li>
        ))}
      </ul>

      <h2>Recent Events</h2>
      <ul>
        {detail.events.map((event) => (
          <li key={event.id}>
            {new Date(event.timestampUtc).toLocaleString()} — {event.browser} — {event.url}
          </li>
        ))}
      </ul>
    </div>
  );
}
