<script lang="ts">
  import '../app.css';
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import { authStore, isAuthenticated } from '$lib/stores/auth';
  import { authApi, ApiError } from '$lib/api';
  import AppHeader from '$lib/components/AppHeader.svelte';
  import Toasts from '$lib/components/Toasts.svelte';

  let { children } = $props();

  let booted = $state(false);

  const PUBLIC_ROUTES = ['/login', '/register'];

  onMount(async () => {
    if ($isAuthenticated) {
      try {
        const me = await authApi.me();
        authStore.setUser(me);
      } catch (err) {
        if (err instanceof ApiError && err.status === 401) {
          authStore.clear();
        }
      }
    }
    booted = true;

    const path = $page.url.pathname;
    const isPublic = PUBLIC_ROUTES.some((r) => path === r || path.startsWith(r + '/'));
    if (!$isAuthenticated && !isPublic) {
      const next = encodeURIComponent(path + $page.url.search);
      goto(`/login?next=${next}`, { replaceState: true });
    }
  });

  let showShell = $derived(
    booted && $isAuthenticated && !PUBLIC_ROUTES.some((r) => $page.url.pathname.startsWith(r))
  );
  let isPublicRoute = $derived(
    PUBLIC_ROUTES.some(
      (r) => $page.url.pathname === r || $page.url.pathname.startsWith(r + '/')
    )
  );
</script>

{#if !booted}
  <div class="boot"></div>
{:else if isPublicRoute}
  {@render children()}
{:else if showShell}
  <AppHeader />
  <main class="content">
    {@render children()}
  </main>
{/if}

<Toasts />

<style>
  .boot {
    height: 100vh;
  }
  .content {
    max-width: var(--content-width);
    margin: 0 auto;
    padding: var(--space-6) var(--space-6) var(--space-8);
  }
  @media (max-width: 720px) {
    .content {
      padding: var(--space-4);
    }
  }
</style>
