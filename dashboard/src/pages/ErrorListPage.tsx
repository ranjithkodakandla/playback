import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { fetchErrors } from '../api/client';
import type { ErrorIssue } from '../types';
import { Filters } from '../components/Filters';

export function ErrorListPage() {
  const [data, setData] = useState<ErrorIssue[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<Record<string, string>>({});

  useEffect(() => {
    setLoading(true);
    fetchErrors(filters)
      .then(setData)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, [filters]);

  return (
    <div>
      <h1>Error Issues</h1>
      <Filters onApply={setFilters} />
      {loading && <p>Loading...</p>}
      {error && <p>{error}</p>}
      <table width="100%" cellPadding={8}>
        <thead>
          <tr>
            <th align="left">Message</th>
            <th align="left">URL</th>
            <th align="left">Browser</th>
            <th align="right">Occurrences</th>
            <th align="left">Last Seen</th>
          </tr>
        </thead>
        <tbody>
          {data.map((issue) => (
            <tr key={issue.id}>
              <td>
                <Link to={`/errors/${issue.id}`}>{issue.message}</Link>
              </td>
              <td>{issue.url}</td>
              <td>{issue.browser}</td>
              <td align="right">{issue.occurrenceCount}</td>
              <td>{new Date(issue.lastSeenAtUtc).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
