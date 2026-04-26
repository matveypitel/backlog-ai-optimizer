import { writable } from 'svelte/store';

export type ToastKind = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

let counter = 0;

function createToastStore() {
  const { subscribe, update } = writable<Toast[]>([]);

  function push(kind: ToastKind, message: string, ttl = 4000) {
    const id = ++counter;
    update((list) => [...list, { id, kind, message }]);
    setTimeout(() => {
      update((list) => list.filter((t) => t.id !== id));
    }, ttl);
  }

  return {
    subscribe,
    success: (msg: string) => push('success', msg),
    error: (msg: string) => push('error', msg, 6000),
    info: (msg: string) => push('info', msg),
    dismiss(id: number) {
      update((list) => list.filter((t) => t.id !== id));
    }
  };
}

export const toast = createToastStore();
