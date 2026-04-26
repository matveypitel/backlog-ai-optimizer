<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { isAdmin, currentUser } from '$lib/stores/auth';

  let { children } = $props();
  let ready = $state(false);

  onMount(() => {
    if ($currentUser && !$isAdmin) {
      goto('/', { replaceState: true });
      return;
    }
    ready = true;
  });
</script>

{#if ready}
  {@render children()}
{/if}
