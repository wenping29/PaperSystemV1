import axios, { AxiosError, AxiosRequestConfig, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { store } from '@/store'
import { clearCredentials, setCredentials } from '@/store/slices/authSlice'
import { refreshToken } from '@/store/api/authApi'

// 创建axios实例
const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 30000, // 30秒超时
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
})


// 请求拦截器
axiosInstance.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = store.getState().auth.token

    if (token) {
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
    // 统一响应格式处理
    const { data } = response
    if (data?.code !== undefined && data.code !== 0) {
      return Promise.reject(new Error(data.message || '请求失败'))
    }
    return data?.data ?? data
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    // 401错误且不是刷新token请求，尝试刷新token
    if (error.response?.status === 401 && !originalRequest._retry && !originalRequest.url?.includes('/auth/refresh')) {
      originalRequest._retry = true

      try {
        // 调用刷新token接口
        const refreshResult = await store.dispatch(refreshToken()).unwrap()

        // 更新store中的token
        store.dispatch(setCredentials(refreshResult))

        // 重试原始请求
        originalRequest.headers.Authorization = `Bearer ${refreshResult.token}`
        return axiosInstance(originalRequest)
      } catch (refreshError) {
        // 刷新token失败，清空认证状态并跳转到登录页
        store.dispatch(clearCredentials())
        window.location.href = '/login'
        return Promise.reject(refreshError)
      }
    }

    // 其他错误处理
    const errorMessage = getErrorMessage(error)
    console.error('API请求错误:', errorMessage)

    // 统一错误处理
    if (error.response?.status === 403) {
      // 权限不足
      console.warn('权限不足，访问被拒绝')
    } else if (error.response?.status === 429) {
      // 请求过于频繁
      console.warn('请求过于频繁，请稍后重试')
    } else if (!error.response) {
      // 网络错误
      console.warn('网络连接失败，请检查网络设置')
    }

    return Promise.reject(error)
  }
)

// 错误消息提取函数
function getErrorMessage(error: AxiosError): string {
  if (error.response?.data) {
    const data = error.response.data as any
    return data.message || data.error || '请求失败'
  }
  if (error.request) {
    return '网络请求失败，请检查网络连接'
  }
  return error.message || '未知错误'
}

export default axiosInstance