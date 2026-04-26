<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { promptsApi, ApiError } from '$lib/api';
  import type { PromptTemplate } from '$lib/types';
  import Button from '$lib/components/Button.svelte';
  import { toast } from '$lib/stores/toast';
  import { formatRelative } from '$lib/utils/format';

  let prompt = $state<PromptTemplate | null>(null);
  let instructions = $state('');
  let saving = $state(false);
  let resetting = $state(false);

  const labels: Record<string, string> = {
    FeatureExtraction: 'Feature extraction',
    FeatureSuggestion: 'Feature suggestion',
    Reprioritization: 'Reprioritization'
  };

  async function load() {
    try {
      prompt = await promptsApi.get($page.params.type);
      instructions = prompt.instructions;
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    }
  }

  async function save() {
    saving = true;
    try {
      await promptsApi.update($page.params.type, instructions);
      toast.success('Saved.');
      await load();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      saving = false;
    }
  }

  async function reset() {
    if (!confirm('Reset instructions to the default? Your edits will be lost.')) return;
    resetting = true;
    try {
      await promptsApi.reset($page.params.type);
      toast.success('Reset to default.');
      await load();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      resetting = false;
    }
  }

  let dirty = $derived(prompt !== null && instructions !== prompt.instructions);

  onMount(load);
</script>

<svelte:head><title>{prompt ? labels[prompt.type] ?? prompt.type : 'Prompt'} · Admin</title></svelte:head>

<a class="back" href="/admin/prompts">← Prompts</a>

{#if !prompt}
  <p class="muted">Loading…</p>
{:else}
  <header class="page-head">
    <span class="eyebrow">Admin / Prompts / {labels[prompt.type] ?? prompt.type}</span>
    <h1>{labels[prompt.type] ?? prompt.type}</h1>
    {#if prompt.updatedAt}
      <p class="when">Last updated {formatRelative(prompt.updatedAt)}</p>
    {:else}
      <p class="when">Currently using the default.</p>
    {/if}
  </header>

  <div class="split">
    <div class="pane">
      <div class="pane-head">
        <h2>Instructions</h2>
        <span class="meta editable">Editable</span>
      </div>
      <p class="hint">
        How the model should think about the task — tone, focus, what to avoid. This is what admins
        tweak.
      </p>
      <textarea bind:value={instructions} spellcheck="false"></textarea>
      <div class="actions">
        <Button onclick={save} loading={saving} disabled={!dirty}>Save</Button>
        <Button variant="danger" onclick={reset} loading={resetting}>Reset to default</Button>
        {#if dirty}<span class="dirty">Unsaved changes</span>{/if}
      </div>
    </div>

    <div class="pane">
      <div class="pane-head">
        <h2>Response schema</h2>
        <span class="meta locked">Locked</span>
      </div>
      <p class="hint">
        Hard-coded JSON contract. Always appended after the instructions. The structure of the output
        cannot be changed without a code change.
      </p>
      <pre class="schema">{prompt.schema}</pre>
    </div>
  </div>
{/if}

<style>
  .back {
    display: inline-block;
    color: var(--ink-soft);
    font-size: 0.9rem;
    margin-bottom: var(--space-4);
    border: 0;
  }
  .back:hover { border: 0; color: var(--ink); }
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
  .when {
    color: var(--ink-soft);
    font-size: 0.9rem;
    margin: 0;
  }
  .muted { color: var(--ink-soft); }

  .split {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-5);
  }
  @media (max-width: 960px) {
    .split { grid-template-columns: 1fr; }
  }

  .pane {
    background: var(--bg-elev);
    border: var(--hairline);
    border-radius: var(--radius);
    padding: var(--space-4);
  }
  .pane-head {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--space-2);
  }
  .pane-head h2 {
    font-size: 1rem;
    margin: 0;
  }
  .meta {
    font-family: var(--font-mono);
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    padding: 2px 6px;
    border: 1px solid;
    border-radius: var(--radius);
  }
  .meta.editable { color: var(--success); border-color: var(--success); }
  .meta.locked { color: var(--ink-faint); border-color: var(--ink-faint); }
  .hint {
    font-size: 0.85rem;
    color: var(--ink-soft);
    margin-bottom: var(--space-3);
  }
  textarea {
    min-height: 480px;
    background: var(--bg);
  }
  .schema {
    font-family: var(--font-mono);
    font-size: 12.5px;
    line-height: 1.6;
    background: var(--bg-sunk);
    padding: var(--space-4);
    border-radius: var(--radius);
    overflow: auto;
    margin: 0;
    max-height: 480px;
    white-space: pre-wrap;
    word-break: break-word;
    color: var(--ink-soft);
  }
  .actions {
    display: flex;
    gap: var(--space-3);
    align-items: center;
    margin-top: var(--space-3);
  }
  .dirty {
    margin-left: auto;
    font-size: 0.8rem;
    color: var(--warning);
    font-family: var(--font-mono);
  }
</style>
