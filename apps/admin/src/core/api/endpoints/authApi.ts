import axiosInstance from '../client'
import type { ApiResponse } from '@/core/types/api.types'
import type { AuthResponse, LoginRequest, RefreshTokenRequest } from '@/core/types/auth.types'

export const authApi = {
  // 登录
  login: async (credentials: LoginRequest): Promise<ApiResponse> => {
    const response = await axiosInstance.post<ApiResponse<AuthResponse>>('/auth/login', credentials)
    return response
  },

  // 注册
  register: async (userData: any): Promise<ApiResponse<AuthResponse>> => {
    const response = await axiosInstance.post<ApiResponse<AuthResponse>>('/auth/register', userData)
    return response.data
  },

  // 刷新token
  refreshToken: async (request: RefreshTokenRequest): Promise<ApiResponse<AuthResponse>> => {
    const response = await axiosInstance.post<ApiResponse<AuthResponse>>('/auth/refresh', request)
    return response.data
  },

  // 获取当前用户
  getCurrentUser: async (): Promise<ApiResponse<any>> => {
    const response = await axiosInstance.get<ApiResponse<any>>('/auth/me')
    return response.data
  },

  // 登出
  logout: async (): Promise<ApiResponse<void>> => {
    const response = await axiosInstance.post<ApiResponse<void>>('/auth/logout')
    return response.data
  },
}
