import { api } from './client';
import type { AuthToken, User, UserSummary } from '$lib/types';

export const authApi = {
  login: (email: string, password: string) =>
    api<AuthToken>('/api/auth/login', { method: 'POST', body: { email, password } }),

  register: (email: string, password: string) =>
    api<AuthToken>('/api/auth/register', { method: 'POST', body: { email, password } }),

  me: () => api<User>('/api/auth/me'),

  listUsers: () => api<UserSummary[]>('/api/auth/users'),

  createUser: (email: string, password: string, role: number) =>
    api('/api/auth/users', { method: 'POST', body: { email, password, role } })
};
