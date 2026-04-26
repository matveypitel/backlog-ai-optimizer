<script lang="ts">
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  import { authApi, ApiError } from '$lib/api';
  import { authStore } from '$lib/stores/auth';
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
      error = err instanceof ApiError ? err.detail ?? err.message : 'Login failed';
    } finally {
      loading = false;
    }
  }
</script>

<svelte:head><title>Sign in · Backlog AI</title></svelte:head>

<div class="auth-shell">
  <div class="brand">Backlog<span>·</span>AI</div>
  <h1>Welcome back.</h1>
  <p class="lede">Sign in to review competitive intelligence.</p>

  <form onsubmit={submit}>
    <Field label="Email" name="email" type="email" autocomplete="email" required bind:value={email} />
    <Field
      label="Password"
      name="password"
      type="password"
      autocomplete="current-password"
      required
      bind:value={password}
    />
    {#if error}<p class="error">{error}</p>{/if}
    <Button type="submit" {loading}>Sign in</Button>
  </form>

  <p class="alt">
    No account yet? <a href="/register">Create one</a>.
  </p>
</div>

<style>
  .auth-shell {
    max-width: 380px;
    margin: 14vh auto 0;
    padding: 0 var(--space-5);
  }
  .brand {
    font-family: var(--font-serif);
    font-size: 1.05rem;
    color: var(--ink-soft);
    margin-bottom: var(--space-7);
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
