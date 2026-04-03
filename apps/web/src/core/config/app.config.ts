// 应用配置

export const APP_CONFIG = {
  // 应用信息
  appName: '写作平台',
  appVersion: '1.0.0',

  // API配置
  api: {
    baseURL: import.meta.env.VITE_API_BASE_URL,
    timeout: 30000,
    retryCount: 3,
  },

  // WebSocket配置
  websocket: {
    chat: import.meta.env.VITE_WS_URL || 'ws://localhost:5000/ws/chat',
    notifications: import.meta.env.VITE_WS_URL || 'ws://localhost:5000/ws/notifications',
    reconnectDelay: 1000,
    maxReconnectAttempts: 5,
  },

  // 本地存储配置
  storage: {
    tokenKey: 'token',
    refreshTokenKey: 'refreshToken',
    userKey: 'user',
    themeKey: 'theme',
    languageKey: 'language',
  },

  // 分页配置
  pagination: {
    defaultPageSize: 12,
    pageSizeOptions: ['12', '24', '48', '96'],
    maxPageSize: 100,
  },

  // 上传配置
  upload: {
    maxFileSize: 10 * 1024 * 1024, // 10MB
    allowedImageTypes: ['image/jpeg', 'image/png', 'image/gif', 'image/webp'],
    allowedDocumentTypes: [
      'application/pdf',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'text/plain',
      'text/markdown',
    ],
  },

  // 编辑器配置
  editor: {
    autosaveInterval: 5000, // 5秒
    maxContentLength: 100000, // 10万字
    defaultFontSize: 16,
  },

  // 安全配置
  security: {
    passwordMinLength: 6,
    passwordMaxLength: 32,
    usernameMinLength: 3,
    usernameMaxLength: 20,
    tokenRefreshThreshold: 300, // 5分钟
  },

  // 功能开关
  features: {
    aiWriting: true,
    realtimeChat: true,
    community: true,
    payment: true,
    darkMode: true,
    i18n: false,
  },
} as const

// 环境配置
export const ENV_CONFIG = {
  isDevelopment: import.meta.env.DEV,
  isProduction: import.meta.env.PROD,
  isTest: import.meta.env.MODE === 'test',
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL,
  wsBaseUrl: import.meta.env.VITE_WS_URL,
  sentryDsn: import.meta.env.VITE_SENTRY_DSN,
} as const