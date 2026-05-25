<script lang="ts">
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import { authStore, currentUser, isAdmin } from '$lib/stores/auth';
  import { T } from '$lib/i18n';
  import LanguageSwitcher from './LanguageSwitcher.svelte';

  function logout() {
    authStore.clear();
    goto('/login');
  }

  $: userLinks = [
    { href: '/', label: $T.nav.dashboard },
    { href: '/suggestions', label: $T.nav.suggestions },
    { href: '/reprioritization', label: $T.nav.reprioritization },
    { href: '/competitors', label: $T.nav.competitors },
    { href: '/scraping', label: $T.nav.scraping },
    { href: '/jira', label: $T.nav.jira }
  ];

  $: adminLinks = [
    { href: '/admin/sync', label: $T.nav.sync },
    { href: '/admin/embeddings', label: $T.nav.embeddings },
    { href: '/admin/prompts', label: $T.nav.prompts },
    { href: '/admin/users', label: $T.nav.users }
  ];

  function isActive(href: string): boolean {
    const path = $page.url.pathname;
    if (href === '/') return path === '/';
    return path === href || path.startsWith(href + '/');
  }
</script>

<header class="app-header">
  <div class="brand">
    <a href="/">Backlog<span>·</span>AI</a>
  </div>

  <nav class="nav-main">
    {#each userLinks as link}
      <a href={link.href} class:active={isActive(link.href)}>{link.label}</a>
    {/each}
  </nav>

  {#if $isAdmin}
    <nav class="nav-admin">
      <span class="nav-label">{$T.nav.admin}</span>
      {#each adminLinks as link}
        <a href={link.href} class:active={isActive(link.href)}>{link.label}</a>
      {/each}
    </nav>
  {/if}

  <div class="right-cluster">
    <LanguageSwitcher />

    {#if $currentUser}
      <span class="email">{$currentUser.email}</span>
      <button onclick={logout}>{$T.nav.signOut}</button>
    {/if}
  </div>
</header>

<style>
  .app-header {
    display: flex;
    align-items: center;
    gap: var(--space-6);
    padding: var(--space-4) var(--space-6);
    border-bottom: var(--hairline);
    background: var(--bg-elev);
    flex-wrap: wrap;
  }
  .brand a {
    font-family: var(--font-serif);
    font-size: 1.15rem;
    font-weight: 500;
    color: var(--ink);
    border: 0;
  }
  .brand span {
    color: var(--accent);
    margin: 0 0.2em;
  }
  .nav-main,
  .nav-admin {
    display: flex;
    gap: var(--space-4);
    align-items: center;
    font-size: 0.9rem;
  }
  .nav-admin {
    margin-left: auto;
    padding-left: var(--space-4);
    border-left: var(--hairline);
  }
  .nav-label {
    font-family: var(--font-mono);
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
  }
  nav a {
    color: var(--ink-soft);
    border: 0;
    padding-bottom: 2px;
    border-bottom: 1px solid transparent;
  }
  nav a:hover {
    color: var(--ink);
  }
  nav a.active {
    color: var(--ink);
    border-bottom-color: var(--accent);
  }
  .right-cluster {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    margin-left: auto;
    font-size: 0.85rem;
  }
  :global(.nav-admin) ~ .right-cluster {
    margin-left: var(--space-4);
  }
  .email {
    color: var(--ink-soft);
    font-family: var(--font-mono);
    font-size: 0.8rem;
  }
  .right-cluster button {
    background: transparent;
    border: var(--hairline);
    color: var(--ink-soft);
    padding: 4px 10px;
    border-radius: var(--radius);
    cursor: pointer;
    font-size: 0.8rem;
  }
  .right-cluster button:hover {
    background: var(--bg-sunk);
    color: var(--ink);
  }
</style>
