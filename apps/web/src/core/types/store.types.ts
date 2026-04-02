// Redux Store类型定义

import { store } from '@/store'

// RootState和AppDispatch类型
export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

// 认证状态
export interface AuthState {
  token: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
  user: UserProfile | null
}

// 用户状态
export interface UserState {
  currentUser: UserProfile | null
  users: Record<number, UserProfile>
  isLoading: boolean
  error: string | null
}

// 作品状态
export interface WritingState {
  works: Work[]
  currentWork: Work | null
  drafts: Work[]
  templates: Template[]
  isLoading: boolean
  isSaving: boolean
  error: string | null
  pagination: {
    page: number
    pageSize: number
    total: number
    hasMore: boolean
  }
}

// 社区状态
export interface CommunityState {
  works: Work[]
  featuredWorks: Work[]
  categories: Category[]
  tags: Tag[]
  isLoading: boolean
  error: string | null
  filters: {
    category: string | null
    tags: string[]
    sortBy: string
    keyword: string
  }
}

// 聊天状态
export interface ChatState {
  conversations: Conversation[]
  currentConversation: Conversation | null
  messages: Record<number, ChatMessage[]>
  isLoading: boolean
  isConnected: boolean
  error: string | null
  unreadCount: number
}

// 通知状态
export interface NotificationState {
  notifications: Notification[]
  unreadCount: number
  isLoading: boolean
  error: string | null
}

// 支付状态
export interface PaymentState {
  balance: number
  transactions: Transaction[]
  isLoading: boolean
  error: string | null
}

// UI状态
export interface UIState {
  theme: 'light' | 'dark'
  language: string
  sidebarCollapsed: boolean
  modalStack: Array<{
    id: string
    component: React.ComponentType<any>
    props?: any
  }>
  toastQueue: Array<{
    id: string
    type: 'success' | 'error' | 'warning' | 'info'
    message: string
    duration?: number
  }>
}

// 应用状态
export interface AppState {
  initialized: boolean
  loading: boolean
  error: string | null
  version: string
  environment: 'development' | 'production' | 'test'
}

// 完整的Redux状态树
export interface ReduxState {
  auth: AuthState
  user: UserState
  writing: WritingState
  community: CommunityState
  chat: ChatState
  notification: NotificationState
  payment: PaymentState
  ui: UIState
  app: AppState
}

// Action类型工具
export type Action<T = any> = {
  type: string
  payload?: T
  error?: boolean
  meta?: any
}

// Thunk Action类型
export type ThunkAction<ReturnType = void> = (
  dispatch: AppDispatch,
  getState: () => RootState
) => ReturnType

// Async Thunk配置
export interface AsyncThunkConfig {
  state: RootState
  dispatch: AppDispatch
  rejectValue: string
}

// RTK Query缓存标签类型
export type CacheTag =
  | 'Auth'
  | 'User'
  | 'Work'
  | 'Template'
  | 'Draft'
  | 'Comment'
  | 'Like'
  | 'Favorite'
  | 'Category'
  | 'Tag'
  | 'Conversation'
  | 'Message'
  | 'Notification'
  | 'Transaction'

export type CacheTagWithId = {
  type: CacheTag
  id: string | number
}

// 选择器类型
export type Selector<T> = (state: RootState) => T
export type SelectorWithProps<T, P> = (state: RootState, props: P) => T

// 创建选择器函数的类型
export type CreateSelector = <
  InputSelectors extends Selector<any>[],
  Result
>(
  ...args: [...InputSelectors, (...args: any[]) => Result]
) => Selector<Result>

// Redux持久化配置
export interface PersistConfig {
  key: string
  storage: any
  whitelist?: string[]
  blacklist?: string[]
  transforms?: any[]
  migrate?: (state: any, version: number) => Promise<any>
  version?: number
  debug?: boolean
}

// Store中间件配置
export interface MiddlewareConfig {
  thunk?: boolean
  immutableCheck?: boolean
  serializableCheck?: boolean
  actionCreatorCheck?: boolean
}