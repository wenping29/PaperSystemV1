import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from '..'

export interface AuthState {
  token: string | null
  refreshToken: string | null
  expiresAt: number | null
  user: UserProfile | null
  isAuthenticated: boolean
  isLoading: boolean
}

export interface UserProfile {
  id: string
  username: string
  email: string
  phone: string
  avatar: string
  role: string
  createdAt: string
  updatedAt: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  expiresAt: number
  user: UserProfile
}

const initialState: AuthState = {
  token: localStorage.getItem('token') || null,
  refreshToken: localStorage.getItem('refreshToken') || null,
  expiresAt: localStorage.getItem('expiresAt') ? Number(localStorage.getItem('expiresAt')) : null,
  user: null,
  isAuthenticated: !!localStorage.getItem('token'),
  isLoading: false
}

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials: (state, action: PayloadAction<LoginResponse>) => {
      const { token, refreshToken, expiresAt, user } = action.payload
      state.token = token
      state.refreshToken = refreshToken
      state.expiresAt = expiresAt
      state.user = user
      state.isAuthenticated = true

      // 保存到本地存储
      localStorage.setItem('token', token)
      localStorage.setItem('refreshToken', refreshToken)
      localStorage.setItem('expiresAt', expiresAt.toString())
    },
    clearCredentials: (state) => {
      state.token = null
      state.refreshToken = null
      state.expiresAt = null
      state.user = null
      state.isAuthenticated = false

      // 清除本地存储
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('expiresAt')
    },
    setUser: (state, action: PayloadAction<UserProfile>) => {
      state.user = action.payload
    },
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload
    }
  }
})

export const { setCredentials, clearCredentials, setUser, setLoading } = authSlice.actions

export const selectIsAuthenticated = (state: RootState) => state.auth.isAuthenticated
export const selectCurrentUser = (state: RootState) => state.auth.user
export const selectAuthLoading = (state: RootState) => state.auth.isLoading

export default authSlice.reducer