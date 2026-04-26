<script lang="ts">
  interface Props {
    title?: string;
    eyebrow?: string;
    children?: import('svelte').Snippet;
    actions?: import('svelte').Snippet;
  }

  let { title, eyebrow, children, actions }: Props = $props();
</script>

<article class="card">
  {#if title || eyebrow || actions}
    <header>
      <div>
        {#if eyebrow}<span class="eyebrow">{eyebrow}</span>{/if}
        {#if title}<h3>{title}</h3>{/if}
      </div>
      {#if actions}<div class="actions">{@render actions()}</div>{/if}
    </header>
  {/if}
  <div class="body">
    {@render children?.()}
  </div>
</article>

<style>
  .card {
    background: var(--bg-elev);
    border: var(--hairline);
    border-radius: var(--radius);
    padding: var(--space-5);
  }
  header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: var(--space-4);
    margin-bottom: var(--space-4);
  }
  .eyebrow {
    display: block;
    font-family: var(--font-mono);
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    margin-bottom: var(--space-2);
  }
  h3 {
    margin: 0;
  }
  .actions {
    display: flex;
    gap: var(--space-2);
    flex-shrink: 0;
  }
  .body :global(p:last-child) {
    margin-bottom: 0;
  }
</style>
