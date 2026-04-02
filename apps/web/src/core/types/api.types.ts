// API相关类型定义

// 统一的API响应格式
export interface ApiResponse<T = any> {
  code: number
  message: string
  data: T
  timestamp: number
}

// 分页响应
export interface PaginatedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
  hasNext: boolean
  hasPrev: boolean
}

// 分页请求参数
export interface PaginationParams {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  keyword?: string
  filters?: Record<string, any>
}

// 错误响应
export interface ApiError {
  code: number
  message: string
  errors?: Record<string, string[]>
  timestamp: number
}

// 上传响应
export interface UploadResponse {
  url: string
  filename: string
  size: number
  mimeType: string
  uploadedAt: string
}

// 文件信息
export interface FileInfo {
  id: string
  name: string
  url: string
  size: number
  type: string
  uploadedAt: string
  uploadedBy: number
}

// WebSocket消息
export interface WebSocketMessage<T = any> {
  type: string
  data: T
  timestamp: number
  sender?: {
    id: number
    name: string
    avatar?: string
  }
}

// 认证相关类型
export interface LoginRequest {
  username: string
  password: string
  rememberMe?: boolean
}

export interface LoginResponse {
  token: string
  refreshToken: string
  expiresIn: number
  user: UserProfile
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
  confirmPassword: string
  agreeTerms: boolean
}

export interface RegisterResponse {
  userId: number
  message: string
  verificationRequired: boolean
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface RefreshTokenResponse {
  token: string
  refreshToken: string
  expiresIn: number
}

// 用户相关类型
export interface UserProfile {
  id: number
  username: string
  email: string
  avatar?: string
  bio?: string
  roles: string[]
  permissions: string[]
  createdAt: string
  updatedAt: string
  settings?: UserSettings
}

export interface UserSettings {
  theme: 'light' | 'dark' | 'auto'
  language: string
  notificationEnabled: boolean
  emailNotifications: boolean
  privacyLevel: 'public' | 'friends' | 'private'
}

export interface UserStats {
  worksCount: number
  followersCount: number
  followingCount: number
  likesReceived: number
  commentsReceived: number
  totalWords: number
}

// 作品相关类型
export interface Work {
  id: number
  title: string
  content: string
  summary?: string
  coverImage?: string
  author: UserProfile
  category: string
  tags: string[]
  status: 'draft' | 'published' | 'archived'
  visibility: 'public' | 'private' | 'friends'
  viewCount: number
  likeCount: number
  commentCount: number
  favoriteCount: number
  createdAt: string
  updatedAt: string
  publishedAt?: string
}

export interface WorkCreateRequest {
  title: string
  content: string
  summary?: string
  category: string
  tags: string[]
  visibility: 'public' | 'private' | 'friends'
}

export interface WorkUpdateRequest {
  title?: string
  content?: string
  summary?: string
  category?: string
  tags?: string[]
  visibility?: 'public' | 'private' | 'friends'
  status?: 'draft' | 'published' | 'archived'
}

// 评论相关类型
export interface Comment {
  id: number
  content: string
  author: UserProfile
  workId: number
  parentId?: number
  replies: Comment[]
  likeCount: number
  createdAt: string
  updatedAt: string
}

export interface CommentCreateRequest {
  content: string
  workId: number
  parentId?: number
}

// 支付相关类型
export interface DonationRequest {
  workId: number
  amount: number
  paymentMethod: 'wechat' | 'alipay' | 'bankcard'
  message?: string
}

export interface DonationResponse {
  transactionId: string
  qrCodeUrl?: string
  paymentUrl?: string
  amount: number
  status: 'pending' | 'paid' | 'failed'
}

export interface Transaction {
  id: string
  amount: number
  type: 'donation' | 'withdrawal'
  status: 'pending' | 'completed' | 'failed'
  paymentMethod: string
  createdAt: string
  updatedAt: string
  relatedWork?: Work
  relatedUser?: UserProfile
}

// 聊天相关类型
export interface ChatMessage {
  id: string
  senderId: number
  senderName: string
  senderAvatar?: string
  content: string
  type: 'text' | 'image' | 'file'
  status: 'sending' | 'sent' | 'delivered' | 'read'
  timestamp: number
  conversationId: number
}

export interface Conversation {
  id: number
  participants: UserProfile[]
  lastMessage?: ChatMessage
  unreadCount: number
  updatedAt: string
}

// 通知相关类型
export interface Notification {
  id: string
  type: 'like' | 'comment' | 'follow' | 'donation' | 'system'
  title: string
  content: string
  read: boolean
  data?: Record<string, any>
  createdAt: string
}