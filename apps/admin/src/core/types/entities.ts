export interface User {
  id: number
  username: string
  email: string
  phone?: string
  bio?: string
  avatarUrl?: string
  location?: string
  website?: string
  role: UserRole
  status: UserStatus
  createdAt: string
  lastLoginAt?: string
  profile?: UserProfile
}

export interface UserProfile {
  biography?: string
  writingCount: number
  likeCount: number
  followersCount: number
  followingCount: number
  totalWords: number
  birthDate?: string
  gender?: string
  twitterUrl?: string
  gitHubUrl?: string
  linkedInUrl?: string
}

export type UserRole = 'user' | 'vip' | 'admin' | 'superadmin'
export type UserStatus = 'active' | 'inactive' | 'banned' | 'suspended'

// ========== 作品相关 ==========
export interface Work {
  id: number
  title: string
  author?: string
  authorId?: number
  authorName?: string
  excerpt?: string
  content?: string
  wordCount: number
  category?: string
  tags?: string[]
  createdAt: string
  updatedAt?: string
  likes: number
  views?: number
  isPublished: boolean
  status?: WorkStatus
  visibility?: WorkVisibility
}

export type WorkStatus = 'draft' | 'published' | 'archived'
export type WorkVisibility = 'public' | 'private' | 'followers'

// ========== 评论相关 ==========
export interface Comment {
  id: number
  postId?: number
  workId?: number
  authorId?: number
  authorName?: string
  parentId?: number
  content: string
  likeCount: number
  createdAt: string
  updatedAt?: string
  isDeleted?: boolean
  replies?: Comment[]
  status?: CommentStatus
}

export type CommentStatus = 'published' | 'hidden' | 'deleted'

// ========== 系统设置相关 ==========
export interface SystemSettings {
  siteName: string
  siteDescription: string
  siteLogo?: string
  enableRegistration: boolean
  enableComments: boolean
  enableAiFeatures: boolean
  maxUploadSize: number
  allowedFileTypes: string[]
  maintenanceMode: boolean
  maintenanceMessage?: string
}

export interface AdminActivityLog {
  id: number
  adminId: number
  adminName: string
  action: string
  targetType: string
  targetId?: number
  details?: string
  ipAddress?: string
  userAgent?: string
  createdAt: string
}

// ========== 统计数据 ==========
export interface DashboardStats {
  summary: {
    users: {
      total: number
      todayNew: number
      weeklyNew: number
      monthlyNew: number
      trend: Array<{ date: string; count: number }>
    }
    works: {
      total: number
      todayNew: number
      weeklyNew: number
      monthlyNew: number
      published: number
      drafts: number
      trend: Array<{ date: string; count: number }>
    }
    comments: {
      total: number
      todayNew: number
      pendingReview: number
      trend: Array<{ date: string; count: number }>
    }
    aiUsage: {
      total: number
      todayNew: number
      trend: Array<{ date: string; count: number }>
    }
    payments: {
      total: number
      todayAmount: number
      totalAmount: number
      trend: Array<{ date: string; amount: number }>
    }
  }
  systemStatus: {
    database: 'healthy' | 'warning' | 'error'
    cache: 'healthy' | 'warning' | 'error'
    storage: 'healthy' | 'warning' | 'error'
    cpu: number
    memory: number
    disk: number
  }
  recentActivities: Array<{
    id: number
    type: string
    description: string
    userId?: number
    username?: string
    createdAt: string
  }>
  topUsers: Array<{
    id: number
    username: string
    avatarUrl?: string
    writingCount: number
    followersCount: number
  }>
  topWorks: Array<{
    id: number
    title: string
    authorName: string
    views: number
    likes: number
    createdAt: string
  }>
}

export interface Work {
  id: number
  userId: number
  title: string
  content: string
  summary?: string
  coverUrl?: string
  status: WorkStatus
  visibility: WorkVisibility
  wordCount: number
  viewCount: number
  likeCount: number
  commentCount: number
  createdAt: string
  updatedAt: string
  author?: User
}

export type WorkStatus = 'draft' | 'published' | 'archived'
export type WorkVisibility = 'public' | 'private' | 'followers'

export interface Comment {
  id: number
  workId: number
  userId: number
  parentId?: number
  content: string
  likeCount: number
  createdAt: string
  updatedAt: string
  user?: User
  replies?: Comment[]
}

export interface Message {
  id: number
  senderId: number
  receiverId: number
  content: string
  isRead: boolean
  createdAt: string
  sender?: User
  receiver?: User
}

export interface Payment {
  id: number
  userId: number
  amount: number
  type: PaymentType
  status: PaymentStatus
  orderId?: string
  description?: string
  createdAt: string
  user?: User
}

export type PaymentType = 'recharge' | 'purchase' | 'reward' | 'withdraw'
export type PaymentStatus = 'pending' | 'completed' | 'failed' | 'refunded'

export interface DashboardStats {
  summary: {
    users: {
      total: number
      todayNew: number
      trend: Array<{ date: string; count: number }>
    }
    works: {
      total: number
      todayNew: number
      trend: Array<{ date: string; count: number }>
    }
    aiUsage: {
      total: number
      todayNew: number
      trend: Array<{ date: string; count: number }>
    }
    payments: {
      total: number
      todayAmount: number
      totalAmount: number
      trend: Array<{ date: string; amount: number }>
    }
  }
  systemStatus: {
    database: 'healthy' | 'warning' | 'error'
    cache: 'healthy' | 'warning' | 'error'
    storage: 'healthy' | 'warning' | 'error'
  }
  recentActivities: Array<{
    id: number
    type: string
    description: string
    userId?: number
    username?: string
    createdAt: string
  }>
}
