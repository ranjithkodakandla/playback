import type { ErrorIssue, ErrorIssueDetail } from '../types';

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080';

export async function fetchErrors(filters: Record<string, string>): Promise<ErrorIssue[]> {
  const qs = new URLSearchParams(filters).toString();
  const res = await fetch(`${API_BASE}/api/errors${qs ? `?${qs}` : ''}`);
  if (!res.ok) throw new Error('Failed to load errors');
  return res.json();
}

export async function fetchErrorDetail(id: string): Promise<ErrorIssueDetail> {
  const res = await fetch(`${API_BASE}/api/errors/${id}`);
  if (!res.ok) throw new Error('Failed to load detail');
  return res.json();
}
