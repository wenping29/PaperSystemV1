import { useState, useEffect, useCallback } from 'react'

export function useLocalStorage<T>(key: string, initialValue: T) {
  // 从localStorage读取初始值
  const readValue = useCallback((): T => {
    if (typeof window === 'undefined') {
      return initialValue
    }

    try {
      const item = window.localStorage.getItem(key)
      return item ? JSON.parse(item) : initialValue
    } catch (error) {
      console.warn(`Error reading localStorage key "${key}":`, error)
      return initialValue
    }
  }, [initialValue, key])

  const [storedValue, setStoredValue] = useState<T>(readValue)

  // 返回包装过的setValue函数，同时更新localStorage
  const setValue = useCallback((value: T | ((val: T) => T)) => {
    try {
      // 允许值是一个函数，类似于useState
      const valueToStore = value instanceof Function ? value(storedValue) : value

      // 保存state
      setStoredValue(valueToStore)

      // 保存到localStorage
      if (typeof window !== 'undefined') {
        window.localStorage.setItem(key, JSON.stringify(valueToStore))
      }
    } catch (error) {
      console.warn(`Error setting localStorage key "${key}":`, error)
    }
  }, [key, storedValue])

  // 监听storage变化
  useEffect(() => {
    const handleStorageChange = (event: StorageEvent) => {
      if (event.key === key && event.newValue) {
        try {
          const newValue = JSON.parse(event.newValue)
          if (newValue !== storedValue) {
            setStoredValue(newValue)
          }
        } catch (error) {
          console.warn(`Error parsing localStorage key "${key}":`, error)
        }
      }
    }

    window.addEventListener('storage', handleStorageChange)
    return () => window.removeEventListener('storage', handleStorageChange)
  }, [key, storedValue])

  return [storedValue, setValue] as const
}

// 增强版：带过期时间的localStorage
export function useLocalStorageWithExpiry<T>(key: string, initialValue: T, ttl: number) {
  const [value, setValue] = useLocalStorage<{ data: T; expiry: number }>(key, {
    data: initialValue,
    expiry: Date.now() + ttl,
  })

  const setValueWithExpiry = useCallback((newValue: T) => {
    const item = {
      data: newValue,
      expiry: Date.now() + ttl,
    }
    setValue(item)
  }, [ttl])

  // 检查是否过期
  const isExpired = value.expiry < Date.now()

  return [
    isExpired ? initialValue : value.data,
    setValueWithExpiry,
    isExpired,
  ] as const
}

// localStorage工具函数
export const localStorageUtil = {
  get: <T>(key: string, defaultValue: T): T => {
    try {
      const item = localStorage.getItem(key)
      return item ? JSON.parse(item) : defaultValue
    } catch (error) {
      return defaultValue
    }
  },

  set: <T>(key: string, value: T): void => {
    try {
      localStorage.setItem(key, JSON.stringify(value))
    } catch (error) {
      console.warn(`Error setting localStorage key "${key}":`, error)
    }
  },

  remove: (key: string): void => {
    try {
      localStorage.removeItem(key)
    } catch (error) {
      console.warn(`Error removing localStorage key "${key}":`, error)
    }
  },

  clear: (): void => {
    try {
      localStorage.clear()
    } catch (error) {
      console.warn('Error clearing localStorage:', error)
    }
  },

  exists: (key: string): boolean => {
    return localStorage.getItem(key) !== null
  },
}