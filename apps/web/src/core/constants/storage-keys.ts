// 本地存储键名配置

export const STORAGE_KEYS = {
  // 认证相关
  TOKEN: 'writing_platform_token',
  REFRESH_TOKEN: 'writing_platform_refresh_token',
  USER_INFO: 'writing_platform_user_info',
  SESSION_ID: 'writing_platform_session_id',

  // 用户偏好
  THEME: 'writing_platform_theme',
  LANGUAGE: 'writing_platform_language',
  FONT_SIZE: 'writing_platform_font_size',
  EDITOR_SETTINGS: 'writing_platform_editor_settings',

  // 应用状态
  LAST_VISITED_ROUTE: 'writing_platform_last_visited_route',
  REDIRECT_AFTER_LOGIN: 'writing_platform_redirect_after_login',
  FORM_DRAFT_PREFIX: 'writing_platform_form_draft_',

  // 缓存数据
  CACHE_PREFIX: 'writing_platform_cache_',
  WORK_CACHE_PREFIX: 'writing_platform_work_',
  USER_CACHE_PREFIX: 'writing_platform_user_',

  // 功能设置
  NOTIFICATION_SETTINGS: 'writing_platform_notification_settings',
  PRIVACY_SETTINGS: 'writing_platform_privacy_settings',
  AI_SETTINGS: 'writing_platform_ai_settings',

  // 临时数据
  UPLOAD_QUEUE: 'writing_platform_upload_queue',
  DRAFT_CONTENT: 'writing_platform_draft_content',

  // 统计信息
  USAGE_STATS: 'writing_platform_usage_stats',
  ERROR_LOGS: 'writing_platform_error_logs',
} as const

// Session Storage键名（标签页关闭后清除）
export const SESSION_KEYS = {
  CURRENT_WORK_ID: 'writing_platform_current_work_id',
  CHAT_HISTORY: 'writing_platform_chat_history',
  EDITOR_STATE: 'writing_platform_editor_state',
  FORM_STATE_PREFIX: 'writing_platform_form_state_',
} as const

// IndexedDB数据库配置
export const DB_CONFIG = {
  NAME: 'WritingPlatformDB',
  VERSION: 1,
  STORES: {
    WORKS: 'works',
    MESSAGES: 'messages',
    NOTIFICATIONS: 'notifications',
    CACHE: 'cache',
  },
} as const