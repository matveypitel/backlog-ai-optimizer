import { writable, derived } from 'svelte/store';
import { browser } from '$app/environment';
import type { User } from '$lib/types';

const STORAGE_KEY = 'bao.auth';

interface AuthState {
  token: string | null;
  user: User | null;
}

function load(): AuthState {
  if (!browser) return { token: null, user: null };
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return { token: null, user: null };
    return JSON.parse(raw) as AuthState;
  } catch {
    return { token: null, user: null };
  }
}

function persist(state: AuthState) {
  if (!browser) return;
  if (state.token) localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
  else localStorage.removeItem(STORAGE_KEY);
}

function createAuthStore() {
  const { subscribe, update, set } = writable<AuthState>(load());

  return {
    subscribe,
    setToken(token: string) {
      update((s) => {
        const next = { ...s, token };
        persist(next);
        return next;
      });
    },
    setUser(user: User) {
      update((s) => {
        const next = { ...s, user };
        persist(next);
        return next;
      });
    },
    setAll(token: string, user: User) {
      const next = { token, user };
      persist(next);
      set(next);
    },
    clear() {
      persist({ token: null, user: null });
      set({ token: null, user: null });
    }
  };
}

export const authStore = createAuthStore();
export const isAuthenticated = derived(authStore, ($a) => Boolean($a.token));
export const isAdmin = derived(authStore, ($a) => $a.user?.role === 'Admin');
export const currentUser = derived(authStore, ($a) => $a.user);
