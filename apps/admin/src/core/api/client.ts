import axios, { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { message } from 'antd'
import { appConfig } from '@/core/config/app.config'
import { STORAGE_KEYS } from '@/core/constants/storage-keys'
import { ROUTES } from '@/core/constants/routes'

// 创建axios实例
const axiosInstance: AxiosInstance = axios.create({
  baseURL: appConfig.apiBaseUrl,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
})

// 请求拦截器
axiosInstance.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN)

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }

    // 添加请求ID用于追踪
    config.headers['X-Request-ID'] = crypto.randomUUID()

    // 开发环境添加调试头
    if (import.meta.env.DEV) {
      config.headers['X-Debug-Mode'] = 'true'
    }

    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
axiosInstance.interceptors.response.use(
  (response: AxiosResponse) => {
    return response
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    // 401错误且不是刷新token请求，尝试刷新token
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      // 清除认证状态并跳转到登录页
      localStorage.removeItem(STORAGE_KEYS.TOKEN)
      localStorage.removeItem(STORAGE_KEYS.USER)
      message.warning('登录已过期，请重新登录')
      window.location.href = ROUTES.LOGIN
      return Promise.reject(error)
    }

    return Promise.reject(error)
  }
)

export default axiosInstance
