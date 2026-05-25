<script lang="ts">
  import { onMount } from 'svelte';
  import { competitorsApi, ApiError } from '$lib/api';
  import type { CompetitorFeature } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Tag from '$lib/components/Tag.svelte';
  import EmptyState from '$lib/components/EmptyState.svelte';
  import { toast } from '$lib/stores/toast';
  import { T } from '$lib/i18n';
  import { formatRelative, truncate } from '$lib/utils/format';

  let items = $state<CompetitorFeature[]>([]);
  let loading = $state(true);
  let search = $state('');
  let timer: ReturnType<typeof setTimeout> | null = null;

  async function load() {
    loading = true;
    try {
      items = await competitorsApi.list(search || undefined);
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failedToLoad);
    } finally {
      loading = false;
    }
  }

  function onSearch() {
    if (timer) clearTimeout(timer);
    timer = setTimeout(load, 300);
  }

  onMount(load);

  let grouped = $derived.by(() => {
    const m = new Map<string, CompetitorFeature[]>();
    for (const f of items) {
      const key = f.sourceUrl;
      const list = m.get(key) ?? [];
      list.push(f);
      m.set(key, list);
    }
    return Array.from(m.entries()).map(([url, list]) => ({
      url,
      title: list[0]?.sourceTitle ?? null,
      features: list
    }));
  });
</script>

<svelte:head><title>{$T.competitors.title}</title></svelte:head>

<header class="page-head">
  <div>
    <span class="eyebrow">{$T.competitors.eyebrow}</span>
    <h1>{$T.competitors.heading}</h1>
  </div>
  <input
    type="search"
    placeholder={$T.competitors.searchPlaceholder}
    bind:value={search}
    oninput={onSearch}
    class="search"
  />
</header>

{#if loading}
  <p class="muted">{$T.common.loading}</p>
{:else if items.length === 0}
  <EmptyState title={$T.competitors.noFeaturesTitle} description={$T.competitors.noFeaturesDesc} />
{:else}
  <div class="groups">
    {#each grouped as g}
      <section class="group">
        <header class="group-head">
          <h2>{g.title || g.url}</h2>
          <a href={g.url} target="_blank" rel="noopener noreferrer" class="source mono">{g.url}</a>
        </header>
        <div class="features">
          {#each g.features as f}
            <a class="feature" href="/competitors/features/{f.id}">
              <Card>
                <div class="row">
                  <h3>{f.name}</h3>
                  {#if f.category}<Tag>{f.category}</Tag>{/if}
                </div>
                <p>{truncate(f.description, 180)}</p>
                <span class="when">{$T.competitors.extracted} {formatRelative(f.extractedAt)}</span>
              </Card>
            </a>
          {/each}
        </div>
      </section>
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
  .search {
    max-width: 280px;
  }
  .muted { color: var(--ink-soft); }
  .groups {
    display: flex;
    flex-direction: column;
    gap: var(--space-7);
  }
  .group-head {
    margin-bottom: var(--space-4);
    padding-bottom: var(--space-3);
    border-bottom: var(--hairline);
  }
  .group-head h2 {
    margin-bottom: var(--space-2);
  }
  .source {
    font-size: 0.8rem;
    color: var(--ink-faint);
    border: 0;
  }
  .features {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    gap: var(--space-3);
  }
  .feature {
    color: inherit;
    border: 0;
  }
  .feature:hover {
    border: 0;
  }
  .row {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: var(--space-3);
    margin-bottom: var(--space-2);
  }
  .row h3 {
    margin: 0;
    font-size: 1.05rem;
  }
  .when {
    display: block;
    margin-top: var(--space-3);
    font-family: var(--font-mono);
    font-size: 0.75rem;
    color: var(--ink-faint);
  }
</style>
