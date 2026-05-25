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
</div>

<style>
  .auth-shell {
    max-width: 380px;
    margin: 14vh auto 0;
    padding: 0 var(--space-5);
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
</style>
