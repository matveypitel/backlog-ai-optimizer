<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { scrapingApi, ApiError } from '$lib/api';
  import type { ScrapingJob } from '$lib/types';
  import Field from '$lib/components/Field.svelte';
  import Button from '$lib/components/Button.svelte';
  import StatusBadge from '$lib/components/StatusBadge.svelte';
  import { toast } from '$lib/stores/toast';
  import { formatRelative } from '$lib/utils/format';

  let url = $state('');
  let submitting = $state(false);
  let jobs = $state<ScrapingJob[]>([]);
  let timer: ReturnType<typeof setInterval> | null = null;

  async function load() {
    try {
      jobs = await scrapingApi.jobs();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    }
  }

  async function enqueue(e: SubmitEvent) {
    e.preventDefault();
    if (!url) return;
    submitting = true;
    try {
      await scrapingApi.enqueue(url);
      toast.success('Queued.');
      url = '';
      await load();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      submitting = false;
    }
  }

  onMount(() => {
    load();
    timer = setInterval(load, 5000);
  });

  onDestroy(() => {
    if (timer) clearInterval(timer);
  });
</script>

<svelte:head><title>Scraping · Admin</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">Admin / Scraping</span>
  <h1>Queue a competitor page.</h1>
</header>

<form onsubmit={enqueue} class="form">
  <div class="row">
    <Field label="URL" name="url" placeholder="https://competitor.com/features" required bind:value={url} />
    <Button type="submit" loading={submitting}>Queue</Button>
  </div>
</form>

<h2>Recent jobs</h2>
{#if jobs.length === 0}
  <p class="muted">No jobs yet.</p>
{:else}
  <table>
    <thead>
      <tr>
        <th>Status</th>
        <th>URL</th>
        <th>Attempts</th>
        <th>Created</th>
        <th>Error</th>
      </tr>
    </thead>
    <tbody>
      {#each jobs as j}
        <tr>
          <td><StatusBadge status={j.status} /></td>
          <td class="url mono">{j.url}</td>
          <td class="mono">{j.attemptCount}</td>
          <td class="muted">{formatRelative(j.createdAt)}</td>
          <td class="error">{j.errorMessage ?? ''}</td>
        </tr>
      {/each}
    </tbody>
  </table>
{/if}

<style>
  .page-head { margin-bottom: var(--space-5); }
  .eyebrow {
    font-family: var(--font-mono);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    display: block;
    margin-bottom: var(--space-2);
  }
  .form { margin-bottom: var(--space-6); }
  .row {
    display: flex;
    gap: var(--space-3);
    align-items: flex-end;
  }
  .row > :global(.field) { flex: 1; max-width: 600px; }
  h2 {
    font-size: 1.05rem;
    margin-bottom: var(--space-3);
    padding-bottom: var(--space-2);
    border-bottom: var(--hairline);
  }
  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.9rem;
  }
  th, td {
    text-align: left;
    padding: var(--space-3);
    border-bottom: 1px solid var(--rule-soft);
    vertical-align: top;
  }
  th {
    font-family: var(--font-mono);
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    font-weight: 500;
    border-bottom: var(--hairline);
  }
  .url { max-width: 400px; word-break: break-all; }
  .muted { color: var(--ink-soft); font-size: 0.85rem; }
  .error { color: var(--danger); font-size: 0.85rem; }
</style>
