<script lang="ts">
  import { onMount } from 'svelte';
  import {
    suggestionsApi,
    reprioritizationApi,
    competitorsApi,
    syncApi,
    ApiError
  } from '$lib/api';
  import Card from '$lib/components/Card.svelte';
  import { formatRelative } from '$lib/utils/format';
  import { currentUser } from '$lib/stores/auth';
  import { T } from '$lib/i18n';

  let suggestionsCount = $state<number | null>(null);
  let highConfidenceReprio = $state<number | null>(null);
  let featuresCount = $state<number | null>(null);
  let lastSync = $state<string | null>(null);
  let totalIssues = $state<number | null>(null);
  let error = $state<string | null>(null);

  onMount(async () => {
    try {
      const [s, r, f, sync] = await Promise.all([
        suggestionsApi.list(),
        reprioritizationApi.list(),
        competitorsApi.list(),
        syncApi.status()
      ]);
      suggestionsCount = s.length;
      highConfidenceReprio = r.filter((x) => x.confidenceScore >= 0.7).length;
      featuresCount = f.length;
      lastSync = sync.lastSyncedAt;
      totalIssues = sync.totalIssues;
    } catch (err) {
      error = err instanceof ApiError ? err.detail ?? err.message : $T.dashboard.failedToLoad;
    }
  });
</script>

<svelte:head><title>{$T.dashboard.title}</title></svelte:head>

<header class="hero">
  <span class="eyebrow">{$T.dashboard.eyebrow}</span>
  <h1>{$T.dashboard.greeting}{$currentUser?.email ? `, ${$currentUser.email.split('@')[0]}` : ''}.</h1>
  <p class="lede">{$T.dashboard.lede}</p>
</header>

{#if error}<p class="error">{error}</p>{/if}

<section class="grid">
  <a class="tile" href="/suggestions">
    <Card eyebrow={$T.dashboard.suggestions} title={suggestionsCount === null ? '—' : String(suggestionsCount)}>
      <p>{$T.dashboard.suggestionsSub}</p>
    </Card>
  </a>

  <a class="tile" href="/reprioritization">
    <Card
      eyebrow={$T.dashboard.reprio}
      title={highConfidenceReprio === null ? '—' : String(highConfidenceReprio)}
    >
      <p>{$T.dashboard.reprioSub}</p>
    </Card>
  </a>

  <a class="tile" href="/competitors">
    <Card eyebrow={$T.dashboard.competitors} title={featuresCount === null ? '—' : String(featuresCount)}>
      <p>{$T.dashboard.competitorsSub}</p>
    </Card>
  </a>

  <a class="tile" href="/jira">
    <Card eyebrow={$T.dashboard.jiraIssues} title={totalIssues === null ? '—' : String(totalIssues)}>
      <p>{$T.dashboard.lastSync} {formatRelative(lastSync)}.</p>
    </Card>
  </a>
</section>

<style>
  .hero {
    margin-bottom: var(--space-7);
    max-width: var(--reading-width);
  }
  .eyebrow {
    font-family: var(--font-mono);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    display: block;
    margin-bottom: var(--space-3);
  }
  h1 {
    font-size: 2.6rem;
    margin-bottom: var(--space-3);
  }
  .lede {
    color: var(--ink-soft);
    font-size: 1.1rem;
  }
  .grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
    gap: var(--space-4);
  }
  .tile {
    color: inherit;
    border: 0;
    transition: transform 120ms ease;
  }
  .tile:hover {
    border: 0;
    transform: translateY(-1px);
  }
  .error {
    color: var(--danger);
    margin-bottom: var(--space-4);
  }
</style>
