import axiosInstance from '../client'
import type { ApiResponse, PaginationParams } from '@/core/types/api.types'
import type { Work, WorkStatus, WorkVisibility } from '@/core/types/entities'

export interface GetWorksParams extends PaginationParams {
  search?: string
  status?: WorkStatus
  visibility?: WorkVisibility
  category?: string
  authorId?: number
}

export interface CreateWorkRequest {
  title: string
  content?: string
  category?: string
  tags?: string[]
  authorId?: number
}

export interface UpdateWorkRequest {
  title?: string
  content?: string
  category?: string
  tags?: string[]
  isPublished?: boolean
  status?: WorkStatus
  visibility?: WorkVisibility
}

export const workApi = {
  // 获取作品列表
  getWorks: async (params?: GetWorksParams): Promise<ApiResponse<any>> => {
    const response = await axiosInstance.get<ApiResponse<any>>('/works', { params })
    return response.data
  },

  // 获取作品详情
  getWorkById: async (id: number): Promise<ApiResponse<Work>> => {
    const response = await axiosInstance.get<ApiResponse<Work>>(`/works/${id}`)
    return response.data
  },

  // 获取作品内容
  getWorkContent: async (id: number): Promise<ApiResponse<any>> => {
    const response = await axiosInstance.get<ApiResponse<any>>(`/works/${id}/content`)
    return response.data
  },

  // 创建作品
  createWork: async (data: CreateWorkRequest): Promise<ApiResponse<Work>> => {
    const response = await axiosInstance.post<ApiResponse<Work>>('/works', data)
    return response.data
  },

  // 更新作品
  updateWork: async (id: number, data: UpdateWorkRequest): Promise<ApiResponse<Work>> => {
    const response = await axiosInstance.put<ApiResponse<Work>>(`/works/${id}`, data)
    return response.data
  },

  // 删除作品
  deleteWork: async (id: number): Promise<ApiResponse<void>> => {
    const response = await axiosInstance.delete<ApiResponse<void>>(`/works/${id}`)
    return response.data
  },

  // 发布作品
  publishWork: async (id: number): Promise<ApiResponse<{ message: string; publishedAt: string }>> => {
    const response = await axiosInstance.post<ApiResponse<{ message: string; publishedAt: string }>>(`/works/${id}/publish`)
    return response.data
  },

  // 点赞作品（管理员可以查看点赞数据）
  likeWork: async (id: number): Promise<ApiResponse<{ message: string; likes: number }>> => {
    const response = await axiosInstance.post<ApiResponse<{ message: string; likes: number }>>(`/works/${id}/like`)
    return response.data
  },
}
