<script lang="ts">
  interface Props {
    label: string;
    name: string;
    type?: string;
    value?: string;
    placeholder?: string;
    required?: boolean;
    autocomplete?: string;
    error?: string;
    hint?: string;
  }

  let {
    label,
    name,
    type = 'text',
    value = $bindable(''),
    placeholder = '',
    required = false,
    autocomplete,
    error,
    hint
  }: Props = $props();
</script>

<div class="field" class:has-error={error}>
  <label for={name}>{label}</label>
  <input
    id={name}
    {name}
    {type}
    {placeholder}
    {required}
    {autocomplete}
    bind:value
  />
  {#if error}
    <span class="error">{error}</span>
  {:else if hint}
    <span class="hint">{hint}</span>
  {/if}
</div>

<style>
  .field {
    margin-bottom: var(--space-4);
  }
  .field.has-error input {
    border-color: var(--danger);
  }
  .error {
    display: block;
    margin-top: var(--space-2);
    color: var(--danger);
    font-size: 0.85rem;
  }
  .hint {
    display: block;
    margin-top: var(--space-2);
    color: var(--ink-faint);
    font-size: 0.85rem;
  }
</style>
