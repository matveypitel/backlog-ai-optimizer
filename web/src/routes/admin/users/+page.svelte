<script lang="ts">
  import { onMount } from 'svelte';
  import { authApi, ApiError } from '$lib/api';
  import type { UserSummary } from '$lib/types';
  import Card from '$lib/components/Card.svelte';
  import Field from '$lib/components/Field.svelte';
  import Button from '$lib/components/Button.svelte';
  import { toast } from '$lib/stores/toast';
  import { T } from '$lib/i18n';
  import { formatDate } from '$lib/utils/format';

  const roles = {
    User: 0,
    Admin: 1
  } as const;

  let users = $state<UserSummary[]>([]);
  let email = $state('');
  let password = $state('');
  let role = $state<'User' | 'Admin'>('User');
  let creating = $state(false);

  async function load() {
    try {
      users = await authApi.listUsers();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
    }
  }

  async function create(e: SubmitEvent) {
    e.preventDefault();
    creating = true;
    try {
      await authApi.createUser(email, password, roles[role]);
      toast.success($T.admin.users.userCreated);
      email = '';
      password = '';
      role = 'User';
      await load();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.detail ?? err.message : $T.common.failed);
    } finally {
      creating = false;
    }
  }

  onMount(load);
</script>

<svelte:head><title>{$T.admin.users.title}</title></svelte:head>

<header class="page-head">
  <span class="eyebrow">{$T.admin.users.eyebrow}</span>
  <h1>{$T.admin.users.heading}</h1>
</header>

<div class="grid">
  <Card title={$T.admin.users.createUser}>
    <form onsubmit={create}>
      <Field label={$T.auth.email} name="email" type="email" required bind:value={email} />
      <Field label={$T.auth.password} name="password" type="password" required hint={$T.admin.users.passwordHint} bind:value={password} />
      <div class="role">
        <span class="role-label">{$T.admin.users.roleLabel}</span>
        <div class="radios">
          <label><input type="radio" name="role" value="User" bind:group={role} /> {$T.admin.users.roleUser}</label>
          <label><input type="radio" name="role" value="Admin" bind:group={role} /> {$T.admin.users.roleAdmin}</label>
        </div>
      </div>
      <Button type="submit" loading={creating}>{$T.common.create}</Button>
    </form>
  </Card>

  <Card title={$T.admin.users.existingUsers} eyebrow={String(users.length)}>
    {#if users.length === 0}
      <p class="muted">{$T.admin.users.noneYet}</p>
    {:else}
      <table>
        <thead>
          <tr>
            <th>{$T.admin.users.colEmail}</th>
            <th>{$T.admin.users.colRole}</th>
            <th>{$T.admin.users.colCreated}</th>
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
  .role-label {
    display: block;
    font-size: 0.9rem;
    font-weight: 500;
    color: var(--ink-soft);
    margin-bottom: var(--space-2);
  }
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
