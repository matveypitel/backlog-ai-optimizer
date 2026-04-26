<script lang="ts">
  import { onMount } from 'svelte';
  import { authApi, ApiError } from '$lib/api';
  import type { UserSummary } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Field from '$lib/components/Field.svelte';
  import Button from '$lib/components/Button.svelte';
  import { toast } from '$lib/stores/toast';
  import { formatDate } from '$lib/utils/format';

  let users = $state<UserSummary[]>([]);
  let email = $state('');
  let password = $state('');
  let role = $state<'User' | 'Admin'>('User');
  let creating = $state(false);

  async function load() {
    try {
      users = await authApi.listUsers();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    }
  }

  async function create(e: SubmitEvent) {
    e.preventDefault();
    creating = true;
    try {
      await authApi.createUser(email, password, role);
      toast.success('User created.');
      email = '';
      password = '';
      role = 'User';
      await load();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : 'Failed');
    } finally {
      creating = false;
    }
  }

  onMount(load);
</script>

<svelte:head><title>Users · Admin</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">Admin / Users</span>
  <h1>Members.</h1>
</header>

<div class="grid">
  <Card title="Create user">
    <form onsubmit={create}>
      <Field label="Email" name="email" type="email" required bind:value={email} />
      <Field label="Password" name="password" type="password" required hint="Min 8 chars." bind:value={password} />
      <div class="role">
        <label>Role</label>
        <div class="radios">
          <label><input type="radio" name="role" value="User" bind:group={role} /> User</label>
          <label><input type="radio" name="role" value="Admin" bind:group={role} /> Admin</label>
        </div>
      </div>
      <Button type="submit" loading={creating}>Create</Button>
    </form>
  </Card>

  <Card title="Existing users" eyebrow={String(users.length)}>
    {#if users.length === 0}
      <p class="muted">None yet.</p>
    {:else}
      <table>
        <thead>
          <tr>
            <th>Email</th>
            <th>Role</th>
            <th>Created</th>
          </tr>
        </thead>
        <tbody>
          {#each users as u}
            <tr>
              <td class="mono">{u.email}</td>
              <td>{u.role}</td>
              <td class="muted">{formatDate(u.createdAt)}</td>
            </tr>
          {/each}
        </tbody>
      </table>
    {/if}
  </Card>
</div>

<style>
  .page-head { margin-bottom: var(--space-6); }
  .eyebrow {
    font-family: var(--font-mono);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    display: block;
    margin-bottom: var(--space-2);
  }
  .grid {
    display: grid;
    grid-template-columns: 1fr 1.5fr;
    gap: var(--space-5);
  }
  @media (max-width: 720px) {
    .grid { grid-template-columns: 1fr; }
  }
  .role { margin-bottom: var(--space-4); }
  .radios {
    display: flex;
    gap: var(--space-4);
  }
  .radios label {
    margin: 0;
    display: flex;
    align-items: center;
    gap: var(--space-2);
    color: var(--ink);
    font-weight: normal;
    font-size: 0.95rem;
    cursor: pointer;
  }
  .radios input { width: auto; }
  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.9rem;
  }
  th, td {
    text-align: left;
    padding: var(--space-2) var(--space-3);
    border-bottom: 1px solid var(--rule-soft);
  }
  th {
    font-family: var(--font-mono);
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-faint);
    font-weight: 500;
    border-bottom: var(--hairline);
  }
  .muted { color: var(--ink-soft); font-size: 0.85rem; }
</style>
