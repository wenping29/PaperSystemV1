import React, { useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Form, Input, Button, Card, Typography, Space, message } from 'antd'
import { UserOutlined, LockOutlined } from '@ant-design/icons'
import { useAuth } from '@/core/hooks/useAuth'

const { Title, Link: TextLink } = Typography

interface LoginFormValues {
  username: string
  password: string
}


const LoginPage: React.FC = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const { login } = useAuth()
  const [loading, setLoading] = useState(false)

  const from = (location.state as any)?.from?.pathname || '/'

  const onFinish = async (values: LoginFormValues) => {
    setLoading(true)
    try {
      await login(values)
      message.success('登录成功！')
      navigate(from, { replace: true })
    } catch (error) {
      message.error('登录失败，请检查用户名和密码')
      console.error('Login error:', error)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
    }}>
      <Card style={{ width: 400, borderRadius: 12, boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)' }}>
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <Title level={2}>AI写作平台</Title>
          <p style={{ color: '#666' }}>登录您的账户，开始创作之旅</p>
        </div>

        <Form
          name="login"
          onFinish={onFinish}
          layout="vertical"
          size="large"
        >
          <Form.Item
            name="username"
            rules={[
              { required: true, message: '请输入用户名' },
              { min: 3, message: '用户名至少3个字符' }
            ]}
          >
            <Input
              prefix={<UserOutlined />}
              placeholder="用户名或邮箱"
            />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[
              { required: true, message: '请输入密码' },
              { min: 6, message: '密码至少6个字符' }
            ]}
          >
            <Input.Password
              prefix={<LockOutlined />}
              placeholder="密码"
            />
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              loading={loading}
              block
            >
              登录
            </Button>
          </Form.Item>

          <div style={{ textAlign: 'center' }}>
            <Space>
              <TextLink onClick={() => navigate('/register')}>注册新账户</TextLink>
              <span style={{ color: '#ccc' }}>|</span>
              <TextLink>忘记密码？</TextLink>
            </Space>
          </div>
        </Form>

        <div style={{ marginTop: 32, textAlign: 'center', color: '#999', fontSize: 12 }}>
          <p>© 2023 AI写作平台 保留所有权利</p>
        </div>
      </Card>
    </div>
  )
}

export default LoginPage