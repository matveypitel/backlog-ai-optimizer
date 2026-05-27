<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { scrapingApi, ApiError } from '$lib/api';
  import type { ScrapingJob, ScrapingSource } from '$lib/types';
  import Field from '$lib/components/Field.svelte';
  import Button from '$lib/components/Button.svelte';
  import StatusBadge from '$lib/components/StatusBadge.svelte';
  import EmptyState from '$lib/components/EmptyState.svelte';
  import { toast } from '$lib/stores/toast';
  import { T } from '$lib/i18n';
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
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.scraping.failedToLoadSources);
    }
  }

  async function loadJobs() {
    try {
      jobs = await scrapingApi.jobs();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.scraping.failedToLoadJobs);
    }
  }

  async function addSource(e: SubmitEvent) {
    e.preventDefault();
    if (!newUrl) return;
    addingSource = true;
    try {
      await scrapingApi.sources.create(newUrl, newName || null);
      toast.success($T.scraping.sourceAdded);
      newUrl = '';
      newName = '';
      await loadSources();
    } catch (err) {
      const msg = err instanceof ApiError ? err.detail ?? err.message : $T.common.failed;
      toast.error(msg);
    } finally {
      addingSource = false;
    }
  }

  async function syncAll() {
    syncingAll = true;
    try {
      const r = await scrapingApi.sources.syncAll();
      if (r.enqueued === 0 && r.skipped === 0) toast.info($T.scraping.nothingToSync);
      else toast.success(`${$T.scraping.queued} ${r.enqueued}${r.skipped > 0 ? `, ${r.skipped} ${$T.scraping.alreadyInFlight}` : ''}.`);
      await loadJobs();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
    } finally {
      syncingAll = false;
    }
  }

  async function syncOne(s: ScrapingSource) {
    busyId = s.id;
    try {
      await scrapingApi.sources.syncOne(s.id);
      toast.success(`${$T.scraping.queued} ${s.name ?? s.url}.`);
      await loadJobs();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
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
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
    } finally {
      busyId = null;
    }
  }

  async function removeSource(s: ScrapingSource) {
    const msg = $T.scraping.removeConfirm.replace('{name}', s.name ?? s.url);
    if (!confirm(msg)) return;
    busyId = s.id;
    try {
      await scrapingApi.sources.remove(s.id);
      toast.success($T.scraping.removed);
      await loadSources();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
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
      toast.success(`${$T.scraping.queued}.`);
      adhocUrl = '';
      await loadJobs();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
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

<svelte:head><title>{$T.scraping.title}</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">{$T.scraping.eyebrow}</span>
  <h1>{$T.scraping.heading}</h1>
  <p class="lede">
    {$T.scraping.lede} <em>{$T.scraping.ledeEmph}</em> {$T.scraping.ledeRest}
  </p>
</header>

<section class="block">
  <div class="block-head">
    <h2>{$T.scraping.sourcesHeading} <span class="count">({sources.length})</span></h2>
    <Button onclick={syncAll} loading={syncingAll} disabled={activeCount === 0}>
      {$T.scraping.syncAllActive}
    </Button>
  </div>

  <form onsubmit={addSource} class="add-form">
    <div class="row">
      <Field label={$T.scraping.urlLabel} name="newUrl" placeholder="https://competitor.com/features" required bind:value={newUrl} />
      <Field label={$T.scraping.nameLabel} name="newName" placeholder={$T.scraping.namePlaceholder} bind:value={newName} />
      <Button type="submit" loading={addingSource}>{$T.common.add}</Button>
    </div>
  </form>

  {#if sources.length === 0}
    <EmptyState title={$T.scraping.noSourcesTitle} description={$T.scraping.noSourcesDesc} />
  {:else}
    <div class="table-scroll"><table>
      <thead>
        <tr>
          <th>{$T.scraping.colName}</th>
          <th>{$T.scraping.urlLabel}</th>
          <th>{$T.scraping.colStatus}</th>
          <th>{$T.scraping.colLastScraped}</th>
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
                {s.isActive ? $T.scraping.statusActive : $T.scraping.statusPaused}
              </span>
            </td>
            <td class="muted">{formatRelative(s.lastScrapedAt)}</td>
            <td class="row-actions">
              <button class="link" onclick={() => syncOne(s)} disabled={busyId === s.id || !s.isActive}>
                {$T.scraping.sync}
              </button>
              <button class="link" onclick={() => toggleActive(s)} disabled={busyId === s.id}>
                {s.isActive ? $T.scraping.pause : $T.scraping.resume}
              </button>
              <button class="link danger" onclick={() => removeSource(s)} disabled={busyId === s.id}>
                {$T.common.remove}
              </button>
            </td>
          </tr>
        {/each}
      </tbody>
    </table></div>
  {/if}
</section>

<section class="block">
  <h2>{$T.scraping.oneOffHeading}</h2>
  <p class="muted small">{$T.scraping.oneOffLede}</p>
  <form onsubmit={adhocEnqueue} class="adhoc-form">
    <div class="row">
      <Field label={$T.scraping.urlLabel} name="adhocUrl" placeholder="https://example.com/page" required bind:value={adhocUrl} />
      <Button type="submit" loading={adhocSubmitting}>{$T.common.queue}</Button>
    </div>
  </form>
</section>

<section class="block">
  <h2>{$T.scraping.recentJobsHeading}</h2>
  {#if jobs.length === 0}
    <p class="muted">{$T.scraping.noJobsYet}</p>
  {:else}
    <div class="table-scroll"><table>
      <thead>
        <tr>
          <th>{$T.jira.colStatus}</th>
          <th>{$T.scraping.urlLabel}</th>
          <th>{$T.scraping.colAttempts}</th>
          <th>{$T.scraping.colCreated}</th>
          <th>{$T.scraping.colError}</th>
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
    </table></div>
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
    flex-wrap: wrap;
  }
  .row > :global(.field) { flex: 1; min-width: 180px; max-width: 380px; margin-bottom: 0; }

  .table-scroll {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }
  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.9rem;
    min-width: 540px;
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
  @media (max-width: 600px) {
    .row {
      flex-direction: column;
      align-items: stretch;
    }
    .row > :global(.field) { max-width: 100%; }
    .block-head {
      flex-direction: column;
      align-items: flex-start;
      gap: var(--space-2);
    }
  }
</style>
