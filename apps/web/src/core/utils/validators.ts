// 验证工具函数

/**
 * 通用验证规则
 */
export const validate = {
  // 必填字段
  required: (value: any): boolean => {
    if (value === undefined || value === null) return false
    if (typeof value === 'string' && value.trim() === '') return false
    if (Array.isArray(value) && value.length === 0) return false
    return true
  },

  // 最小长度
  minLength: (value: string | any[], min: number): boolean => {
    if (!value) return false
    return value.length >= min
  },

  // 最大长度
  maxLength: (value: string | any[], max: number): boolean => {
    if (!value) return false
    return value.length <= max
  },

  // 长度范围
  lengthBetween: (value: string | any[], min: number, max: number): boolean => {
    if (!value) return false
    return value.length >= min && value.length <= max
  },

  // 邮箱格式
  email: (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email)
  },

  // 手机号格式（中国）
  phone: (phone: string): boolean => {
    const phoneRegex = /^1[3-9]\d{9}$/
    return phoneRegex.test(phone)
  },

  // 用户名格式（字母数字下划线，3-20位）
  username: (username: string): boolean => {
    const usernameRegex = /^[a-zA-Z0-9_]{3,20}$/
    return usernameRegex.test(username)
  },

  // 密码格式（至少6位，包含字母和数字）
  password: (password: string): boolean => {
    const passwordRegex = /^(?=.*[A-Za-z])(?=.*\d).{6,}$/
    return passwordRegex.test(password)
  },

  // 强密码格式（至少8位，包含大小写字母和数字）
  strongPassword: (password: string): boolean => {
    const strongPasswordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/
    return strongPasswordRegex.test(password)
  },

  // URL格式
  url: (url: string): boolean => {
    try {
      new URL(url)
      return true
    } catch {
      return false
    }
  },

  // 数字范围
  numberBetween: (value: number, min: number, max: number): boolean => {
    return value >= min && value <= max
  },

  // 整数
  integer: (value: number): boolean => {
    return Number.isInteger(value)
  },

  // 正数
  positive: (value: number): boolean => {
    return value > 0
  },

  // 非负数
  nonNegative: (value: number): boolean => {
    return value >= 0
  },

  // 身份证号码（中国）
  idCard: (idCard: string): boolean => {
    const idCardRegex = /(^\d{15}$)|(^\d{17}([0-9]|X|x)$)/
    return idCardRegex.test(idCard)
  },

  // 中文姓名（2-4个汉字）
  chineseName: (name: string): boolean => {
    const chineseNameRegex = /^[\u4e00-\u9fa5]{2,4}$/
    return chineseNameRegex.test(name)
  },

  // 邮政编码（中国）
  postalCode: (code: string): boolean => {
    const postalCodeRegex = /^\d{6}$/
    return postalCodeRegex.test(code)
  },

  // IP地址
  ipAddress: (ip: string): boolean => {
    const ipRegex = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/
    return ipRegex.test(ip)
  },

  // 文件类型
  fileType: (file: File, allowedTypes: string[]): boolean => {
    return allowedTypes.includes(file.type)
  },

  // 文件大小（单位：字节）
  fileSize: (file: File, maxSize: number): boolean => {
    return file.size <= maxSize
  },

  // 图片尺寸
  imageDimensions: (file: File, maxWidth: number, maxHeight: number): Promise<boolean> => {
    return new Promise((resolve) => {
      const img = new Image()
      img.onload = () => {
        resolve(img.width <= maxWidth && img.height <= maxHeight)
      }
      img.onerror = () => resolve(false)
      img.src = URL.createObjectURL(file)
    })
  },
}

/**
 * 表单验证器
 */
export class Validator {
  private errors: Record<string, string[]> = {}

  constructor(private data: Record<string, any>) {}

  // 添加验证规则
  addRule(field: string, rule: (value: any) => boolean | string, message: string): this {
    const value = this.data[field]
    const result = rule(value)

    if (result === false || typeof result === 'string') {
      if (!this.errors[field]) {
        this.errors[field] = []
      }
      this.errors[field].push(typeof result === 'string' ? result : message)
    }

    return this
  }

  // 必填
  required(field: string, message = '该字段为必填项'): this {
    return this.addRule(field, validate.required, message)
  }

  // 邮箱
  email(field: string, message = '请输入有效的邮箱地址'): this {
    return this.addRule(field, validate.email, message)
  }

  // 手机号
  phone(field: string, message = '请输入有效的手机号码'): this {
    return this.addRule(field, validate.phone, message)
  }

  // 最小长度
  minLength(field: string, min: number, message = `长度不能少于${min}个字符`): this {
    return this.addRule(field, (value) => validate.minLength(value, min), message)
  }

  // 最大长度
  maxLength(field: string, max: number, message = `长度不能超过${max}个字符`): this {
    return this.addRule(field, (value) => validate.maxLength(value, max), message)
  }

  // 长度范围
  lengthBetween(field: string, min: number, max: number, message = `长度应在${min}到${max}个字符之间`): this {
    return this.addRule(field, (value) => validate.lengthBetween(value, min, max), message)
  }

  // 用户名
  username(field: string, message = '用户名必须是3-20位的字母、数字或下划线'): this {
    return this.addRule(field, validate.username, message)
  }

  // 密码
  password(field: string, message = '密码至少6位，必须包含字母和数字'): this {
    return this.addRule(field, validate.password, message)
  }

  // 确认密码
  confirmPassword(passwordField: string, confirmField: string, message = '两次输入的密码不一致'): this {
    const password = this.data[passwordField]
    const confirm = this.data[confirmField]

    if (password !== confirm) {
      if (!this.errors[confirmField]) {
        this.errors[confirmField] = []
      }
      this.errors[confirmField].push(message)
    }

    return this
  }

  // 数字范围
  numberBetween(field: string, min: number, max: number, message = `数值应在${min}到${max}之间`): this {
    return this.addRule(field, (value) => validate.numberBetween(Number(value), min, max), message)
  }

  // 自定义规则
  custom(field: string, validator: (value: any, data: Record<string, any>) => boolean | string, message: string): this {
    return this.addRule(field, (value) => validator(value, this.data), message)
  }

  // 获取验证结果
  validate(): { isValid: boolean; errors: Record<string, string[]> } {
    return {
      isValid: Object.keys(this.errors).length === 0,
      errors: this.errors,
    }
  }

  // 获取第一个错误信息
  getFirstError(): string | null {
    for (const field in this.errors) {
      if (this.errors[field].length > 0) {
        return this.errors[field][0]
      }
    }
    return null
  }

  // 清空错误
  clear(): void {
    this.errors = {}
  }
}

/**
 * 表单验证工具
 */
export const formValidate = {
  // 验证登录表单
  loginForm: (data: { username: string; password: string }) => {
    const validator = new Validator(data)
    validator
      .required('username', '请输入用户名')
      .required('password', '请输入密码')

    return validator.validate()
  },

  // 验证注册表单
  registerForm: (data: {
    username: string
    email: string
    password: string
    confirmPassword: string
    agreeTerms: boolean
  }) => {
    const validator = new Validator(data)
    validator
      .required('username', '请输入用户名')
      .username('username', '用户名必须是3-20位的字母、数字或下划线')
      .required('email', '请输入邮箱')
      .email('email', '请输入有效的邮箱地址')
      .required('password', '请输入密码')
      .password('password', '密码至少6位，必须包含字母和数字')
      .confirmPassword('password', 'confirmPassword', '两次输入的密码不一致')
      .addRule('agreeTerms', (value) => value === true, '请同意用户协议')

    return validator.validate()
  },

  // 验证作品表单
  workForm: (data: {
    title: string
    content: string
    category: string
    tags: string[]
  }) => {
    const validator = new Validator(data)
    validator
      .required('title', '请输入作品标题')
      .lengthBetween('title', 1, 100, '标题长度应在1-100个字符之间')
      .required('content', '请输入作品内容')
      .minLength('content', 10, '内容长度不能少于10个字符')
      .required('category', '请选择分类')
      .addRule('tags', (value) => Array.isArray(value) && value.length > 0, '请至少添加一个标签')
      .addRule('tags', (value) => Array.isArray(value) && value.length <= 5, '标签数量不能超过5个')

    return validator.validate()
  },

  // 验证评论表单
  commentForm: (data: { content: string }) => {
    const validator = new Validator(data)
    validator
      .required('content', '请输入评论内容')
      .lengthBetween('content', 1, 500, '评论长度应在1-500个字符之间')

    return validator.validate()
  },

  // 验证支付表单
  paymentForm: (data: { amount: number; paymentMethod: string }) => {
    const validator = new Validator(data)
    validator
      .required('amount', '请输入支付金额')
      .numberBetween('amount', 1, 5000, '支付金额应在1-5000元之间')
      .required('paymentMethod', '请选择支付方式')

    return validator.validate()
  },
}

/**
 * 实时验证工具
 */
export const realtimeValidate = {
  // 实时验证用户名
  username: (username: string): { valid: boolean; message?: string } => {
    if (!username) return { valid: false, message: '用户名不能为空' }
    if (!validate.username(username)) return { valid: false, message: '用户名必须是3-20位的字母、数字或下划线' }
    return { valid: true }
  },

  // 实时验证邮箱
  email: (email: string): { valid: boolean; message?: string } => {
    if (!email) return { valid: false, message: '邮箱不能为空' }
    if (!validate.email(email)) return { valid: false, message: '请输入有效的邮箱地址' }
    return { valid: true }
  },

  // 实时验证密码
  password: (password: string): { valid: boolean; message?: string; strength?: { score: number; label: string; color: string } } => {
    if (!password) return { valid: false, message: '密码不能为空' }

    const strength = validate.password(password)
      ? { score: 3, label: '中', color: '#faad14' }
      : { score: 1, label: '弱', color: '#ff4d4f' }

    if (!validate.password(password)) {
      return {
        valid: false,
        message: '密码至少6位，必须包含字母和数字',
        strength
      }
    }

    return { valid: true, strength }
  },

  // 实时验证手机号
  phone: (phone: string): { valid: boolean; message?: string } => {
    if (!phone) return { valid: false, message: '手机号不能为空' }
    if (!validate.phone(phone)) return { valid: false, message: '请输入有效的手机号码' }
    return { valid: true }
  },
}

/**
 * 文件验证工具
 */
export const fileValidate = {
  // 验证上传文件
  uploadFile: (file: File, options: {
    allowedTypes?: string[]
    maxSize?: number
    maxWidth?: number
    maxHeight?: number
  } = {}): Promise<{ valid: boolean; errors: string[] }> => {
    const errors: string[] = []

    // 文件类型验证
    if (options.allowedTypes && options.allowedTypes.length > 0) {
      if (!validate.fileType(file, options.allowedTypes)) {
        errors.push(`不支持的文件类型，仅支持：${options.allowedTypes.join(', ')}`)
      }
    }

    // 文件大小验证
    if (options.maxSize && !validate.fileSize(file, options.maxSize)) {
      errors.push(`文件大小不能超过${options.maxSize / 1024 / 1024}MB`)
    }

    // 图片尺寸验证
    if (options.maxWidth && options.maxHeight && file.type.startsWith('image/')) {
      return validate.imageDimensions(file, options.maxWidth, options.maxHeight).then((valid) => {
        if (!valid) {
          errors.push(`图片尺寸不能超过${options.maxWidth}x${options.maxHeight}像素`)
        }
        return { valid: errors.length === 0, errors }
      })
    }

    return Promise.resolve({ valid: errors.length === 0, errors })
  },
}