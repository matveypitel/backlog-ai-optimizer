export type Role = 'Admin' | 'User';

export interface User {
  id: string;
  email: string;
  role: Role;
}

export interface AuthToken {
  accessToken: string;
  expiresAtUtc: string;
  email: string;
  role: Role;
}

export type JobStatus = 'Pending' | 'Running' | 'Completed' | 'Failed';

export interface JobStatusInfo {
  id: string;
  status: JobStatus;
  errorMessage: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface FeatureSuggestion {
  id: string;
  title: string;
  description: string;
  issueType: string;
  suggestedPriority: string;
  tags: string;
  reasoning: string;
  competitorEvidence: string;
  businessValue: string;
  estimatedImpact: string;
  userStories: string;
  acceptanceCriteria: string;
  analyzedAt: string;
}

export interface ReprioritizationSuggestion {
  jiraKey: string;
  currentPriority: string;
  suggestedPriority: string;
  reasoning: string;
  competitorEvidence: string;
  confidenceScore: number;
  analyzedAt: string;
}

export interface CompetitorFeature {
  id: string;
  name: string;
  description: string;
  category: string | null;
  keyBenefits: string;
  useCases: string;
  differentiators: string;
  targetAudience: string | null;
  sourceUrl: string;
  sourceTitle: string | null;
  extractedAt: string;
}

export interface ScrapedPage {
  id: string;
  url: string;
  title: string | null;
  featuresCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface ScrapingSource {
  id: string;
  url: string;
  name: string | null;
  isActive: boolean;
  lastScrapedAt: string | null;
  refreshIntervalHours: number | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface SyncAllResult {
  enqueued: number;
  skipped: number;
}

export interface ScrapingJob {
  id: string;
  url: string;
  status: 'Pending' | 'Running' | 'Completed' | 'Failed';
  attemptCount: number;
  errorMessage: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface JiraIssue {
  id: string;
  jiraKey: string;
  summary: string;
  description: string | null;
  status: string;
  priority: string | null;
  issueType: string;
  assigneeEmail: string | null;
  jiraUrl: string | null;
  jiraCreatedAt: string;
  jiraUpdatedAt: string;
  lastSyncedAt: string;
}

export interface SyncStatus {
  totalIssues: number;
  lastSyncedAt: string | null;
}

export interface SyncResult {
  syncedCount: number;
  errors: string[];
}

export interface EmbeddingSyncResult {
  embeddedCount: number;
  skippedCount: number;
  errors: string[];
}

export interface SimilarityMatch {
  competitorFeatureId: string;
  name: string;
  description: string;
  category: string | null;
  sourceUrl: string;
  similarity: number;
}

export interface PromptTemplate {
  type: 'FeatureExtraction' | 'FeatureSuggestion' | 'Reprioritization';
  instructions: string;
  schema: string;
  defaultInstructions: string;
  updatedAt: string | null;
  updatedByUserId: string | null;
}

export interface UserSummary {
  id: string;
  email: string;
  role: string;
  createdAt: string;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  detail?: string;
  status?: number;
}
