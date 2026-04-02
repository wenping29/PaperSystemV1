export interface ApiResponse<T = any> {
  code?: number
  message?: string
  data: T
  timestamp?: number
}

export interface PaginatedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface PaginationParams {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface ErrorResponse {
  error: string
  message?: string
}
