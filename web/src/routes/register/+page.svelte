<script lang="ts">
  import { goto } from '$app/navigation';
  import { authApi, ApiError } from '$lib/api';
  import { authStore } from '$lib/stores/auth';
  import Field from '$lib/components/Field.svelte';
  import Button from '$lib/components/Button.svelte';

  let email = $state('');
  let password = $state('');
  let confirm = $state('');
  let error = $state<string | null>(null);
  let loading = $state(false);

  async function submit(e: SubmitEvent) {
    e.preventDefault();
    error = null;
    if (password.length < 8) {
      error = 'Password must be at least 8 characters.';
      return;
    }
    if (password !== confirm) {
      error = 'Passwords do not match.';
      return;
    }
    loading = true;
    try {
      const token = await authApi.register(email, password);
      authStore.setAll(token.accessToken, {
        id: '',
        email: token.email,
        role: token.role
      });
      const me = await authApi.me();
      authStore.setUser(me);
      await goto('/', { replaceState: true });
    } catch (err) {
      error = err instanceof ApiError ? err.detail ?? err.message : 'Registration failed';
    } finally {
      loading = false;
    }
  }
</script>

<svelte:head><title>Create account · Backlog AI</title></svelte:head>

<div class="auth-shell">
  <div class="brand">Backlog<span>·</span>AI</div>
  <h1>Create an account.</h1>
  <p class="lede">Read suggestions, browse competitive features, search the backlog.</p>

  <form onsubmit={submit}>
    <Field label="Email" name="email" type="email" autocomplete="email" required bind:value={email} />
    <Field
      label="Password"
      name="password"
      type="password"
      autocomplete="new-password"
      required
      hint="At least 8 characters."
      bind:value={password}
    />
    <Field
      label="Confirm password"
      name="confirm"
      type="password"
      autocomplete="new-password"
      required
      bind:value={confirm}
    />
    {#if error}<p class="error">{error}</p>{/if}
    <Button type="submit" {loading}>Create account</Button>
  </form>

  <p class="alt">
    Already have one? <a href="/login">Sign in</a>.
  </p>
</div>

<style>
  .auth-shell {
    max-width: 380px;
    margin: 12vh auto 0;
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
