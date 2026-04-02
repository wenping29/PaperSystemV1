// 格式化工具函数

import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'

// 设置dayjs默认语言
dayjs.locale('zh-cn')

/**
 * 日期时间格式化
 */
export const formatDate = {
  // 完整日期时间
  full: (date: string | Date | number): string => {
    return dayjs(date).format('YYYY-MM-DD HH:mm:ss')
  },

  // 相对时间（如：3分钟前）
  relative: (date: string | Date | number): string => {
    const now = dayjs()
    const target = dayjs(date)
    const diffSeconds = now.diff(target, 'second')
    const diffMinutes = now.diff(target, 'minute')
    const diffHours = now.diff(target, 'hour')
    const diffDays = now.diff(target, 'day')
    const diffMonths = now.diff(target, 'month')
    const diffYears = now.diff(target, 'year')

    if (diffSeconds < 60) {
      return '刚刚'
    } else if (diffMinutes < 60) {
      return `${diffMinutes}分钟前`
    } else if (diffHours < 24) {
      return `${diffHours}小时前`
    } else if (diffDays < 30) {
      return `${diffDays}天前`
    } else if (diffMonths < 12) {
      return `${diffMonths}个月前`
    } else {
      return `${diffYears}年前`
    }
  },

  // 聊天时间格式
  chatTime: (date: string | Date | number): string => {
    const target = dayjs(date)
    const now = dayjs()

    if (target.isSame(now, 'day')) {
      return target.format('HH:mm')
    } else if (target.isSame(now.subtract(1, 'day'), 'day')) {
      return '昨天 ' + target.format('HH:mm')
    } else if (target.isSame(now, 'year')) {
      return target.format('MM-DD HH:mm')
    } else {
      return target.format('YYYY-MM-DD HH:mm')
    }
  },

  // 短日期格式
  shortDate: (date: string | Date | number): string => {
    return dayjs(date).format('YYYY-MM-DD')
  },

  // 月份格式
  monthYear: (date: string | Date | number): string => {
    return dayjs(date).format('YYYY年MM月')
  },
}

/**
 * 数字格式化
 */
export const formatNumber = {
  // 千位分隔符
  thousands: (num: number): string => {
    return num.toLocaleString('zh-CN')
  },

  // 文件大小
  fileSize: (bytes: number): string => {
    if (bytes === 0) return '0 B'

    const k = 1024
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))

    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  },

  // 百分比
  percent: (num: number, decimal = 2): string => {
    return (num * 100).toFixed(decimal) + '%'
  },

  // 金额格式（人民币）
  currency: (amount: number): string => {
    return '¥' + amount.toFixed(2)
  },

  // 缩写数字（如：1.2k, 3.4m）
  compact: (num: number): string => {
    if (num < 1000) return num.toString()
    if (num < 1000000) return (num / 1000).toFixed(1) + 'k'
    if (num < 1000000000) return (num / 1000000).toFixed(1) + 'm'
    return (num / 1000000000).toFixed(1) + 'b'
  },
}

/**
 * 文本格式化
 */
export const formatText = {
  // 截断文本
  truncate: (text: string, maxLength: number, suffix = '...'): string => {
    if (text.length <= maxLength) return text
    return text.substring(0, maxLength) + suffix
  },

  // 移除HTML标签
  stripHtml: (html: string): string => {
    return html.replace(/<[^>]*>/g, '')
  },

  // 首字母大写
  capitalize: (text: string): string => {
    return text.charAt(0).toUpperCase() + text.slice(1).toLowerCase()
  },

  // 生成摘要（移除HTML标签并截断）
  excerpt: (html: string, maxLength = 200): string => {
    const plainText = formatText.stripHtml(html)
    return formatText.truncate(plainText, maxLength)
  },

  // 密码强度显示
  passwordStrength: (password: string): { score: number; label: string; color: string } => {
    let score = 0

    // 长度评分
    if (password.length >= 8) score += 1
    if (password.length >= 12) score += 1

    // 复杂度评分
    if (/[a-z]/.test(password)) score += 1
    if (/[A-Z]/.test(password)) score += 1
    if (/[0-9]/.test(password)) score += 1
    if (/[^a-zA-Z0-9]/.test(password)) score += 1

    // 根据评分返回结果
    if (score <= 2) {
      return { score, label: '弱', color: '#ff4d4f' }
    } else if (score <= 4) {
      return { score, label: '中', color: '#faad14' }
    } else {
      return { score, label: '强', color: '#52c41a' }
    }
  },
}

/**
 * URL格式化
 */
export const formatUrl = {
  // 确保URL有协议
  ensureProtocol: (url: string): string => {
    if (!url.startsWith('http://') && !url.startsWith('https://')) {
      return 'https://' + url
    }
    return url
  },

  // 提取域名
  extractDomain: (url: string): string => {
    try {
      const urlObj = new URL(formatUrl.ensureProtocol(url))
      return urlObj.hostname
    } catch {
      return url
    }
  },

  // 生成头像URL
  avatarUrl: (userId: number, size = 100): string => {
    return `https://api.dicebear.com/7.x/avataaars/svg?seed=${userId}&size=${size}`
  },

  // 作品封面URL
  workCoverUrl: (workId: number, size = 'medium'): string => {
    const sizes = {
      small: '400x300',
      medium: '800x600',
      large: '1200x900',
    }
    return `https://picsum.photos/seed/${workId}/${sizes[size as keyof typeof sizes]}`
  },
}

/**
 * 时间格式化
 */
export const formatDuration = {
  // 秒转换为时分秒
  secondsToHMS: (seconds: number): string => {
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    const remainingSeconds = seconds % 60

    const parts = []
    if (hours > 0) parts.push(`${hours}小时`)
    if (minutes > 0) parts.push(`${minutes}分钟`)
    if (remainingSeconds > 0 || parts.length === 0) {
      parts.push(`${remainingSeconds}秒`)
    }

    return parts.join('')
  },

  // 毫秒转换为可读格式
  milliseconds: (ms: number): string => {
    if (ms < 1000) return `${ms}ms`
    if (ms < 60000) return `${(ms / 1000).toFixed(1)}s`
    if (ms < 3600000) return `${(ms / 60000).toFixed(1)}min`
    return `${(ms / 3600000).toFixed(1)}h`
  },
}

/**
 * 颜色格式化
 */
export const formatColor = {
  // HEX转RGB
  hexToRgb: (hex: string): { r: number; g: number; b: number } => {
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex)
    return result ? {
      r: parseInt(result[1], 16),
      g: parseInt(result[2], 16),
      b: parseInt(result[3], 16),
    } : { r: 0, g: 0, b: 0 }
  },

  // 计算对比色（黑白）
  contrastColor: (hexColor: string): '#000000' | '#ffffff' => {
    const rgb = formatColor.hexToRgb(hexColor)
    // 计算亮度（YIQ公式）
    const brightness = (rgb.r * 299 + rgb.g * 587 + rgb.b * 114) / 1000
    return brightness > 128 ? '#000000' : '#ffffff'
  },

  // 生成随机颜色
  random: (): string => {
    const letters = '0123456789ABCDEF'
    let color = '#'
    for (let i = 0; i < 6; i++) {
      color += letters[Math.floor(Math.random() * 16)]
    }
    return color
  },
}

/**
 * 数据格式化
 */
export const formatData = {
  // 对象转查询字符串
  toQueryString: (params: Record<string, any>): string => {
    const searchParams = new URLSearchParams()

    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        if (Array.isArray(value)) {
          value.forEach(item => searchParams.append(key, String(item)))
        } else {
          searchParams.append(key, String(value))
        }
      }
    })

    return searchParams.toString()
  },

  // 查询字符串转对象
  fromQueryString: (queryString: string): Record<string, string | string[]> => {
    const params = new URLSearchParams(queryString)
    const result: Record<string, string | string[]> = {}

    for (const [key, value] of params.entries()) {
      if (key in result) {
        const existing = result[key]
        if (Array.isArray(existing)) {
          existing.push(value)
        } else {
          result[key] = [existing as string, value]
        }
      } else {
        result[key] = value
      }
    }

    return result
  },

  // 深度克隆对象
  deepClone: <T>(obj: T): T => {
    return JSON.parse(JSON.stringify(obj))
  },

  // 合并对象（深度合并）
  deepMerge: <T extends Record<string, any>>(target: T, source: T): T => {
    const output = { ...target }

    for (const key in source) {
      if (source.hasOwnProperty(key)) {
        if (isObject(source[key]) && isObject(target[key])) {
          output[key] = formatData.deepMerge(target[key], source[key])
        } else {
          output[key] = source[key]
        }
      }
    }

    return output
  },
}

// 辅助函数
const isObject = (item: any): boolean => {
  return item && typeof item === 'object' && !Array.isArray(item)
}