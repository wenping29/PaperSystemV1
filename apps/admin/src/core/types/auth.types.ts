import type { User } from './entities'

export interface LoginRequest {
  username: string
  password: string
}

export interface AuthResponse {
  token: string
  refreshToken: string
  expiresAt: string
  user: User
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface AuthState {
  token: string | null
  refreshToken: string | null
  expiresAt: string | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
  user: User | null
}

export interface CreateUserRequest {
  username: string
  email: string
  password: string
  phone?: string
}

export interface UpdateUserRequest {
  username?: string
  email?: string
  phone?: string
  bio?: string
  avatarUrl?: string
  location?: string
  website?: string
}

export interface UpdateUserRoleRequest {
  role: string
  reason?: string
}
