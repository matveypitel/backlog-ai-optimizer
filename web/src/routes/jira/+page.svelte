<script lang="ts">
  import { onMount } from 'svelte';
  import { jiraApi, ApiError } from '$lib/api';
  import type { JiraIssue } from '$lib/types';
  import StatusBadge from '$lib/components/StatusBadge.svelte';
  import EmptyState from '$lib/components/EmptyState.svelte';
  import { toast } from '$lib/stores/toast';
  import { formatRelative } from '$lib/utils/format';

  let items = $state<JiraIssue[]>([]);
  let loading = $state(true);
  let search = $state('');
  let status = $state('');
  let priority = $state('');
  let timer: ReturnType<typeof setTimeout> | null = null;

  async function load() {
    loading = true;
    try {
      items = await jiraApi.list({ search, status, priority });
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed to load');
    } finally {
      loading = false;
    }
  }

  function debouncedLoad() {
    if (timer) clearTimeout(timer);
    timer = setTimeout(load, 300);
  }

  onMount(load);

  let statuses = $derived([...new Set(items.map((i) => i.status))]);
  let priorities = $derived([...new Set(items.map((i) => i.priority).filter(Boolean))] as string[]);
</script>

<svelte:head><title>Jira · Backlog AI</title></svelte:head>

<header class="page-head">
  <div>
    <span class="eyebrow">Jira issues</span>
    <h1>The current backlog.</h1>
  </div>
</header>

<div class="filters">
  <input type="search" placeholder="Search key or summary…" bind:value={search} oninput={debouncedLoad} />
  <select bind:value={status} onchange={load}>
    <option value="">All statuses</option>
    {#each statuses as s}<option value={s}>{s}</option>{/each}
  </select>
  <select bind:value={priority} onchange={load}>
    <option value="">All priorities</option>
    {#each priorities as p}<option value={p}>{p}</option>{/each}
  </select>
</div>

{#if loading && items.length === 0}
  <p class="muted">Loading…</p>
{:else if items.length === 0}
  <EmptyState title="No issues" description="Sync from Jira from the admin panel." />
{:else}
  <table>
    <thead>
      <tr>
        <th>Key</th>
        <th>Summary</th>
        <th>Status</th>
        <th>Priority</th>
        <th>Type</th>
        <th>Updated</th>
      </tr>
    </thead>
    <tbody>
      {#each items as i}
        <tr>
          <td class="mono key">
            {#if i.jiraUrl}
              <a href={i.jiraUrl} target="_blank" rel="noopener">{i.jiraKey}</a>
            {:else}
              {i.jiraKey}
            {/if}
          </td>
          <td class="summary">{i.summary}</td>
          <td><StatusBadge status={i.status} /></td>
          <td>{#if i.priority}<StatusBadge status={i.priority} />{:else}<span class="dim">—</span>{/if}</td>
          <td class="muted">{i.issueType}</td>
          <td class="muted">{formatRelative(i.jiraUpdatedAt)}</td>
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
  .filters {
    display: flex;
    gap: var(--space-3);
    margin-bottom: var(--space-5);
    flex-wrap: wrap;
  }
  .filters input { max-width: 320px; }
  .filters select { max-width: 200px; }
  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.92rem;
  }
  th, td {
    text-align: left;
    padding: var(--space-3) var(--space-3);
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
  .key { white-space: nowrap; }
  .summary { max-width: 480px; }
  .muted { color: var(--ink-soft); font-size: 0.85rem; }
  .dim { color: var(--ink-faint); }
  tr:hover td { background: var(--bg-elev); }
</style>
