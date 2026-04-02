import axiosInstance from '../client'
import type { ApiResponse, PaginationParams, PaginatedResponse } from '@/core/types/api.types'
import type { User } from '@/core/types/entities'
import type { CreateUserRequest, UpdateUserRequest, UpdateUserRoleRequest } from '@/core/types/auth.types'

export interface GetUsersParams extends PaginationParams {
  search?: string
  role?: string
  status?: string
}

export const userApi = {
  // 获取用户列表
  getUsers: async (params?: GetUsersParams): Promise<ApiResponse<User[]>> => {
    const response = await axiosInstance.get<ApiResponse<User[]>>('/users', { params })
    return response.data
  },

  // 获取用户总数
  getUsersCount: async (search?: string): Promise<ApiResponse<{ count: number }>> => {
    const response = await axiosInstance.get<ApiResponse<{ count: number }>>('/users/count', { params: { search } })
    return response.data
  },

  // 获取用户详情
  getUserById: async (id: number): Promise<ApiResponse<User>> => {
    const response = await axiosInstance.get<ApiResponse<User>>(`/users/${id}`)
    return response.data
  },

  // 创建用户
  createUser: async (data: CreateUserRequest): Promise<ApiResponse<User>> => {
    const response = await axiosInstance.post<ApiResponse<User>>('/users', data)
    return response.data
  },

  // 更新用户
  updateUser: async (id: number, data: UpdateUserRequest): Promise<ApiResponse<User>> => {
    const response = await axiosInstance.put<ApiResponse<User>>(`/users/${id}`, data)
    return response.data
  },

  // 删除用户
  deleteUser: async (id: number): Promise<ApiResponse<void>> => {
    const response = await axiosInstance.delete<ApiResponse<void>>(`/users/${id}`)
    return response.data
  },

  // 更新用户角色
  updateUserRole: async (id: number, data: UpdateUserRoleRequest): Promise<ApiResponse<{ message: string }>> => {
    const response = await axiosInstance.put<ApiResponse<{ message: string }>>(`/users/${id}/role`, data)
    return response.data
  },

  // 获取用户统计信息
  getUserStats: async (id: number): Promise<ApiResponse<any>> => {
    const response = await axiosInstance.get<ApiResponse<any>>(`/users/${id}/stats`)
    return response.data
  },
}
