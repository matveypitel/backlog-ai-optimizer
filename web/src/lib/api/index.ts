import { api } from './client';
import type {
  CompetitorFeature,
  EmbeddingSyncResult,
  FeatureSuggestion,
  JiraIssue,
  JobStatusInfo,
  PromptTemplate,
  ReprioritizationSuggestion,
  ScrapedPage,
  ScrapingJob,
  SimilarityMatch,
  SyncResult,
  SyncStatus
} from '$lib/types';

export { api, ApiError } from './client';
export { authApi } from './auth';

export const scrapingApi = {
  enqueue: (url: string) => api('/api/scraping', { method: 'POST', body: { url } }),
  jobs: () => api<ScrapingJob[]>('/api/scraping/jobs'),
  pages: () => api<ScrapedPage[]>('/api/scraping/pages')
};

export const competitorsApi = {
  list: (search?: string, pageId?: string) =>
    api<CompetitorFeature[]>('/api/competitor-features', { query: { search, pageId } }),
  get: (id: string) => api<CompetitorFeature>(`/api/competitor-features/${id}`)
};

export const jiraApi = {
  list: (params?: { status?: string; priority?: string; search?: string }) =>
    api<JiraIssue[]>('/api/jira-issues', { query: params })
};

export const syncApi = {
  run: () => api<SyncResult>('/api/sync', { method: 'POST' }),
  status: () => api<SyncStatus>('/api/sync/status')
};

export const suggestionsApi = {
  analyze: () => api('/api/feature-suggestions/analyze', { method: 'POST' }),
  list: () => api<FeatureSuggestion[]>('/api/feature-suggestions'),
  get: (id: string) => api<FeatureSuggestion>(`/api/feature-suggestions/${id}`),
  latestJob: () => api<JobStatusInfo>('/api/feature-suggestions/jobs/latest')
};

export const reprioritizationApi = {
  analyze: () => api('/api/reprioritization/analyze', { method: 'POST' }),
  list: () => api<ReprioritizationSuggestion[]>('/api/reprioritization'),
  latestJob: () => api<JobStatusInfo>('/api/reprioritization/jobs/latest')
};

export const embeddingsApi = {
  syncJira: () => api<EmbeddingSyncResult>('/api/embeddings/sync/jira', { method: 'POST' }),
  syncFeatures: () => api<EmbeddingSyncResult>('/api/embeddings/sync/features', { method: 'POST' }),
  search: (jiraKey: string, topN = 5) =>
    api<SimilarityMatch[]>('/api/embeddings/search', { method: 'POST', body: { jiraKey, topN } })
};

export const promptsApi = {
  list: () => api<PromptTemplate[]>('/api/admin/prompts'),
  get: (type: string) => api<PromptTemplate>(`/api/admin/prompts/${type}`),
  update: (type: string, instructions: string) =>
    api(`/api/admin/prompts/${type}`, { method: 'PUT', body: { instructions } }),
  reset: (type: string) => api(`/api/admin/prompts/${type}/reset`, { method: 'POST' })
};
