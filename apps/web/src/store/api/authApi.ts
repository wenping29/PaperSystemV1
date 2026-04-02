import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import type { LoginResponse, UserProfile } from '../slices/authSlice'

export interface LoginRequest {
  username: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  phone: string
  password: string
}

export const authApi = createApi({
  reducerPath: 'authApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/auth',
    prepareHeaders: (headers) => {
      const token = localStorage.getItem('token')
      if (token) {
        headers.set('Authorization', `Bearer ${token}`)
      }
      return headers
    }
  }),
  tagTypes: ['User'],
  endpoints: (builder) => ({
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (credentials) => ({
        url: '/login',
        method: 'POST',
        body: credentials
      }),
      invalidatesTags: ['User']
    }),
    logout: builder.mutation<void, void>({
      query: () => ({
        url: '/logout',
        method: 'POST'
      }),
      invalidatesTags: ['User']
    }),
    register: builder.mutation<void, RegisterRequest>({
      query: (userData) => ({
        url: '/register',
        method: 'POST',
        body: userData
      })
    }),
    getProfile: builder.query<UserProfile, void>({
      query: () => '/profile',
      providesTags: ['User']
    }),
    updateProfile: builder.mutation<UserProfile, Partial<UserProfile>>({
      query: (profile) => ({
        url: '/profile',
        method: 'PUT',
        body: profile
      }),
      invalidatesTags: ['User']
    }),
    refreshToken: builder.mutation<{ token: string; refreshToken: string }, void>({
      query: () => ({
        url: '/refresh',
        method: 'POST'
      })
    })
  })
})

export const {
  useLoginMutation,
  useLogoutMutation,
  useRegisterMutation,
  useGetProfileQuery,
  useUpdateProfileMutation,
  useRefreshTokenMutation
} = authApi