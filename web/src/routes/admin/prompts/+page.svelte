<script lang="ts">
  import { onMount } from 'svelte';
  import { promptsApi, ApiError } from '$lib/api';
  import type { PromptTemplate } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Button from '$lib/components/Button.svelte';
  import { toast } from '$lib/stores/toast';
  import { formatRelative, truncate } from '$lib/utils/format';

  let prompts = $state<PromptTemplate[]>([]);
  let loading = $state(true);

  const labels: Record<string, string> = {
    FeatureExtraction: 'Feature extraction',
    FeatureSuggestion: 'Feature suggestion',
    Reprioritization: 'Reprioritization'
  };

  const subtitles: Record<string, string> = {
    FeatureExtraction: 'Used when distilling competitor pages into features.',
    FeatureSuggestion: 'Turns competitive gaps into backlog items.',
    Reprioritization: 'Decides whether priorities should change.'
  };

  onMount(async () => {
    try {
      prompts = await promptsApi.list();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      loading = false;
    }
  });
</script>

<svelte:head><title>Prompts · Admin</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">Admin / Prompts</span>
  <h1>The voice of the analysis.</h1>
  <p class="lede">
    Edit the instructions that shape how the LLM reasons. The response schema is fixed in code and
    appended automatically — it is not editable, so the structure of the output is always preserved.
  </p>
</header>

{#if loading}
  <p class="muted">Loading…</p>
{:else}
  <div class="grid">
    {#each prompts as p}
      <a class="entry" href="/admin/prompts/{p.type}">
        <Card eyebrow={labels[p.type] ?? p.type} title={subtitles[p.type] ?? ''}>
          <p class="preview">{truncate(p.instructions, 240)}</p>
          <span class="when">
            {p.updatedAt ? `Updated ${formatRelative(p.updatedAt)}` : 'Default'}
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
