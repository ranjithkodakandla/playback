import { useState } from 'react';

interface Props {
  onApply: (filters: Record<string, string>) => void;
}

export function Filters({ onApply }: Props) {
  const [browser, setBrowser] = useState('');
  const [url, setUrl] = useState('');
  const [startUtc, setStartUtc] = useState('');
  const [endUtc, setEndUtc] = useState('');

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        const next: Record<string, string> = {};
        if (browser) next.browser = browser;
        if (url) next.url = url;
        if (startUtc) next.startUtc = new Date(startUtc).toISOString();
        if (endUtc) next.endUtc = new Date(endUtc).toISOString();
        onApply(next);
      }}
      style={{ display: 'grid', gridTemplateColumns: 'repeat(5, minmax(120px, 1fr))', gap: 12, marginBottom: 16 }}
    >
      <input placeholder="Browser" value={browser} onChange={(e) => setBrowser(e.target.value)} />
      <input placeholder="URL contains" value={url} onChange={(e) => setUrl(e.target.value)} />
      <input type="datetime-local" value={startUtc} onChange={(e) => setStartUtc(e.target.value)} />
      <input type="datetime-local" value={endUtc} onChange={(e) => setEndUtc(e.target.value)} />
      <button type="submit">Apply</button>
    </form>
  );
}
