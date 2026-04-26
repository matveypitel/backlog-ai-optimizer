<script lang="ts">
  import { toast } from '$lib/stores/toast';
</script>

<div class="toasts" aria-live="polite">
  {#each $toast as t (t.id)}
    <div class="toast toast-{t.kind}" role="status">
      <span>{t.message}</span>
      <button onclick={() => toast.dismiss(t.id)} aria-label="Dismiss">×</button>
    </div>
  {/each}
</div>

<style>
  .toasts {
    position: fixed;
    bottom: var(--space-5);
    right: var(--space-5);
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
    z-index: 100;
    max-width: 420px;
  }
  .toast {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-4);
    background: var(--bg-elev);
    border: var(--hairline);
    border-left: 3px solid;
    border-radius: var(--radius);
    font-size: 0.9rem;
  }
  .toast-success { border-left-color: var(--success); }
  .toast-error { border-left-color: var(--danger); }
  .toast-info { border-left-color: var(--accent); }
  button {
    background: none;
    border: none;
    color: var(--ink-faint);
    font-size: 1.2rem;
    line-height: 1;
    cursor: pointer;
    padding: 0;
  }
  button:hover { color: var(--ink); }
</style>
