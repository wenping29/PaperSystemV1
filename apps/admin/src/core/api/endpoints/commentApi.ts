import axiosInstance from '../client'
import type { ApiResponse, PaginationParams } from '@/core/types/api.types'
import type { Comment, CommentStatus } from '@/core/types/entities'

export interface GetCommentsParams extends PaginationParams {
  search?: string
  postId?: number
  workId?: number
  authorId?: number
  parentId?: number
  status?: CommentStatus
  sortBy?: string
}

export interface CreateCommentRequest {
  postId?: number
  workId?: number
  parentId?: number
  content: string
}

export interface UpdateCommentRequest {
  content?: string
  status?: CommentStatus
}

export const commentApi = {
  // 获取评论列表
  getComments: async (params?: GetCommentsParams): Promise<ApiResponse<any>> => {
    const response = await axiosInstance.get<ApiResponse<any>>('/comments', { params })
    return response.data
  },

  // 获取评论详情
  getCommentById: async (id: number): Promise<ApiResponse<Comment>> => {
    const response = await axiosInstance.get<ApiResponse<Comment>>(`/comments/${id}`)
    return response.data
  },

  // 获取评论回复
  getReplies: async (id: number, page = 1, pageSize = 20): Promise<ApiResponse<any>> => {
    const response = await axiosInstance.get<ApiResponse<any>>(`/comments/${id}/replies`, {
      params: { page, pageSize },
    })
    return response.data
  },

  // 获取评论统计
  getCommentStats: async (id: number): Promise<ApiResponse<any>> => {
    const response = await axiosInstance.get<ApiResponse<any>>(`/comments/${id}/stats`)
    return response.data
  },

  // 创建评论
  createComment: async (data: CreateCommentRequest): Promise<ApiResponse<Comment>> => {
    const response = await axiosInstance.post<ApiResponse<Comment>>('/comments', data)
    return response.data
  },

  // 更新评论
  updateComment: async (id: number, data: UpdateCommentRequest): Promise<ApiResponse<Comment>> => {
    const response = await axiosInstance.put<ApiResponse<Comment>>(`/comments/${id}`, data)
    return response.data
  },

  // 删除评论
  deleteComment: async (id: number): Promise<ApiResponse<void>> => {
    const response = await axiosInstance.delete<ApiResponse<void>>(`/comments/${id}`)
    return response.data
  },

  // 点赞评论
  likeComment: async (id: number): Promise<ApiResponse<{ message: string }>> => {
    const response = await axiosInstance.post<ApiResponse<{ message: string }>>(`/comments/${id}/like`)
    return response.data
  },

  // 取消点赞评论
  unlikeComment: async (id: number): Promise<ApiResponse<{ message: string }>> => {
    const response = await axiosInstance.delete<ApiResponse<{ message: string }>>(`/comments/${id}/like`)
    return response.data
  },
}
