import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthResponse, UserDto } from '../types/api';
import { api } from '../lib/api';

interface AuthState {
  user: UserDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, firstName: string, lastName: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: () => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,

      login: async (email, password) => {
        const res = await api.post<AuthResponse>('/api/auth/login', { email, password });
        if (!res) throw new Error('Login failed');
        set({
          user: res.user,
          accessToken: res.accessToken,
          refreshToken: res.refreshToken,
        });
      },

      register: async (email, password, firstName, lastName) => {
        const res = await api.post<AuthResponse>('/api/auth/register', {
          email, password, firstName, lastName,
        });
        if (!res) throw new Error('Register failed');
        set({
          user: res.user,
          accessToken: res.accessToken,
          refreshToken: res.refreshToken,
        });
      },

      logout: () => {
        const rt = get().refreshToken;
        if (rt) {
          // fire & forget
          api.post('/api/auth/logout', { refreshToken: rt }).catch(() => {});
        }
        set({ user: null, accessToken: null, refreshToken: null });
      },

      isAuthenticated: () => !!get().accessToken,
    }),
    {
      name: 'trading-app-auth',
    }
  )
);