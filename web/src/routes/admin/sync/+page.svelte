<script lang="ts">
  import { onMount } from 'svelte';
  import { syncApi, ApiError } from '$lib/api';
  import type { SyncResult, SyncStatus } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Button from '$lib/components/Button.svelte';
  import { toast } from '$lib/stores/toast';
  import { formatRelative } from '$lib/utils/format';

  let status = $state<SyncStatus | null>(null);
  let lastRun = $state<SyncResult | null>(null);
  let running = $state(false);

  async function loadStatus() {
    try {
      status = await syncApi.status();
    } catch {
      // ignore
    }
  }

  async function run() {
    running = true;
    try {
      lastRun = await syncApi.run();
      toast.success(`Synced ${lastRun.syncedCount} issues.`);
      await loadStatus();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Sync failed');
    } finally {
      running = false;
    }
  }

  onMount(loadStatus);
</script>

<svelte:head><title>Jira sync · Admin</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">Admin / Jira sync</span>
  <h1>Pull the latest backlog.</h1>
</header>

<div class="grid">
  <Card eyebrow="Currently synced" title={status ? String(status.totalIssues) : '—'}>
    <p>Last sync {formatRelative(status?.lastSyncedAt)}</p>
  </Card>

  <Card title="Run sync">
    <p>Pulls all configured-project issues from Jira and upserts into the local database.</p>
    <Button onclick={run} loading={running}>Sync now</Button>
  </Card>
</div>

{#if lastRun}
  <section class="result">
    <h2>Last run</h2>
    <p>Synced <strong>{lastRun.syncedCount}</strong> issues.</p>
    {#if lastRun.errors.length > 0}
      <h3>Errors</h3>
      <ul>
        {#each lastRun.errors as e}<li class="error">{e}</li>{/each}
      </ul>
    {:else}
      <p class="muted">No errors.</p>
    {/if}
  </section>
{/if}

<style>
  .page-head { margin-bottom: var(--space-6); }
  .eyebrow {
    font-family: var(--font-mono);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    display: block;
    margin-bottom: var(--space-2);
  }
  .grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    gap: var(--space-4);
    margin-bottom: var(--space-6);
  }
  .result {
    border-top: var(--hairline);
    padding-top: var(--space-5);
  }
  h2 {
    font-size: 1.05rem;
    margin-bottom: var(--space-3);
  }
  h3 {
    font-size: 0.95rem;
    margin-top: var(--space-4);
    margin-bottom: var(--space-2);
  }
  .error { color: var(--danger); font-family: var(--font-mono); font-size: 0.85rem; }
  .muted { color: var(--ink-soft); }
</style>
