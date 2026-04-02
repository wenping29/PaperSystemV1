export const appConfig = {
  title: import.meta.env.VITE_APP_TITLE || 'PaperSystem 管理后台',
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api/v1',
  tokenKey: 'admin_token',
  userKey: 'admin_user',
  defaultPageSize: 20,
  pageSizeOptions: [10, 20, 50, 100],
} as const
