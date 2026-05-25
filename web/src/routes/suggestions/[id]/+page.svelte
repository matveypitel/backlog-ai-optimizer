<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { suggestionsApi, ApiError } from '$lib/api';
  import type { FeatureSuggestion } from '$lib/types';
  import StatusBadge from '$lib/components/StatusBadge.svelte';
  import Tag from '$lib/components/Tag.svelte';
  import { T } from '$lib/i18n';
  import { formatDate, parseJsonArray } from '$lib/utils/format';

  let item = $state<FeatureSuggestion | null>(null);
  let error = $state<string | null>(null);

  onMount(async () => {
    try {
      item = await suggestionsApi.get($page.params.id);
    } catch (err) {
      error = err instanceof ApiError ? err.detail ?? err.message : $T.common.failedToLoad;
    }
  });

  let stories = $derived(item ? parseJsonArray(item.userStories) : []);
  let criteria = $derived(item ? parseJsonArray(item.acceptanceCriteria) : []);
  let tags = $derived(item ? parseJsonArray(item.tags) : []);
</script>

<svelte:head><title>{item?.title ?? $T.suggestions.eyebrow} · Backlog AI</title></svelte:head>

<a class="back" href="/suggestions">{$T.suggestions_detail.backLink}</a>

{#if error}
  <p class="error">{error}</p>
{:else if !item}
  <p class="muted">{$T.common.loading}</p>
{:else}
  <article class="detail">
    <header>
      <span class="eyebrow">{item.issueType} · {formatDate(item.analyzedAt)}</span>
      <h1>{item.title}</h1>
      <div class="badges">
        <StatusBadge status={item.suggestedPriority} />
        {#if item.estimatedImpact}
          <span class="impact">{$T.suggestions_detail.impact}: {item.estimatedImpact}</span>
        {/if}
      </div>
    </header>

    <p class="desc">{item.description}</p>

    {#if item.businessValue}
      <section>
        <h2>{$T.suggestions_detail.businessValue}</h2>
        <p>{item.businessValue}</p>
      </section>
    {/if}

    {#if stories.length > 0}
      <section>
        <h2>{$T.suggestions_detail.userStories}</h2>
        <ul class="stories">
          {#each stories as s}<li>{s}</li>{/each}
        </ul>
      </section>
    {/if}

    {#if criteria.length > 0}
      <section>
        <h2>{$T.suggestions_detail.acceptanceCriteria}</h2>
        <ul class="ac">
          {#each criteria as c}<li>{c}</li>{/each}
        </ul>
      </section>
    {/if}

    {#if item.reasoning}
      <section>
        <h2>{$T.suggestions_detail.reasoning}</h2>
        <p>{item.reasoning}</p>
      </section>
    {/if}

    {#if item.competitorEvidence}
      <section>
        <h2>{$T.suggestions_detail.competitorEvidence}</h2>
        <p class="evidence">{item.competitorEvidence}</p>
      </section>
    {/if}

    {#if tags.length > 0}
      <section class="tags-section">
        {#each tags as t}<Tag>{t}</Tag>{/each}
      </section>
    {/if}
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
  .back:hover {
    border: 0;
    color: var(--ink);
  }
  .detail {
    max-width: var(--reading-width);
  }
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
    margin-bottom: var(--space-3);
  }
  .badges {
    display: flex;
    gap: var(--space-3);
    align-items: center;
    flex-wrap: wrap;
  }
  .impact {
    color: var(--ink-soft);
    font-size: 0.9rem;
  }
  .desc {
    font-size: 1.1rem;
    color: var(--ink);
    margin-bottom: var(--space-6);
  }
  section {
    margin-bottom: var(--space-6);
  }
  h2 {
    font-size: 1.05rem;
    margin-bottom: var(--space-3);
    padding-bottom: var(--space-2);
    border-bottom: 1px solid var(--rule-soft);
  }
  ul {
    margin: 0;
    padding-left: var(--space-5);
  }
  ul li {
    margin-bottom: var(--space-2);
  }
  .stories li {
    font-style: italic;
    color: var(--ink-soft);
  }
  .ac li {
    list-style: none;
    padding-left: var(--space-4);
    position: relative;
  }
  .ac li::before {
    content: '✓';
    position: absolute;
    left: 0;
    color: var(--success);
    font-family: var(--font-mono);
  }
  .evidence {
    font-family: var(--font-mono);
    font-size: 0.85rem;
    color: var(--ink-soft);
    background: var(--bg-sunk);
    padding: var(--space-3) var(--space-4);
    border-radius: var(--radius);
  }
  .tags-section {
    display: flex;
    gap: var(--space-2);
    flex-wrap: wrap;
  }
  .error {
    color: var(--danger);
  }
  .muted {
    color: var(--ink-soft);
  }
</style>
