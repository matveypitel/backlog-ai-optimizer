<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { scrapingApi, ApiError } from '$lib/api';
  import type { ScrapingJob, ScrapingSource } from '$lib/types';
  import Field from '$lib/components/Field.svelte';
  import Button from '$lib/components/Button.svelte';
  import StatusBadge from '$lib/components/StatusBadge.svelte';
  import EmptyState from '$lib/components/EmptyState.svelte';
  import { toast } from '$lib/stores/toast';
  import { formatRelative } from '$lib/utils/format';

  // Sources
  let sources = $state<ScrapingSource[]>([]);
  let newUrl = $state('');
  let newName = $state('');
  let addingSource = $state(false);
  let syncingAll = $state(false);
  let busyId = $state<string | null>(null);

  // Ad-hoc
  let adhocUrl = $state('');
  let adhocSubmitting = $state(false);

  // Jobs
  let jobs = $state<ScrapingJob[]>([]);
  let timer: ReturnType<typeof setInterval> | null = null;

  async function loadAll() {
    await Promise.all([loadSources(), loadJobs()]);
  }

  async function loadSources() {
    try {
      sources = await scrapingApi.sources.list();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed to load sources');
    }
  }

  async function loadJobs() {
    try {
      jobs = await scrapingApi.jobs();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed to load jobs');
    }
  }

  async function addSource(e: SubmitEvent) {
    e.preventDefault();
    if (!newUrl) return;
    addingSource = true;
    try {
      await scrapingApi.sources.create(newUrl, newName || null);
      toast.success('Source added.');
      newUrl = '';
      newName = '';
      await loadSources();
    } catch (err) {
      const msg = err instanceof ApiError ? err.detail ?? err.message : 'Failed';
      toast.error(msg);
    } finally {
      addingSource = false;
    }
  }

  async function syncAll() {
    syncingAll = true;
    try {
      const r = await scrapingApi.sources.syncAll();
      if (r.enqueued === 0 && r.skipped === 0) toast.info('Nothing active to sync.');
      else toast.success(`Queued ${r.enqueued}${r.skipped > 0 ? `, ${r.skipped} already in flight` : ''}.`);
      await loadJobs();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      syncingAll = false;
    }
  }

  async function syncOne(s: ScrapingSource) {
    busyId = s.id;
    try {
      await scrapingApi.sources.syncOne(s.id);
      toast.success(`Queued ${s.name ?? s.url}.`);
      await loadJobs();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      busyId = null;
    }
  }

  async function toggleActive(s: ScrapingSource) {
    busyId = s.id;
    try {
      await scrapingApi.sources.update(s.id, {
        name: s.name,
        isActive: !s.isActive,
        refreshIntervalHours: s.refreshIntervalHours
      });
      await loadSources();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      busyId = null;
    }
  }

  async function removeSource(s: ScrapingSource) {
    if (!confirm(`Remove ${s.name ?? s.url}? Past scraped pages and features stay.`)) return;
    busyId = s.id;
    try {
      await scrapingApi.sources.remove(s.id);
      toast.success('Removed.');
      await loadSources();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      busyId = null;
    }
  }

  async function adhocEnqueue(e: SubmitEvent) {
    e.preventDefault();
    if (!adhocUrl) return;
    adhocSubmitting = true;
    try {
      await scrapingApi.enqueue(adhocUrl);
      toast.success('Queued.');
      adhocUrl = '';
      await loadJobs();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      adhocSubmitting = false;
    }
  }

  onMount(() => {
    loadAll();
    timer = setInterval(loadJobs, 5000);
  });

  onDestroy(() => {
    if (timer) clearInterval(timer);
  });

  let activeCount = $derived(sources.filter((s) => s.isActive).length);
</script>

<svelte:head><title>Scraping · Backlog AI</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">Scraping</span>
  <h1>Tracked sources.</h1>
  <p class="lede">
    Maintain a list of competitor pages to scrape regularly. Use <em>Sync all active</em> to enqueue
    every active source at once, or scrape a one-off URL at the bottom.
  </p>
</header>

<section class="block">
  <div class="block-head">
    <h2>Sources <span class="count">({sources.length})</span></h2>
    <Button onclick={syncAll} loading={syncingAll} disabled={activeCount === 0}>
      Sync all active
    </Button>
  </div>

  <form onsubmit={addSource} class="add-form">
    <div class="row">
      <Field label="URL" name="newUrl" placeholder="https://competitor.com/features" required bind:value={newUrl} />
      <Field label="Name (optional)" name="newName" placeholder="Defaults to URL" bind:value={newName} />
      <Button type="submit" loading={addingSource}>Add</Button>
    </div>
  </form>

  {#if sources.length === 0}
    <EmptyState title="No sources yet" description="Add the first competitor URL above." />
  {:else}
    <table>
      <thead>
        <tr>
          <th>Name</th>
          <th>URL</th>
          <th>Status</th>
          <th>Last scraped</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {#each sources as s}
          <tr class:inactive={!s.isActive}>
            <td>{s.name ?? '—'}</td>
            <td><a href={s.url} target="_blank" rel="noopener noreferrer" class="mono url">{s.url}</a></td>
            <td>
              <span class="state mono" class:on={s.isActive}>
                {s.isActive ? 'active' : 'paused'}
              </span>
            </td>
            <td class="muted">{formatRelative(s.lastScrapedAt)}</td>
            <td class="row-actions">
              <button class="link" onclick={() => syncOne(s)} disabled={busyId === s.id || !s.isActive}>
                Sync
              </button>
              <button class="link" onclick={() => toggleActive(s)} disabled={busyId === s.id}>
                {s.isActive ? 'Pause' : 'Resume'}
              </button>
              <button class="link danger" onclick={() => removeSource(s)} disabled={busyId === s.id}>
                Remove
              </button>
            </td>
          </tr>
        {/each}
      </tbody>
    </table>
  {/if}
</section>

<section class="block">
  <h2>One-off scrape</h2>
  <p class="muted small">
    For a single URL you don't want to track. The job runs once and is not associated with a source.
  </p>
  <form onsubmit={adhocEnqueue} class="adhoc-form">
    <div class="row">
      <Field label="URL" name="adhocUrl" placeholder="https://example.com/page" required bind:value={adhocUrl} />
      <Button type="submit" loading={adhocSubmitting}>Queue</Button>
    </div>
  </form>
</section>

<section class="block">
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
</section>

<style>
  .page-head { margin-bottom: var(--space-6); max-width: var(--reading-width); }
  .eyebrow {
    font-family: var(--font-mono);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    display: block;
    margin-bottom: var(--space-2);
  }
  .lede { color: var(--ink-soft); }
  .lede em { font-style: italic; color: var(--accent); }

  .block { margin-bottom: var(--space-7); }
  .block-head {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    margin-bottom: var(--space-3);
    padding-bottom: var(--space-2);
    border-bottom: var(--hairline);
  }
  .block-head h2 {
    border: 0;
    padding: 0;
    margin: 0;
  }
  h2 {
    font-size: 1.05rem;
    margin: 0 0 var(--space-3);
    padding-bottom: var(--space-2);
    border-bottom: var(--hairline);
  }
  .count { color: var(--ink-faint); font-family: var(--font-mono); font-size: 0.85rem; font-weight: normal; }

  .add-form { margin-bottom: var(--space-4); }
  .adhoc-form { margin-top: var(--space-3); }
  .row {
    display: flex;
    gap: var(--space-3);
    align-items: flex-end;
  }
  .row > :global(.field) { flex: 1; max-width: 380px; }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.9rem;
  }
  th, td {
    text-align: left;
    padding: var(--space-3);
    border-bottom: 1px solid var(--rule-soft);
    vertical-align: middle;
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
  tr.inactive td { opacity: 0.55; }
  .url { max-width: 360px; word-break: break-all; border: 0; }
  .url:hover { border-bottom: 1px solid var(--accent); }
  .muted { color: var(--ink-soft); font-size: 0.85rem; }
  .small { font-size: 0.85rem; }
  .error { color: var(--danger); font-size: 0.85rem; }

  .state {
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    border-bottom: 2px solid var(--ink-faint);
    padding-bottom: 1px;
  }
  .state.on {
    color: var(--success);
    border-bottom-color: var(--success);
  }

  .row-actions {
    text-align: right;
    white-space: nowrap;
  }
  button.link {
    background: none;
    border: 0;
    color: var(--ink-soft);
    font-size: 0.85rem;
    padding: 4px 8px;
    cursor: pointer;
    border-radius: var(--radius);
  }
  button.link:hover:not(:disabled) {
    background: var(--accent-bg);
    color: var(--accent);
  }
  button.link.danger:hover:not(:disabled) {
    background: var(--danger-bg);
    color: var(--danger);
  }
  button.link:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }
</style>
