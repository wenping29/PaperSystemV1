export const ROUTES = {
  LOGIN: '/login',
  DASHBOARD: '/dashboard',
  USERS: '/users',
  ADMIN_USERS: '/admin-users',
  WORKS: '/works',
  COMMENTS: '/comments',
  MESSAGES: '/messages',
  PAYMENTS: '/payments',
  SETTINGS: '/settings',
} as const

export const ROUTE_NAMES: Record<string, string> = {
  [ROUTES.LOGIN]: '登录',
  [ROUTES.DASHBOARD]: '仪表板',
  [ROUTES.USERS]: '用户管理',
  [ROUTES.ADMIN_USERS]: '管理员管理',
  [ROUTES.WORKS]: '作品管理',
  [ROUTES.COMMENTS]: '评论管理',
  [ROUTES.MESSAGES]: '消息管理',
  [ROUTES.PAYMENTS]: '支付管理',
  [ROUTES.SETTINGS]: '系统设置',
}
