import { UserProfile } from '@/store/slices/authSlice'
import axiosInstance from '../client'

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  user: UserProfile
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
}

export interface RegisterResponse {
  message: string
  userId: number
}

export const authApi = {
  // 登录
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await axiosInstance.post<ApiResponse<LoginResponse>>('/auth/login', credentials)
    return response.data
  },

  // 注册
  register: async (userData: RegisterRequest): Promise<RegisterResponse> => {
    const response = await axiosInstance.post<ApiResponse<RegisterResponse>>('/auth/register', userData)
    return response.data
  },

  // 登出
  logout: async (): Promise<void> => {
    await axiosInstance.post('/auth/logout')
  },

  // 获取用户资料
  getProfile: async (id: number): Promise<UserProfile> => {
    const response = await axiosInstance.get<ApiResponse<UserProfile>>(`/auth/profile/${id}`)
    return response.data
  },

  // 刷新token
  refreshToken: async (): Promise<LoginResponse> => {
    const response = await axiosInstance.post<ApiResponse<LoginResponse>>('/auth/refresh')
    return response.data
  },
}

// 统一的API响应类型
export interface ApiResponse<T = any> {
  code: number
  message: string
  data: T
  timestamp: number
}

// 分页参数类型
export interface PaginationParams {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

// 分页响应类型
export interface PaginatedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}