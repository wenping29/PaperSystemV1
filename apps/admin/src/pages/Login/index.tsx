import React, { useState } from 'react'
import { Form, Input, Button, Card, Typography, Alert, Spin } from 'antd'
import { UserOutlined, LockOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import { authApi } from '@/core/api'
import { showError } from '@/core/utils/error-handler'
import { ROUTES } from '@/core/constants/routes'
import type { LoginRequest } from '@/core/types/auth.types'
import styles from './Login.module.css'

const { Title } = Typography

const Login: React.FC = () => {
  const [form] = Form.useForm()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const navigate = useNavigate()
  const { login, isAuthenticated } = useAuthStore()

  // 如果已经登录，跳转到仪表板
  React.useEffect(() => {
    if (isAuthenticated) {
      navigate(ROUTES.DASHBOARD)
    }
  }, [isAuthenticated, navigate])

  const handleSubmit = async (values: LoginRequest) => {
    try {
      setIsSubmitting(true)
      console.log('登录响应数据：', values)
      const response = await authApi.login(values)
      console.log('登录响应数据：222', response)
      if (response.data) {
        const { token, refreshToken, expiresAt, user } = response.data;
        console.log('登录响应数据：', response.data)

        // 检查用户角色是否为管理员
        if (user.role.toLowerCase() !== 'admin' && user.role.toLowerCase() !== 'superadmin') {
          showError('非管理员用户，无法登录管理后台')
          return
        }
        console.log('登录成功，用户信息：', user)


        login(token, refreshToken, expiresAt, user)
        navigate(ROUTES.DASHBOARD)
      }
    } catch (error) {
      showError(error)
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isAuthenticated) {
    return (
      <div className={styles.loadingContainer}>
        <Spin size="large" tip="正在跳转..." />
      </div>
    )
  }

  return (
    <div className={styles.container}>
      <Card className={styles.loginCard}>
        <div className={styles.logoSection}>
          <Title level={3} className={styles.title}>
            PaperSystem 管理后台
          </Title>
          <p className={styles.subtitle}>请使用管理员账号登录</p>
        </div>

        <Form
          form={form}
          onFinish={handleSubmit}
          size="large"
          autoComplete="off"
        >
          <Form.Item
            name="username"
            rules={[
              { required: true, message: '请输入用户名' },
              { min: 4, message: '用户名至少4个字符' },
            ]}
          >
            <Input
              prefix={<UserOutlined />}
              placeholder="请输入用户名"
              autoComplete="username"
            />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[
              { required: true, message: '请输入密码' },
              { min: 6, message: '密码至少6个字符' },
            ]}
          >
            <Input.Password
              prefix={<LockOutlined />}
              placeholder="请输入密码"
              autoComplete="current-password"
            />
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              block
              loading={isSubmitting}
              size="large"
            >
              {isSubmitting ? '登录中...' : '登录'}
            </Button>
          </Form.Item>
        </Form>

        <Alert
          message="提示"
          description="请使用在后端系统中配置的管理员账号登录"
          type="info"
          showIcon
        />
      </Card>
    </div>
  )
}

export default Login
