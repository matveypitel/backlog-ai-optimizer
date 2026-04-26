<script lang="ts">
  interface Props {
    variant?: 'primary' | 'ghost' | 'danger';
    type?: 'button' | 'submit' | 'reset';
    href?: string;
    disabled?: boolean;
    loading?: boolean;
    onclick?: (e: MouseEvent) => void;
    children?: import('svelte').Snippet;
  }

  let {
    variant = 'primary',
    type = 'button',
    href,
    disabled = false,
    loading = false,
    onclick,
    children
  }: Props = $props();
</script>

{#if href}
  <a class="btn btn-{variant}" class:disabled href={disabled ? undefined : href}>
    {#if loading}<span class="spinner"></span>{/if}
    {@render children?.()}
  </a>
{:else}
  <button class="btn btn-{variant}" {type} disabled={disabled || loading} {onclick}>
    {#if loading}<span class="spinner"></span>{/if}
    {@render children?.()}
  </button>
{/if}

<style>
  .btn {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    padding: var(--space-3) var(--space-5);
    border: var(--hairline);
    border-radius: var(--radius);
    font-size: 0.9rem;
    font-weight: 500;
    cursor: pointer;
    transition: background 120ms ease, border-color 120ms ease, color 120ms ease;
    text-decoration: none;
    line-height: 1;
    white-space: nowrap;
  }
  .btn:disabled,
  .btn.disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  .btn-primary {
    background: var(--accent);
    color: var(--bg);
    border-color: var(--accent);
  }
  .btn-primary:hover:not(:disabled):not(.disabled) {
    background: var(--accent-hover);
    border-color: var(--accent-hover);
  }
  .btn-ghost {
    background: transparent;
    color: var(--ink);
  }
  .btn-ghost:hover:not(:disabled):not(.disabled) {
    background: var(--accent-bg);
  }
  .btn-danger {
    background: transparent;
    color: var(--danger);
    border-color: var(--danger);
  }
  .btn-danger:hover:not(:disabled):not(.disabled) {
    background: var(--danger-bg);
  }

  .spinner {
    width: 12px;
    height: 12px;
    border: 1.5px solid currentColor;
    border-top-color: transparent;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
  }
  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
</style>
