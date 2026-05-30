<script lang="ts">
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  import { authApi, ApiError } from '$lib/api';
  import { authStore } from '$lib/stores/auth';
  import { T } from '$lib/i18n';
  import LanguageSwitcher from '$lib/components/LanguageSwitcher.svelte';
  import Field from '$lib/components/Field.svelte';
  import Button from '$lib/components/Button.svelte';

  let email = $state('');
  let password = $state('');
  let error = $state<string | null>(null);
  let loading = $state(false);

  async function submit(e: SubmitEvent) {
    e.preventDefault();
    error = null;
    loading = true;
    try {
      const token = await authApi.login(email, password);
      authStore.setAll(token.accessToken, {
        id: '',
        email: token.email,
        role: token.role
      });
      const me = await authApi.me();
      authStore.setUser(me);
      const next = $page.url.searchParams.get('next') || '/';
      await goto(next, { replaceState: true });
    } catch (err) {
      error = err instanceof ApiError ? err.detail ?? err.message : $T.auth.loginFailed;
    } finally {
      loading = false;
    }
  }
</script>

<svelte:head><title>{$T.auth.signIn} · Backlog AI</title></svelte:head>

<div class="auth-shell">
  <div class="auth-top">
    <div class="brand">Backlog<span>·</span>AI</div>
    <LanguageSwitcher />
  </div>
  <h1>{$T.auth.welcomeBack}</h1>
  <p class="lede">{$T.auth.signInLede}</p>

  <form onsubmit={submit}>
    <Field label={$T.auth.email} name="email" type="email" autocomplete="email" required bind:value={email} />
    <Field
      label={$T.auth.password}
      name="password"
      type="password"
      autocomplete="current-password"
      required
      bind:value={password}
    />
    {#if error}<p class="error">{error}</p>{/if}
    <Button type="submit" {loading}>{$T.auth.signIn}</Button>
  </form>

  <p class="alt">
    {$T.auth.noAccount} <a href="/register">{$T.auth.createOne}</a>.
  </p>

  <div class="demo-box">
    <p class="demo-title">Demo accounts</p>
    <div class="demo-accounts">
      <button class="demo-account" onclick={() => { email = 'admin@example.com'; password = 'Admin123!'; }}>
        <span class="demo-badge demo-badge--admin">Admin</span>
        <span class="demo-email">admin@example.com</span>
        <span class="demo-pass">Admin123!</span>
      </button>
      <button class="demo-account" onclick={() => { email = 'user@example.com'; password = 'User123!'; }}>
        <span class="demo-badge demo-badge--user">User</span>
        <span class="demo-email">user@example.com</span>
        <span class="demo-pass">User123!</span>
      </button>
    </div>
  </div>
</div>

<style>
  .auth-shell {
    max-width: 380px;
    margin: 14vh auto 0;
    padding: 0 var(--space-5);
  }
  @media (max-width: 600px) {
    .auth-shell {
      margin-top: var(--space-7);
      padding: 0 var(--space-4);
    }
  }
  .auth-top {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--space-7);
  }
  .brand {
    font-family: var(--font-serif);
    font-size: 1.05rem;
    color: var(--ink-soft);
    letter-spacing: -0.01em;
  }
  .brand span {
    color: var(--accent);
  }
  h1 {
    font-size: 2.2rem;
    margin-bottom: var(--space-2);
  }
  .lede {
    color: var(--ink-soft);
    margin-bottom: var(--space-6);
  }
  form {
    margin-bottom: var(--space-5);
  }
  .error {
    color: var(--danger);
    font-size: 0.9rem;
    margin-bottom: var(--space-3);
  }
  .alt {
    color: var(--ink-soft);
    font-size: 0.9rem;
    margin-top: var(--space-5);
    border-top: var(--hairline);
    padding-top: var(--space-4);
  }
  .demo-box {
    margin-top: var(--space-5);
    padding: var(--space-4);
    border: var(--hairline);
    border-radius: var(--radius-md, 8px);
    background: var(--surface-raised, var(--surface));
  }
  .demo-title {
    font-size: 0.75rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: var(--ink-soft);
    margin-bottom: var(--space-3);
  }
  .demo-accounts {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }
  .demo-account {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    width: 100%;
    padding: var(--space-3) var(--space-3);
    background: var(--surface);
    border: var(--hairline);
    border-radius: var(--radius-sm, 6px);
    cursor: pointer;
    text-align: left;
    font-size: 0.85rem;
    color: var(--ink);
    transition: background 0.15s;
  }
  .demo-account:hover {
    background: var(--surface-hover, var(--surface-raised, #f5f5f5));
  }
  .demo-badge {
    flex-shrink: 0;
    font-size: 0.7rem;
    font-weight: 600;
    padding: 2px 7px;
    border-radius: 99px;
    text-transform: uppercase;
    letter-spacing: 0.04em;
  }
  .demo-badge--admin {
    background: #fde8d8;
    color: #b84500;
  }
  .demo-badge--user {
    background: #dbeafe;
    color: #1d4ed8;
  }
  .demo-email {
    flex: 1;
    color: var(--ink);
  }
  .demo-pass {
    font-family: var(--font-mono, monospace);
    font-size: 0.8rem;
    color: var(--ink-soft);
  }
</style>
