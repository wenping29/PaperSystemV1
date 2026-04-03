import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { AuthState } from '@/core/types/auth.types'
import type { User } from '@/core/types/entities'
import { STORAGE_KEYS } from '@/core/constants/storage-keys'

interface AuthStore extends AuthState {
  login: (token: string, refreshToken: string, expiresAt: string, user: User) => void
  logout: () => void
  setLoading: (loading: boolean) => void
  setError: (error: string | null) => void
  updateUser: (user: Partial<User>) => void
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set) => ({
      token: null,
      refreshToken: null,
      expiresAt: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,
      user: null,

      login: (token: string, refreshToken: string, expiresAt: string, user: User) =>
        set({
          token,
          refreshToken,
          expiresAt,
          user,
          isAuthenticated: true,
          isLoading: false,
          error: null,
        }),

      logout: () =>
        set({
          token: null,
          refreshToken: null,
          expiresAt: null,
          user: null,
          isAuthenticated: false,
          isLoading: false,
          error: null,
        }),

      setLoading: (loading: boolean) => set({ isLoading: loading }),

      setError: (error: string | null) => set({ error }),

      updateUser: (userData: Partial<User>) =>
        set((state) => ({
          user: state.user ? { ...state.user, ...userData } : null,
        })),
    }),
    {
      name: STORAGE_KEYS.TOKEN,
      partialize: (state) => ({
        token: state.token,
        refreshToken: state.refreshToken,
        expiresAt: state.expiresAt,
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
)
