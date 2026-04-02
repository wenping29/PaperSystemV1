import React, { useState } from 'react'
import {
  Card,
  Form,
  Input,
  Switch,
  InputNumber,
  Button,
  Row,
  Col,
  Divider,
  message,
  Typography,
  Alert,
} from 'antd'
import { SaveOutlined, SettingOutlined } from '@ant-design/icons'
import type { SystemSettings } from '@/core/types/entities'

const { Title, Text } = Typography
const { TextArea } = Input

const Settings: React.FC = () => {
  const [form] = Form.useForm()
  const [loading, setLoading] = useState(false)

  // 模拟系统设置数据
  const [settings, setSettings] = useState<SystemSettings>({
    siteName: 'PaperSystem',
    siteDescription: 'AI 写作平台',
    enableRegistration: true,
    enableComments: true,
    enableAiFeatures: true,
    maxUploadSize: 10,
    allowedFileTypes: ['jpg', 'jpeg', 'png', 'gif', 'pdf', 'doc', 'docx'],
    maintenanceMode: false,
  })

  React.useEffect(() => {
    form.setFieldsValue(settings)
  }, [form, settings])

  const handleSave = async (values: any) => {
    try {
      setLoading(true)
      // 模拟保存设置
      await new Promise((resolve) => setTimeout(resolve, 800))
      setSettings({ ...settings, ...values })
      message.success('设置保存成功')
    } catch (error) {
      message.error('保存失败，请重试')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <Card>
        <Title level={4}>
          <SettingOutlined style={{ marginRight: 8 }} />
          系统设置
        </Title>
      </Card>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} lg={16}>
          <Card title="基本设置">
            <Form form={form} layout="vertical" onFinish={handleSave}>
              <Form.Item name="siteName" label="网站名称">
                <Input placeholder="请输入网站名称" />
              </Form.Item>

              <Form.Item name="siteDescription" label="网站描述">
                <TextArea rows={3} placeholder="请输入网站描述" />
              </Form.Item>

              <Form.Item name="siteLogo" label="网站 Logo">
                <Input placeholder="Logo URL" />
              </Form.Item>

              <Divider />

              <Title level={5}>功能开关</Title>

              <Form.Item name="enableRegistration" label="开放注册" valuePropName="checked">
                <Switch />
              </Form.Item>

              <Form.Item name="enableComments" label="启用评论" valuePropName="checked">
                <Switch />
              </Form.Item>

              <Form.Item name="enableAiFeatures" label="启用 AI 功能" valuePropName="checked">
                <Switch />
              </Form.Item>

              <Divider />

              <Title level={5}>上传设置</Title>

              <Form.Item name="maxUploadSize" label="最大上传大小 (MB)">
                <InputNumber min={1} max={100} style={{ width: '100%' }} />
              </Form.Item>

              <Form.Item name="allowedFileTypes" label="允许的文件类型">
                <TextArea rows={2} placeholder="多个类型用逗号分隔" />
              </Form.Item>

              <Divider />

              <Title level={5}>维护模式</Title>

              <Form.Item name="maintenanceMode" label="维护模式" valuePropName="checked">
                <Switch />
              </Form.Item>

              <Form.Item name="maintenanceMessage" label="维护提示信息">
                <TextArea rows={3} placeholder="请输入维护期间的提示信息" />
              </Form.Item>

              <Form.Item>
                <Button type="primary" htmlType="submit" icon={<SaveOutlined />} loading={loading}>
                  保存设置
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card title="操作提示">
            <Alert
              message="提示"
              description="修改系统设置后，部分设置可能需要重启服务才能生效。"
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            <div>
              <Text strong>最近修改:</Text>
              <div style={{ marginTop: 8, color: '#666', fontSize: 13 }}>
                暂无修改记录
              </div>
            </div>
          </Card>
        </Col>
      </Row>
    </div>
  )
}

export default Settings
