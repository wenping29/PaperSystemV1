import React, { useState } from 'react'
import { Card, Row, Col, Avatar, Button, Typography, Space, Tabs, Form, Input, Upload, message } from 'antd'
import { UserOutlined, EditOutlined, SaveOutlined, MailOutlined, PhoneOutlined, CameraOutlined } from '@ant-design/icons'

const { Title, Text, Paragraph, Tag } = Typography
const { TabPane } = Tabs
const { TextArea } = Input

interface UserProfile {
  username: string
  email: string
  phone: string
  bio: string
  location: string
  website: string
  avatar: string
  joinDate: string
  level: number
  postsCount: number
  likesCount: number
  followersCount: number
  followingCount: number
}

const ProfilePage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('profile')
  const [editing, setEditing] = useState(false)
  const [loading, setLoading] = useState(false)

  const [profile, setProfile] = useState<UserProfile>({
    username: '张三',
    email: 'zhangsan@example.com',
    phone: '13800138000',
    bio: '热爱写作的程序员，喜欢分享技术经验和生活感悟。',
    location: '北京',
    website: 'https://zhangsan.dev',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=张三',
    joinDate: '2023-01-15',
    level: 5,
    postsCount: 42,
    likesCount: 1245,
    followersCount: 234,
    followingCount: 156
  })

  const handleSaveProfile = async (values: any) => {
    setLoading(true)
    try {
      // 模拟保存API调用
      await new Promise(resolve => setTimeout(resolve, 1000))
      setProfile({ ...profile, ...values })
      setEditing(false)
      message.success('个人资料更新成功！')
    } catch (error) {
      message.error('更新失败，请稍后重试')
      console.error('Update error:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleAvatarChange = (info: any) => {
    if (info.file.status === 'done') {
      message.success('头像上传成功')
      setProfile({ ...profile, avatar: info.file.response.url })
    }
  }

  const stats = [
    { label: '作品数', value: profile.postsCount, color: '#1890ff' },
    { label: '获赞数', value: profile.likesCount, color: '#52c41a' },
    { label: '粉丝数', value: profile.followersCount, color: '#faad14' },
    { label: '关注数', value: profile.followingCount, color: '#722ed1' }
  ]

  return (
    <div>
      <Card style={{ marginBottom: 24 }}>
        <Row gutter={[24, 24]} align="middle">
          <Col xs={24} sm={8} style={{ textAlign: 'center' }}>
            <Space direction="vertical">
              <Avatar
                size={120}
                src={profile.avatar}
                icon={<UserOutlined />}
              />
              <Upload
                showUploadList={false}
                action="/api/upload/avatar"
                onChange={handleAvatarChange}
              >
                <Button icon={<CameraOutlined />}>更换头像</Button>
              </Upload>
              <Title level={3}>{profile.username}</Title>
              <Tag color="blue">Lv.{profile.level}</Tag>
            </Space>
          </Col>
          <Col xs={24} sm={16}>
            <Row gutter={[16, 16]}>
              {stats.map((stat, index) => (
                <Col xs={12} sm={6} key={index}>
                  <Card bordered={false} style={{ textAlign: 'center' }}>
                    <Title level={2} style={{ color: stat.color, margin: 0 }}>
                      {stat.value}
                    </Title>
                    <Text type="secondary">{stat.label}</Text>
                  </Card>
                </Col>
              ))}
            </Row>
            <div style={{ marginTop: 24 }}>
              <Paragraph>
                <Text strong>个人简介：</Text>
                {profile.bio}
              </Paragraph>
              <Space wrap>
                <Text><MailOutlined /> {profile.email}</Text>
                <Text><PhoneOutlined /> {profile.phone}</Text>
                <Text>📍 {profile.location}</Text>
                <Text>🌐 {profile.website}</Text>
                <Text>加入时间：{profile.joinDate}</Text>
              </Space>
            </div>
          </Col>
        </Row>
      </Card>

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab="个人资料" key="profile">
          <Card
            title="编辑个人资料"
            extra={
              editing ? (
                <Space>
                  <Button onClick={() => setEditing(false)}>取消</Button>
                  <Button
                    type="primary"
                    icon={<SaveOutlined />}
                    loading={loading}
                    form="profileForm"
                    htmlType="submit"
                  >
                    保存
                  </Button>
                </Space>
              ) : (
                <Button icon={<EditOutlined />} onClick={() => setEditing(true)}>
                  编辑资料
                </Button>
              )
            }
          >
            <Form
              id="profileForm"
              layout="vertical"
              initialValues={profile}
              onFinish={handleSaveProfile}
              disabled={!editing}
            >
              <Row gutter={[24, 16]}>
                <Col span={12}>
                  <Form.Item label="用户名" name="username" rules={[{ required: true }]}>
                    <Input prefix={<UserOutlined />} />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item label="邮箱" name="email" rules={[{ type: 'email', required: true }]}>
                    <Input prefix={<MailOutlined />} />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item label="手机号" name="phone">
                    <Input prefix={<PhoneOutlined />} />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item label="所在地" name="location">
                    <Input />
                  </Form.Item>
                </Col>
                <Col span={24}>
                  <Form.Item label="个人简介" name="bio">
                    <TextArea rows={3} maxLength={200} showCount />
                  </Form.Item>
                </Col>
                <Col span={24}>
                  <Form.Item label="个人网站" name="website">
                    <Input />
                  </Form.Item>
                </Col>
              </Row>
            </Form>
          </Card>
        </TabPane>
        <TabPane tab="我的作品" key="works">
          <Card>
            <Paragraph>暂无作品，<Button type="link" href="/writing">开始创作</Button>您的第一篇作品吧！</Paragraph>
          </Card>
        </TabPane>
        <TabPane tab="账户设置" key="settings">
          <Card title="安全设置">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Button>修改密码</Button>
              <Button>绑定第三方账号</Button>
              <Button type="dashed" danger>注销账户</Button>
            </Space>
          </Card>
        </TabPane>
      </Tabs>
    </div>
  )
}

export default ProfilePage