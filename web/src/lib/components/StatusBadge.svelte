<script lang="ts">
  interface Props {
    status: string;
  }

  let { status }: Props = $props();

  const map: Record<string, string> = {
    Pending: 'pending',
    Running: 'running',
    Completed: 'success',
    Failed: 'danger',
    Highest: 'danger',
    High: 'warning',
    Medium: 'info',
    Low: 'muted',
    Lowest: 'muted'
  };

  let kind = $derived(map[status] ?? 'muted');
</script>

<span class="badge badge-{kind}">{status}</span>

<style>
  .badge {
    display: inline-block;
    font-family: var(--font-mono);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    padding: 2px 8px 3px;
    border-bottom: 2px solid;
    color: var(--ink);
  }
  .badge-pending { border-color: var(--ink-faint); color: var(--ink-soft); }
  .badge-running { border-color: var(--warning); color: var(--warning); }
  .badge-success { border-color: var(--success); color: var(--success); }
  .badge-danger { border-color: var(--danger); color: var(--danger); }
  .badge-warning { border-color: var(--warning); color: var(--warning); }
  .badge-info { border-color: var(--accent); color: var(--accent); }
  .badge-muted { border-color: var(--ink-faint); color: var(--ink-soft); }
</style>
