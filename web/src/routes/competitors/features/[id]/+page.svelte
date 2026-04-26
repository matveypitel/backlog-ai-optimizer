<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { competitorsApi, ApiError } from '$lib/api';
  import type { CompetitorFeature } from '$lib/types';
  import Tag from '$lib/components/Tag.svelte';
  import { formatDate, parseJsonArray } from '$lib/utils/format';

  let item = $state<CompetitorFeature | null>(null);
  let error = $state<string | null>(null);

  onMount(async () => {
    try {
      item = await competitorsApi.get($page.params.id);
    } catch (err) {
      error = err instanceof ApiError ? err.detail ?? err.message : 'Failed to load';
    }
  });

  let benefits = $derived(item ? parseJsonArray(item.keyBenefits) : []);
  let useCases = $derived(item ? parseJsonArray(item.useCases) : []);
</script>

<svelte:head><title>{item?.name ?? 'Feature'} · Backlog AI</title></svelte:head>

<a class="back" href="/competitors">← Competitors</a>

{#if error}
  <p class="error">{error}</p>
{:else if !item}
  <p class="muted">Loading…</p>
{:else}
  <article class="detail">
    <header>
      <span class="eyebrow">
        {item.category ?? 'Competitor feature'} · extracted {formatDate(item.extractedAt)}
      </span>
      <h1>{item.name}</h1>
      {#if item.targetAudience}
        <p class="audience">For {item.targetAudience}</p>
      {/if}
    </header>

    <p class="desc">{item.description}</p>

    {#if benefits.length > 0}
      <section>
        <h2>Key benefits</h2>
        <ul class="bullet">
          {#each benefits as b}<li>{b}</li>{/each}
        </ul>
      </section>
    {/if}

    {#if useCases.length > 0}
      <section>
        <h2>Use cases</h2>
        <ul class="bullet">
          {#each useCases as u}<li>{u}</li>{/each}
        </ul>
      </section>
    {/if}

    {#if item.differentiators}
      <section>
        <h2>Differentiators</h2>
        <p>{item.differentiators}</p>
      </section>
    {/if}

    <section>
      <h2>Source</h2>
      <p>
        <a href={item.sourceUrl} target="_blank" rel="noopener noreferrer" class="mono">{item.sourceUrl}</a>
        {#if item.sourceTitle}<br /><span class="muted">{item.sourceTitle}</span>{/if}
      </p>
    </section>
  </article>
{/if}

<style>
  .back {
    display: inline-block;
    color: var(--ink-soft);
    font-size: 0.9rem;
    margin-bottom: var(--space-5);
    border: 0;
  }
  .back:hover { border: 0; color: var(--ink); }
  .detail { max-width: var(--reading-width); }
  header {
    margin-bottom: var(--space-6);
    padding-bottom: var(--space-5);
    border-bottom: var(--hairline);
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
  h1 {
    font-size: 2.2rem;
    line-height: 1.15;
    margin-bottom: var(--space-2);
  }
  .audience {
    color: var(--ink-soft);
    font-style: italic;
    margin-bottom: 0;
  }
  .desc {
    font-size: 1.1rem;
    margin-bottom: var(--space-6);
  }
  section { margin-bottom: var(--space-6); }
  h2 {
    font-size: 1.05rem;
    margin-bottom: var(--space-3);
    padding-bottom: var(--space-2);
    border-bottom: 1px solid var(--rule-soft);
  }
  ul.bullet {
    margin: 0;
    padding-left: var(--space-5);
  }
  ul.bullet li { margin-bottom: var(--space-2); }
  .muted { color: var(--ink-soft); }
  .error { color: var(--danger); }
</style>
