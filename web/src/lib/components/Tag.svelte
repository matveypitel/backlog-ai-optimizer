<script lang="ts">
  interface Props {
    children?: import('svelte').Snippet;
  }
  let { children }: Props = $props();
</script>

<span class="tag">{@render children?.()}</span>

<style>
  .tag {
    display: inline-block;
    background: var(--accent-bg);
    color: var(--accent);
    padding: 2px var(--space-2);
    font-size: 0.78rem;
    border-radius: var(--radius);
    line-height: 1.4;
  }
</style>
