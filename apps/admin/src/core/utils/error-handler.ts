import { message } from 'antd'
import type { AxiosError } from 'axios'

interface ErrorResponse {
  error?: string
  message?: string
}

export const handleApiError = (error: unknown): string => {
  let errorMessage = '发生未知错误'

  if (isAxiosError(error)) {
    const axiosError = error as AxiosError<ErrorResponse>

    if (axiosError.response) {
      const { status, data } = axiosError.response

      switch (status) {
        case 400:
          errorMessage = data?.message || data?.error || '请求参数错误'
          break
        case 401:
          errorMessage = '登录已过期，请重新登录'
          break
        case 403:
          errorMessage = '权限不足，无法访问'
          break
        case 404:
          errorMessage = '请求的资源不存在'
          break
        case 429:
          errorMessage = '请求过于频繁，请稍后再试'
          break
        case 500:
          errorMessage = '服务器内部错误'
          break
        case 502:
          errorMessage = '网关错误'
          break
        case 503:
          errorMessage = '服务暂不可用'
          break
        default:
          errorMessage = data?.message || data?.error || `请求失败 (${status})`
      }
    } else if (axiosError.request) {
      errorMessage = '网络连接失败，请检查网络设置'
    } else {
      errorMessage = axiosError.message || '请求配置错误'
    }
  } else if (error instanceof Error) {
    errorMessage = error.message
  }

  return errorMessage
}

export const showError = (error: unknown): void => {
  const errorMessage = handleApiError(error)
  message.error(errorMessage)
}

const isAxiosError = (error: unknown): error is AxiosError => {
  return (error as AxiosError)?.isAxiosError === true
}
