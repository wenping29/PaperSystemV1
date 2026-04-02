// 领域实体类型定义

// 基础实体接口
export interface BaseEntity {
  id: number
  createdAt: string
  updatedAt: string
  deletedAt?: string
}

// 用户实体
export interface User extends BaseEntity {
  username: string
  email: string
  passwordHash: string
  avatar?: string
  bio?: string
  isActive: boolean
  isVerified: boolean
  lastLoginAt?: string
  settings: UserSettings
}

// 作品实体
export interface Work extends BaseEntity {
  title: string
  content: string
  summary?: string
  coverImage?: string
  authorId: number
  category: string
  tags: string[]
  status: 'draft' | 'published' | 'archived'
  visibility: 'public' | 'private' | 'friends'
  viewCount: number
  likeCount: number
  commentCount: number
  favoriteCount: number
  wordCount: number
  characterCount: number
  publishedAt?: string
  version: number
}

// 作品版本实体（用于版本控制）
export interface WorkVersion extends BaseEntity {
  workId: number
  version: number
  title: string
  content: string
  summary?: string
  authorId: number
  changes: string[]
}

// 评论实体
export interface Comment extends BaseEntity {
  content: string
  authorId: number
  workId: number
  parentId?: number
  likeCount: number
  depth: number
  path: string // 用于嵌套评论的路径，如 "1/2/3"
}

// 点赞实体
export interface Like extends BaseEntity {
  userId: number
  targetType: 'work' | 'comment'
  targetId: number
}

// 收藏实体
export interface Favorite extends BaseEntity {
  userId: number
  workId: number
}

// 关注实体
export interface Follow extends BaseEntity {
  followerId: number
  followingId: number
}

// 分类实体
export interface Category extends BaseEntity {
  name: string
  slug: string
  description?: string
  parentId?: number
  order: number
  isActive: boolean
}

// 标签实体
export interface Tag extends BaseEntity {
  name: string
  slug: string
  description?: string
  usageCount: number
}

// 文件实体
export interface File extends BaseEntity {
  name: string
  originalName: string
  path: string
  url: string
  size: number
  mimeType: string
  uploadedById: number
  isPublic: boolean
  metadata?: Record<string, any>
}

// 消息实体
export interface Message extends BaseEntity {
  senderId: number
  receiverId: number
  content: string
  type: 'text' | 'image' | 'file'
  status: 'sent' | 'delivered' | 'read'
  conversationId: number
}

// 会话实体
export interface Conversation extends BaseEntity {
  type: 'direct' | 'group'
  name?: string
  avatar?: string
  lastMessageId?: number
  lastMessageAt?: string
}

// 会话参与者实体
export interface ConversationParticipant extends BaseEntity {
  conversationId: number
  userId: number
  role: 'member' | 'admin'
  joinedAt: string
  leftAt?: string
  unreadCount: number
}

// 通知实体
export interface Notification extends BaseEntity {
  userId: number
  type: 'like' | 'comment' | 'follow' | 'donation' | 'system' | 'mention'
  title: string
  content: string
  data?: Record<string, any>
  read: boolean
  readAt?: string
}

// 交易实体
export interface Transaction extends BaseEntity {
  userId: number
  type: 'donation' | 'withdrawal' | 'refund'
  amount: number
  currency: string
  status: 'pending' | 'completed' | 'failed' | 'cancelled'
  paymentMethod: string
  paymentId?: string
  recipientId?: number
  workId?: number
  description?: string
  metadata?: Record<string, any>
}

// 余额实体
export interface Balance extends BaseEntity {
  userId: number
  available: number
  frozen: number
  total: number
  currency: string
  lastUpdatedAt: string
}

// 模板实体
export interface Template extends BaseEntity {
  name: string
  content: string
  description?: string
  category: string
  tags: string[]
  authorId: number
  isPublic: boolean
  usageCount: number
  rating: number
}

// AI提示实体
export interface AIPrompt extends BaseEntity {
  name: string
  content: string
  description?: string
  category: string
  parameters: Record<string, any>
  authorId: number
  isPublic: boolean
  usageCount: number
  rating: number
}

// 审核记录实体
export interface AuditLog extends BaseEntity {
  userId: number
  action: string
  targetType: string
  targetId: number
  details?: Record<string, any>
  ipAddress?: string
  userAgent?: string
}

// 系统配置实体
export interface SystemConfig extends BaseEntity {
  key: string
  value: string
  description?: string
  isEncrypted: boolean
  isPublic: boolean
}