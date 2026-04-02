class WebSocketClient {
  private ws: WebSocket | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectDelay = 1000
  private messageHandlers: Map<string, Function[]> = new Map()
  private connectionPromise: Promise<void> | null = null

  constructor(private url: string) {}

  // 建立连接
  async connect(): Promise<void> {
    if (this.connectionPromise) {
      return this.connectionPromise
    }

    this.connectionPromise = new Promise((resolve, reject) => {
      try {
        const token = localStorage.getItem('token')
        const wsUrl = `${this.url}?token=${encodeURIComponent(token || '')}`

        this.ws = new WebSocket(wsUrl)

        this.ws.onopen = () => {
          console.log('WebSocket连接已建立')
          this.reconnectAttempts = 0
          resolve()
        }

        this.ws.onclose = (event) => {
          console.log('WebSocket连接关闭:', event.code, event.reason)
          this.ws = null
          this.connectionPromise = null

          // 自动重连
          if (this.reconnectAttempts < this.maxReconnectAttempts) {
            setTimeout(() => {
              this.reconnectAttempts++
              console.log(`尝试重连... (${this.reconnectAttempts}/${this.maxReconnectAttempts})`)
              this.connect()
            }, this.reconnectDelay * this.reconnectAttempts)
          }
        }

        this.ws.onerror = (error) => {
          console.error('WebSocket错误:', error)
          reject(error)
        }

        this.ws.onmessage = (event) => {
          try {
            const message = JSON.parse(event.data)
            this.handleMessage(message)
          } catch (error) {
            console.error('消息解析失败:', error)
          }
        }
      } catch (error) {
        reject(error)
      }
    })

    return this.connectionPromise
  }

  // 发送消息
  send(type: string, data?: any): boolean {
    if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
      console.warn('WebSocket未连接，无法发送消息')
      return false
    }

    try {
      const message = JSON.stringify({ type, data, timestamp: Date.now() })
      this.ws.send(message)
      return true
    } catch (error) {
      console.error('发送消息失败:', error)
      return false
    }
  }

  // 订阅消息
  subscribe(type: string, handler: Function): () => void {
    if (!this.messageHandlers.has(type)) {
      this.messageHandlers.set(type, [])
    }

    const handlers = this.messageHandlers.get(type)!
    handlers.push(handler)

    // 返回取消订阅函数
    return () => {
      const index = handlers.indexOf(handler)
      if (index > -1) {
        handlers.splice(index, 1)
      }
    }
  }

  // 处理消息
  private handleMessage(message: any) {
    const { type, data } = message
    const handlers = this.messageHandlers.get(type)

    if (handlers) {
      handlers.forEach(handler => {
        try {
          handler(data)
        } catch (error) {
          console.error(`消息处理器错误 (${type}):`, error)
        }
      })
    }
  }

  // 断开连接
  disconnect(): void {
    if (this.ws) {
      this.ws.close(1000, '正常关闭')
      this.ws = null
    }
    this.connectionPromise = null
    this.messageHandlers.clear()
  }

  // 获取连接状态
  get isConnected(): boolean {
    return this.ws?.readyState === WebSocket.OPEN
  }
}

// 全局WebSocket实例
export const chatWebSocket = new WebSocketClient(import.meta.env.VITE_WS_URL || 'ws://localhost:5000/ws/chat')
export const notificationWebSocket = new WebSocketClient(import.meta.env.VITE_WS_URL || 'ws://localhost:5000/ws/notifications')