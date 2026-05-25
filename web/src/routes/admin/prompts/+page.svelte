<script lang="ts">
  import { onMount } from 'svelte';
  import { promptsApi, ApiError } from '$lib/api';
  import type { PromptTemplate } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import { toast } from '$lib/stores/toast';
  import { T } from '$lib/i18n';
  import { formatRelative, truncate } from '$lib/utils/format';

  let prompts = $state<PromptTemplate[]>([]);
  let loading = $state(true);

  let labels = $derived<Record<string, string>>({
    FeatureExtraction: $T.admin.prompts.featureExtraction,
    FeatureSuggestion: $T.admin.prompts.featureSuggestion,
    Reprioritization: $T.admin.prompts.reprioritization
  });

  let subtitles = $derived<Record<string, string>>({
    FeatureExtraction: $T.admin.prompts.featureExtractionSub,
    FeatureSuggestion: $T.admin.prompts.featureSuggestionSub,
    Reprioritization: $T.admin.prompts.reprioritizationSub
  });

  onMount(async () => {
    try {
      prompts = await promptsApi.list();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
    } finally {
      loading = false;
    }
  });
</script>

<svelte:head><title>{$T.admin.prompts.title}</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">{$T.admin.prompts.eyebrow}</span>
  <h1>{$T.admin.prompts.heading}</h1>
  <p class="lede">{$T.admin.prompts.lede}</p>
</header>

{#if loading}
  <p class="muted">{$T.common.loading}</p>
{:else}
  <div class="grid">
    {#each prompts as p}
      <a class="entry" href="/admin/prompts/{p.type}">
        <Card eyebrow={labels[p.type] ?? p.type} title={subtitles[p.type] ?? ''}>
          <p class="preview">{truncate(p.instructions, 240)}</p>
          <span class="when">
            {p.updatedAt ? `${$T.common.updated} ${formatRelative(p.updatedAt)}` : $T.common.default}
          </span>
        </Card>
      </a>
    {/each}
  </div>
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
  .lede { color: var(--ink-soft); font-size: 1rem; }
  .muted { color: var(--ink-soft); }
  .grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: var(--space-4);
  }
  .entry { color: inherit; border: 0; }
  .entry:hover { border: 0; }
  .preview {
    color: var(--ink-soft);
    font-size: 0.9rem;
    line-height: 1.5;
    margin-bottom: var(--space-3);
  }
  .when {
    font-family: var(--font-mono);
    font-size: 0.75rem;
    color: var(--ink-faint);
  }
</style>
