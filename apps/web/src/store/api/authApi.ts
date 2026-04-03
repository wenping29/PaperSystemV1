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

// 从环境变量读取（开发环境 = /api，走代理）
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL 

export const authApi = createApi({
  reducerPath: 'authApi',
  baseQuery: fetchBaseQuery({
    // 👇 这里正确拼接
    baseUrl: API_BASE_URL,
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
        url: '/auth/login',
        method: 'POST',
        body: credentials
      }),
      invalidatesTags: ['User']
    }),
    logout: builder.mutation<void, void>({
      query: () => ({
        url: '/auth/logout',
        method: 'POST'
      })
    }),
    register: builder.mutation<void, RegisterRequest>({
      query: (userData) => ({
        url: '/auth/register',
        method: 'POST',
        body: userData
      })
    }),
    // 👇 这里修复！！！把 /2/profile 改成正确路径
    getProfile: builder.query<UserProfile, number>({
      query: (id) => `/users/profile/${id}`, // ✅ 修复
      providesTags: ['User']
    }),
    updateProfile: builder.mutation<UserProfile, Partial<UserProfile>>({
      query: (profile) => ({
        url: '/auth/profile',
        method: 'PUT',
        body: profile
      }),
      invalidatesTags: ['User']
    }),
    refreshToken: builder.mutation<{ token: string; refreshToken: string }, void>({
      query: () => ({
        url: '/auth/refresh',
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