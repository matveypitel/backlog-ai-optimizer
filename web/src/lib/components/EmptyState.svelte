<script lang="ts">
  interface Props {
    title: string;
    description?: string;
    children?: import('svelte').Snippet;
  }
  let { title, description, children }: Props = $props();
</script>

<div class="empty">
  <h3>{title}</h3>
  {#if description}<p>{description}</p>{/if}
  {@render children?.()}
</div>

<style>
  .empty {
    text-align: center;
    padding: var(--space-7) var(--space-5);
    border: 1px dashed var(--rule);
    border-radius: var(--radius);
    background: var(--bg-elev);
  }
  h3 {
    margin-bottom: var(--space-2);
  }
  p {
    color: var(--ink-soft);
    margin-bottom: var(--space-4);
  }
</style>
