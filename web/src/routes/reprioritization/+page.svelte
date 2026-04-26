<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { reprioritizationApi, ApiError } from '$lib/api';
  import type { ReprioritizationSuggestion, JobStatusInfo } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Button from '$lib/components/Button.svelte';
  import StatusBadge from '$lib/components/StatusBadge.svelte';
  import EmptyState from '$lib/components/EmptyState.svelte';
  import { toast } from '$lib/stores/toast';
  import { formatRelative } from '$lib/utils/format';

  let items = $state<ReprioritizationSuggestion[]>([]);
  let loading = $state(true);
  let analyzing = $state(false);
  let job = $state<JobStatusInfo | null>(null);
  let pollTimer: ReturnType<typeof setInterval> | null = null;

  async function load() {
    try {
      items = await reprioritizationApi.list();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed to load');
    } finally {
      loading = false;
    }
  }

  async function analyze() {
    analyzing = true;
    try {
      await reprioritizationApi.analyze();
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
        job = await reprioritizationApi.latestJob();
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
        // ignore
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

  let high = $derived(items.filter((x) => x.confidenceScore >= 0.8));
  let medium = $derived(items.filter((x) => x.confidenceScore >= 0.5 && x.confidenceScore < 0.8));
  let low = $derived(items.filter((x) => x.confidenceScore < 0.5));
</script>

<svelte:head><title>Reprioritization · Backlog AI</title></svelte:head>

<header class="page-head">
  <div>
    <span class="eyebrow">Reprioritization</span>
    <h1>Where the wind is blowing.</h1>
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
    title="No reprioritization suggestions yet"
    description="Run analysis once Jira issues and competitor features are synced."
  />
{:else}
  {#snippet group(label: string, list: ReprioritizationSuggestion[])}
    {#if list.length > 0}
      <h2 class="group-label">{label} <span class="count">({list.length})</span></h2>
      <div class="list">
        {#each list as r}
          <Card>
            <div class="row">
              <span class="key mono">{r.jiraKey}</span>
              <div class="prio">
                <StatusBadge status={r.currentPriority} />
                <span class="arrow">→</span>
                <StatusBadge status={r.suggestedPriority} />
                <span class="confidence mono">conf {r.confidenceScore.toFixed(2)}</span>
              </div>
            </div>
            <p>{r.reasoning}</p>
            {#if r.competitorEvidence}
              <p class="evidence">{r.competitorEvidence}</p>
            {/if}
          </Card>
        {/each}
      </div>
    {/if}
  {/snippet}

  {@render group('High confidence', high)}
  {@render group('Medium confidence', medium)}
  {@render group('Low confidence', low)}
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
  .muted { color: var(--ink-soft); }
  .group-label {
    font-size: 1.05rem;
    margin: var(--space-6) 0 var(--space-3);
    padding-bottom: var(--space-2);
    border-bottom: var(--hairline);
  }
  .count {
    color: var(--ink-faint);
    font-family: var(--font-mono);
    font-size: 0.85rem;
    font-weight: normal;
  }
  .list {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }
  .row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: var(--space-3);
    margin-bottom: var(--space-3);
    flex-wrap: wrap;
  }
  .key {
    font-weight: 500;
    color: var(--ink);
  }
  .prio {
    display: flex;
    align-items: center;
    gap: var(--space-2);
  }
  .arrow {
    color: var(--ink-faint);
  }
  .confidence {
    font-size: 0.75rem;
    color: var(--ink-faint);
    margin-left: var(--space-2);
  }
  .evidence {
    font-family: var(--font-mono);
    font-size: 0.8rem;
    color: var(--ink-soft);
    background: var(--bg-sunk);
    padding: var(--space-2) var(--space-3);
    border-radius: var(--radius);
    margin-top: var(--space-2);
  }
</style>
