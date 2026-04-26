<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { suggestionsApi, ApiError } from '$lib/api';
  import type { FeatureSuggestion, JobStatusInfo } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Button from '$lib/components/Button.svelte';
  import StatusBadge from '$lib/components/StatusBadge.svelte';
  import Tag from '$lib/components/Tag.svelte';
  import EmptyState from '$lib/components/EmptyState.svelte';
  import { toast } from '$lib/stores/toast';
  import { formatRelative, parseJsonArray, truncate } from '$lib/utils/format';

  let items = $state<FeatureSuggestion[]>([]);
  let loading = $state(true);
  let analyzing = $state(false);
  let job = $state<JobStatusInfo | null>(null);
  let pollTimer: ReturnType<typeof setInterval> | null = null;

  async function load() {
    try {
      items = await suggestionsApi.list();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed to load');
    } finally {
      loading = false;
    }
  }

  async function analyze() {
    analyzing = true;
    try {
      await suggestionsApi.analyze();
      toast.info('Analysis started.');
      startPolling();
    } catch (err) {
      analyzing = false;
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    }
  }

  function startPolling() {
    if (pollTimer) clearInterval(pollTimer);
    pollTimer = setInterval(async () => {
      try {
        job = await suggestionsApi.latestJob();
        if (job.status === 'Completed') {
          stopPolling();
          analyzing = false;
          toast.success('Analysis complete.');
          await load();
        } else if (job.status === 'Failed') {
          stopPolling();
          analyzing = false;
          toast.error(job.errorMessage ?? 'Analysis failed.');
        }
      } catch {
        // ignore poll errors
      }
    }, 3000);
  }

  function stopPolling() {
    if (pollTimer) {
      clearInterval(pollTimer);
      pollTimer = null;
    }
  }

  onMount(load);
  onDestroy(stopPolling);
</script>

<svelte:head><title>Suggestions · Backlog AI</title></svelte:head>

<header class="page-head">
  <div>
    <span class="eyebrow">Feature suggestions</span>
    <h1>Closing the gap.</h1>
  </div>
  <Button onclick={analyze} loading={analyzing} disabled={analyzing}>Run analysis</Button>
</header>

{#if job && analyzing}
  <p class="job-status">
    <StatusBadge status={job.status} /> Started {formatRelative(job.createdAt)}
  </p>
{/if}

{#if loading}
  <p class="muted">Loading…</p>
{:else if items.length === 0}
  <EmptyState
    title="No suggestions yet"
    description="Once competitor features are scraped and analyzed, gaps will appear here."
  />
{:else}
  <div class="list">
    {#each items as item}
      <a href="/suggestions/{item.id}" class="entry">
        <Card>
          <div class="row">
            <h3>{item.title}</h3>
            <StatusBadge status={item.suggestedPriority} />
          </div>
          {#if item.businessValue}
            <p class="value">{truncate(item.businessValue, 220)}</p>
          {/if}
          <div class="meta">
            <span class="mono">{item.issueType}</span>
            {#each parseJsonArray(item.tags).slice(0, 4) as tag}
              <Tag>{tag}</Tag>
            {/each}
            <span class="when">· {formatRelative(item.analyzedAt)}</span>
          </div>
        </Card>
      </a>
    {/each}
  </div>
{/if}

<style>
  .page-head {
    display: flex;
    justify-content: space-between;
    align-items: flex-end;
    gap: var(--space-4);
    margin-bottom: var(--space-6);
    flex-wrap: wrap;
  }
  .eyebrow {
    font-family: var(--font-mono);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    display: block;
    margin-bottom: var(--space-2);
  }
  .job-status {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    color: var(--ink-soft);
    font-size: 0.9rem;
    margin-bottom: var(--space-4);
  }
  .muted {
    color: var(--ink-soft);
  }
  .list {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }
  .entry {
    color: inherit;
    border: 0;
  }
  .entry:hover {
    border: 0;
  }
  .row {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: var(--space-4);
    margin-bottom: var(--space-3);
  }
  .row h3 {
    margin: 0;
  }
  .value {
    color: var(--ink-soft);
    margin-bottom: var(--space-3);
  }
  .meta {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    flex-wrap: wrap;
    font-size: 0.85rem;
    color: var(--ink-soft);
  }
  .when {
    margin-left: var(--space-2);
  }
</style>
