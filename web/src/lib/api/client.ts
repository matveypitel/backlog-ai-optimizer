import { PUBLIC_API_BASE_URL } from '$env/static/public';
import { authStore } from '$lib/stores/auth';
import { get } from 'svelte/store';
import { goto } from '$app/navigation';
import { browser } from '$app/environment';
import type { ProblemDetails } from '$lib/types';

export class ApiError extends Error {
  status: number;
  detail?: string;

  constructor(message: string, status: number, detail?: string) {
    super(message);
    this.status = status;
    this.detail = detail;
  }
}

interface ApiOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
  body?: unknown;
  query?: Record<string, string | number | undefined | null>;
  signal?: AbortSignal;
}

function buildUrl(path: string, query?: ApiOptions['query']): string {
  const url = new URL(path, PUBLIC_API_BASE_URL);
  if (query) {
    for (const [k, v] of Object.entries(query)) {
      if (v !== undefined && v !== null && v !== '') url.searchParams.set(k, String(v));
    }
  }
  return url.toString();
}

export async function api<T = unknown>(path: string, opts: ApiOptions = {}): Promise<T> {
  const auth = get(authStore);
  const headers: Record<string, string> = {
    Accept: 'application/json'
  };
  if (opts.body !== undefined) headers['Content-Type'] = 'application/json';
  if (auth.token) headers.Authorization = `Bearer ${auth.token}`;

  const res = await fetch(buildUrl(path, opts.query), {
    method: opts.method ?? 'GET',
    headers,
    body: opts.body !== undefined ? JSON.stringify(opts.body) : undefined,
    signal: opts.signal
  });

  if (res.status === 401) {
    if (browser) {
      authStore.clear();
      const next = window.location.pathname + window.location.search;
      if (!window.location.pathname.startsWith('/login')) {
        await goto(`/login?next=${encodeURIComponent(next)}`);
      }
    }
    throw new ApiError('Unauthorized', 401);
  }

  if (!res.ok) {
    let detail: string | undefined;
    try {
      const problem = (await res.json()) as ProblemDetails;
      detail = problem.detail ?? problem.title;
    } catch {
      detail = res.statusText;
    }
    throw new ApiError(detail ?? 'Request failed', res.status, detail);
  }

  if (res.status === 204) return undefined as T;

  const text = await res.text();
  if (!text) return undefined as T;
  return JSON.parse(text) as T;
}
