// 应用路由配置

export const ROUTES = {
  // 公共路由
  HOME: '/',
  LOGIN: '/login',
  REGISTER: '/register',
  FORGOT_PASSWORD: '/forgot-password',
  RESET_PASSWORD: '/reset-password',
  ABOUT: '/about',
  CONTACT: '/contact',
  PRIVACY: '/privacy',
  TERMS: '/terms',

  // 认证后路由
  DASHBOARD: '/dashboard',
  WRITING: '/writing',
  WRITING_EDIT: (workId: string | number) => `/writing/${workId}`,
  WRITING_NEW: '/writing/new',
  COMMUNITY: '/community',
  COMMUNITY_WORK: (workId: string | number) => `/community/works/${workId}`,
  PROFILE: '/profile',
  SETTINGS: '/settings',
  NOTIFICATIONS: '/notifications',
  MESSAGES: '/messages',
  PAYMENT: '/payment',
  PAYMENT_HISTORY: '/payment/history',

  // 管理路由
  ADMIN: '/admin',
  ADMIN_USERS: '/admin/users',
  ADMIN_WORKS: '/admin/works',
  ADMIN_REPORTS: '/admin/reports',
  ADMIN_SETTINGS: '/admin/settings',

  // API路由
  API: {
    AUTH: {
      LOGIN: '/api/v1/auth/login',
      REGISTER: '/api/v1/auth/register',
      LOGOUT: '/api/v1/auth/logout',
      REFRESH: '/api/v1/auth/refresh',
      PROFILE: '/api/v1/auth/profile',
    },
    USER: {
      LIST: '/api/v1/users',
      DETAIL: (id: string | number) => `/api/v1/users/${id}`,
      UPDATE: (id: string | number) => `/api/v1/users/${id}`,
      DELETE: (id: string | number) => `/api/v1/users/${id}`,
    },
    WRITING: {
      WORKS: '/api/v1/works',
      WORK_DETAIL: (id: string | number) => `/api/v1/works/${id}`,
      DRAFTS: '/api/v1/works/drafts',
      TEMPLATES: '/api/v1/works/templates',
      AI_SUGGEST: '/api/v1/works/ai/suggest',
    },
    COMMUNITY: {
      WORKS: '/api/v1/community/works',
      WORK_DETAIL: (id: string | number) => `/api/v1/community/works/${id}`,
      COMMENTS: (workId: string | number) => `/api/v1/community/works/${workId}/comments`,
      LIKES: (workId: string | number) => `/api/v1/community/works/${workId}/likes`,
      FAVORITES: (workId: string | number) => `/api/v1/community/works/${workId}/favorites`,
    },
    PAYMENT: {
      DONATE: '/api/v1/payment/donate',
      WITHDRAW: '/api/v1/payment/withdraw',
      BALANCE: '/api/v1/payment/balance',
      HISTORY: '/api/v1/payment/history',
    },
  },
} as const