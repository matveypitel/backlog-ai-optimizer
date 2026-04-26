<script lang="ts">
  import { embeddingsApi, ApiError } from '$lib/api';
  import type { EmbeddingSyncResult } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Button from '$lib/components/Button.svelte';
  import { toast } from '$lib/stores/toast';

  let lastJira = $state<EmbeddingSyncResult | null>(null);
  let lastFeatures = $state<EmbeddingSyncResult | null>(null);
  let runningJira = $state(false);
  let runningFeatures = $state(false);

  async function syncJira() {
    runningJira = true;
    try {
      lastJira = await embeddingsApi.syncJira();
      toast.success(`Embedded ${lastJira.embeddedCount} Jira issues.`);
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      runningJira = false;
    }
  }

  async function syncFeatures() {
    runningFeatures = true;
    try {
      lastFeatures = await embeddingsApi.syncFeatures();
      toast.success(`Embedded ${lastFeatures.embeddedCount} features.`);
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      runningFeatures = false;
    }
  }
</script>

<svelte:head><title>Embeddings · Admin</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">Admin / Embeddings</span>
  <h1>Vector index maintenance.</h1>
  <p class="lede">Re-run after new Jira issues or competitor features have been added.</p>
</header>

<div class="grid">
  <Card title="Jira issues">
    <p>Embed any Jira issues that are new or have changed since last embedded.</p>
    <Button onclick={syncJira} loading={runningJira}>Sync Jira embeddings</Button>
    {#if lastJira}
      <p class="result">
        Embedded <strong>{lastJira.embeddedCount}</strong>, skipped {lastJira.skippedCount}.
        {#if lastJira.errors.length > 0}<span class="err">{lastJira.errors.length} errors.</span>{/if}
      </p>
    {/if}
  </Card>

  <Card title="Competitor features">
    <p>Embed competitor features extracted from scraped pages.</p>
    <Button onclick={syncFeatures} loading={runningFeatures}>Sync feature embeddings</Button>
    {#if lastFeatures}
      <p class="result">
        Embedded <strong>{lastFeatures.embeddedCount}</strong>, skipped {lastFeatures.skippedCount}.
        {#if lastFeatures.errors.length > 0}<span class="err">{lastFeatures.errors.length} errors.</span>{/if}
      </p>
    {/if}
  </Card>
</div>

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
  .grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
    gap: var(--space-4);
  }
  .result {
    margin-top: var(--space-3);
    font-size: 0.9rem;
    color: var(--ink-soft);
  }
  .err { color: var(--danger); }
</style>
