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

  let menuOpen = $state(false);

  let userLinks = [
    { href: '/', label: $T.nav.dashboard },
    { href: '/suggestions', label: $T.nav.suggestions },
    { href: '/reprioritization', label: $T.nav.reprioritization },
    { href: '/competitors', label: $T.nav.competitors },
    { href: '/scraping', label: $T.nav.scraping },
    { href: '/jira', label: $T.nav.jira }
  ];

 let adminLinks = [
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
  <!-- ── Desktop row (hidden on mobile) ── -->
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

  <!-- ── Mobile top bar ── -->
  <div class="mobile-bar">
    <div class="brand-mobile">
      <a href="/">Backlog<span>·</span>AI</a>
    </div>
    <div class="mobile-bar-right">
      <LanguageSwitcher />
      <button class="hamburger" onclick={() => (menuOpen = !menuOpen)} aria-label="Toggle navigation" aria-expanded={menuOpen}>
        {#if menuOpen}
          <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
            <line x1="2" y1="2" x2="16" y2="16" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
            <line x1="16" y1="2" x2="2" y2="16" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
          </svg>
        {:else}
          <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
            <line x1="2" y1="5" x2="16" y2="5" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
            <line x1="2" y1="9" x2="16" y2="9" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
            <line x1="2" y1="13" x2="16" y2="13" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
          </svg>
        {/if}
      </button>
    </div>
  </div>

  <!-- ── Mobile drawer ── -->
  {#if menuOpen}
    <div class="mobile-drawer">
      <nav class="drawer-section">
        {#each userLinks as link}
          <a href={link.href} class:active={isActive(link.href)} onclick={() => (menuOpen = false)}>{link.label}</a>
        {/each}
      </nav>

      {#if $isAdmin}
        <div class="drawer-divider">
          <span class="nav-label">{$T.nav.admin}</span>
        </div>
        <nav class="drawer-section">
          {#each adminLinks as link}
            <a href={link.href} class:active={isActive(link.href)} onclick={() => (menuOpen = false)}>{link.label}</a>
          {/each}
        </nav>
      {/if}

      {#if $currentUser}
        <div class="drawer-divider drawer-user">
          <span class="email">{$currentUser.email}</span>
          <button onclick={() => { menuOpen = false; logout(); }}>{$T.nav.signOut}</button>
        </div>
      {/if}
    </div>
  {/if}
</header>

<style>
  /* ── Desktop layout ── */
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

  /* ── Mobile-only elements (hidden on desktop) ── */
  .mobile-bar,
  .mobile-drawer {
    display: none;
  }

  /* ── Responsive breakpoint ── */
  @media (max-width: 768px) {
    /* Hide desktop nav; switch header to column */
    .app-header {
      display: block;
      padding: 0;
    }

    .brand,
    .nav-main,
    .nav-admin,
    .right-cluster {
      display: none;
    }

    /* Mobile top bar */
    .mobile-bar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-3) var(--space-4);
    }

    .mobile-bar-right {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }

    .brand-mobile a {
      font-family: var(--font-serif);
      font-size: 1.1rem;
      font-weight: 500;
      color: var(--ink);
      border: 0;
    }
    .brand-mobile span {
      color: var(--accent);
      margin: 0 0.2em;
    }

    .hamburger {
      background: transparent;
      border: var(--hairline);
      color: var(--ink-soft);
      padding: 6px 8px;
      border-radius: var(--radius);
      cursor: pointer;
      line-height: 0;
      display: flex;
      align-items: center;
    }
    .hamburger:hover {
      background: var(--bg-sunk);
      color: var(--ink);
    }

    /* Drawer */
    .mobile-drawer {
      display: block;
      border-top: var(--hairline);
    }

    .drawer-section {
      display: flex;
      flex-direction: column;
      padding: var(--space-2) 0;
    }
    .drawer-section a {
      display: block;
      padding: 10px var(--space-4);
      color: var(--ink-soft);
      border: 0;
      font-size: 0.95rem;
      border-left: 2px solid transparent;
      transition: background 100ms ease, color 100ms ease;
    }
    .drawer-section a:hover {
      color: var(--ink);
      background: var(--bg-sunk);
    }
    .drawer-section a.active {
      color: var(--ink);
      border-left-color: var(--accent);
      background: var(--accent-bg);
    }

    .drawer-divider {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-2) var(--space-4);
      border-top: var(--hairline);
      margin-top: var(--space-1);
    }
    .drawer-user {
      justify-content: space-between;
      padding-bottom: var(--space-3);
    }
    .drawer-user .email {
      color: var(--ink-soft);
      font-family: var(--font-mono);
      font-size: 0.8rem;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .drawer-user button {
      background: transparent;
      border: var(--hairline);
      color: var(--ink-soft);
      padding: 4px 10px;
      border-radius: var(--radius);
      cursor: pointer;
      font-size: 0.8rem;
      white-space: nowrap;
      flex-shrink: 0;
    }
    .drawer-user button:hover {
      background: var(--bg-sunk);
      color: var(--ink);
    }
  }
</style>
