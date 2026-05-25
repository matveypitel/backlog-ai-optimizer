<script lang="ts">
  import { onMount } from 'svelte';
  import { syncApi, ApiError } from '$lib/api';
  import type { SyncResult, SyncStatus } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Button from '$lib/components/Button.svelte';
  import { toast } from '$lib/stores/toast';
  import { T } from '$lib/i18n';
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
      toast.success(`${$T.admin.sync.syncedCount} ${lastRun.syncedCount} ${$T.admin.sync.issues}`);
      await loadStatus();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.admin.sync.syncFailed);
    } finally {
      running = false;
    }
  }

  onMount(loadStatus);
</script>

<svelte:head><title>{$T.admin.sync.title}</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">{$T.admin.sync.eyebrow}</span>
  <h1>{$T.admin.sync.heading}</h1>
</header>

<div class="grid">
  <Card eyebrow={$T.admin.sync.currentlySynced} title={status ? String(status.totalIssues) : '—'}>
    <p>{$T.admin.sync.lastSync} {formatRelative(status?.lastSyncedAt)}</p>
  </Card>

  <Card title={$T.admin.sync.runSync}>
    <p>{$T.admin.sync.runSyncDesc}</p>
    <Button onclick={run} loading={running}>{$T.admin.sync.syncNow}</Button>
  </Card>
</div>

{#if lastRun}
  <section class="result">
    <h2>{$T.admin.sync.lastRun}</h2>
    <p>{$T.admin.sync.syncedCount} <strong>{lastRun.syncedCount}</strong> {$T.admin.sync.issues}</p>
    {#if lastRun.errors.length > 0}
      <h3>{$T.common.errors}</h3>
      <ul>
        {#each lastRun.errors as e}<li class="error">{e}</li>{/each}
      </ul>
    {:else}
      <p class="muted">{$T.common.noErrors}</p>
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
