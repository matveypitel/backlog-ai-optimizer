<script lang="ts">
  import { embeddingsApi, ApiError } from '$lib/api';
  import type { EmbeddingSyncResult } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Button from '$lib/components/Button.svelte';
  import { toast } from '$lib/stores/toast';
  import { T } from '$lib/i18n';

  let lastJira = $state<EmbeddingSyncResult | null>(null);
  let lastFeatures = $state<EmbeddingSyncResult | null>(null);
  let runningJira = $state(false);
  let runningFeatures = $state(false);

  async function syncJira() {
    runningJira = true;
    try {
      lastJira = await embeddingsApi.syncJira();
      toast.success($T.admin.embeddings.embeddedJira.replace('{count}', String(lastJira.embeddedCount)));
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
    } finally {
      runningJira = false;
    }
  }

  async function syncFeatures() {
    runningFeatures = true;
    try {
      lastFeatures = await embeddingsApi.syncFeatures();
      toast.success($T.admin.embeddings.embeddedFeatures.replace('{count}', String(lastFeatures.embeddedCount)));
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
    } finally {
      runningFeatures = false;
    }
  }
</script>

<svelte:head><title>{$T.admin.embeddings.title}</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">{$T.admin.embeddings.eyebrow}</span>
  <h1>{$T.admin.embeddings.heading}</h1>
  <p class="lede">{$T.admin.embeddings.lede}</p>
</header>

<div class="grid">
  <Card title={$T.admin.embeddings.jiraCard}>
    <p>{$T.admin.embeddings.jiraDesc}</p>
    <Button onclick={syncJira} loading={runningJira}>{$T.admin.embeddings.syncJira}</Button>
    {#if lastJira}
      <p class="result">
        {$T.admin.embeddings.embedded} <strong>{lastJira.embeddedCount}</strong>, {$T.admin.embeddings.skipped} {lastJira.skippedCount}.
        {#if lastJira.errors.length > 0}<span class="err">{lastJira.errors.length} {$T.admin.embeddings.errorsCount}</span>{/if}
      </p>
    {/if}
  </Card>

  <Card title={$T.admin.embeddings.featuresCard}>
    <p>{$T.admin.embeddings.featuresDesc}</p>
    <Button onclick={syncFeatures} loading={runningFeatures}>{$T.admin.embeddings.syncFeatures}</Button>
    {#if lastFeatures}
      <p class="result">
        {$T.admin.embeddings.embedded} <strong>{lastFeatures.embeddedCount}</strong>, {$T.admin.embeddings.skipped} {lastFeatures.skippedCount}.
        {#if lastFeatures.errors.length > 0}<span class="err">{lastFeatures.errors.length} {$T.admin.embeddings.errorsCount}</span>{/if}
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
