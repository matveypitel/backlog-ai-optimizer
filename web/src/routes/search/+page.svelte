<script lang="ts">
  import { embeddingsApi, ApiError } from '$lib/api';
  import type { SimilarityMatch } from '$lib/types';
  import Field from '$lib/components/Field.svelte';
  import Button from '$lib/components/Button.svelte';
  import Card from '$lib/components/Card.svelte';
  import { toast } from '$lib/stores/toast';

  let jiraKey = $state('');
  let topN = $state(5);
  let results = $state<SimilarityMatch[]>([]);
  let loading = $state(false);
  let searched = $state(false);

  async function submit(e: SubmitEvent) {
    e.preventDefault();
    if (!jiraKey) return;
    loading = true;
    try {
      results = await embeddingsApi.search(jiraKey, topN);
      searched = true;
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      loading = false;
    }
  }
</script>

<svelte:head><title>Search · Backlog AI</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">Similarity search</span>
  <h1>Find competitor features near a backlog item.</h1>
</header>

<form onsubmit={submit} class="search-form">
  <div class="row">
    <Field label="Jira key" name="jiraKey" placeholder="ABC-123" required bind:value={jiraKey} />
    <div class="topn">
      <label for="topN">Top N</label>
      <input id="topN" type="number" min="1" max="20" bind:value={topN} />
    </div>
    <Button type="submit" {loading}>Search</Button>
  </div>
</form>

{#if searched}
  {#if results.length === 0}
    <p class="muted">No similar features found.</p>
  {:else}
    <div class="list">
      {#each results as r}
        <Card>
          <div class="row-head">
            <h3>{r.name}</h3>
            <span class="score mono">similarity {r.similarity.toFixed(3)}</span>
          </div>
          <p>{r.description}</p>
          <a href={r.sourceUrl} target="_blank" rel="noopener noreferrer" class="mono source">
            {r.sourceUrl}
          </a>
        </Card>
      {/each}
    </div>
  {/if}
{/if}

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
  .search-form { margin-bottom: var(--space-6); }
  .row {
    display: flex;
    gap: var(--space-3);
    align-items: flex-end;
  }
  .row > :global(.field) { flex: 1; max-width: 320px; }
  .topn { width: 90px; }
  .list { display: flex; flex-direction: column; gap: var(--space-3); }
  .row-head {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: var(--space-2);
    gap: var(--space-3);
  }
  .row-head h3 { margin: 0; font-size: 1.05rem; }
  .score { font-size: 0.78rem; color: var(--ink-faint); }
  .source { font-size: 0.85rem; color: var(--ink-faint); border: 0; }
  .muted { color: var(--ink-soft); }
</style>
