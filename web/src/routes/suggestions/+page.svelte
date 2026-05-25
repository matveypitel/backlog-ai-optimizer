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
  import { T } from '$lib/i18n';
  import { formatRelative, parseJsonArray, truncate } from '$lib/utils/format';

  let items = $state<FeatureSuggestion[]>([]);
  let loading = $state(true);
  let analyzing = $state(false);
  let applying = $state(new Set<string>());
  let job = $state<JobStatusInfo | null>(null);
  let pollTimer: ReturnType<typeof setInterval> | null = null;

  async function applyOne(event: MouseEvent, id: string) {
    event.preventDefault();
    event.stopPropagation();
    applying = new Set(applying).add(id);
    try {
      const result = await suggestionsApi.apply(id);
      items = items.filter((x) => x.id !== id);
      toast.success(`${$T.suggestions.createdIn} ${result.jiraKey} ${$T.suggestions.inJira}`);
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.suggestions.failedToCreate);
    } finally {
      const next = new Set(applying);
      next.delete(id);
      applying = next;
    }
  }

  async function load() {
    try {
      items = await suggestionsApi.list();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failedToLoad);
    } finally {
      loading = false;
    }
  }

  async function analyze() {
    analyzing = true;
    try {
      await suggestionsApi.analyze();
      toast.info($T.common.analysisStarted);
      startPolling();
    } catch (err) {
      analyzing = false;
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
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
          toast.success($T.common.analysisComplete);
          await load();
        } else if (job.status === 'Failed') {
          stopPolling();
          analyzing = false;
          toast.error(job.errorMessage ?? $T.common.analysisFailed);
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

<svelte:head><title>{$T.suggestions.title}</title></svelte:head>

<header class="page-head">
  <div>
    <span class="eyebrow">{$T.suggestions.eyebrow}</span>
    <h1>{$T.suggestions.heading}</h1>
  </div>
  <Button onclick={analyze} loading={analyzing} disabled={analyzing}>{$T.common.runAnalysis}</Button>
</header>

{#if job && analyzing}
  <p class="job-status">
    <StatusBadge status={job.status} /> {$T.common.started} {formatRelative(job.createdAt)}
  </p>
{/if}

{#if loading}
  <p class="muted">{$T.common.loading}</p>
{:else if items.length === 0}
  <EmptyState
    title={$T.suggestions.noSuggestionsTitle}
    description={$T.suggestions.noSuggestionsDesc}
  />
{:else}
  <div class="list">
    {#each items as item}
      <a href="/suggestions/{item.id}" class="entry">
        <Card>
          <div class="row">
            <h3>{item.title}</h3>
            <div class="row-right">
              <StatusBadge status={item.suggestedPriority} />
              <Button
                onclick={(e: MouseEvent) => applyOne(e, item.id)}
                loading={applying.has(item.id)}
                disabled={applying.has(item.id)}
              >{$T.suggestions.createInJira}</Button>
            </div>
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
    flex-wrap: wrap;
  }
  .row h3 {
    margin: 0;
    flex: 1;
    min-width: 0;
  }
  .row-right {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    flex-wrap: wrap;
    flex-shrink: 0;
  }
  @media (max-width: 600px) {
    .row {
      flex-direction: column;
      gap: var(--space-2);
    }
    .row-right {
      align-self: flex-start;
    }
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
