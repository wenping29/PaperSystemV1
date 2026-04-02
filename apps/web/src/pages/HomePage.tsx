import React from 'react'
import { Card, Row, Col, Statistic, Button, Typography, Space } from 'antd'
import {
  EditOutlined,
  TeamOutlined,
  RiseOutlined,
  FileTextOutlined,
  ArrowRightOutlined
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'

const { Title, Paragraph } = Typography

const HomePage: React.FC = () => {
  const navigate = useNavigate()

  const stats = [
    { title: '今日创作', value: 12, icon: <EditOutlined />, color: '#1890ff' },
    { title: '社区互动', value: 156, icon: <TeamOutlined />, color: '#52c41a' },
    { title: '作品增长', value: '23%', icon: <RiseOutlined />, color: '#faad14' },
    { title: '累计作品', value: 189, icon: <FileTextOutlined />, color: '#722ed1' }
  ]

  const quickActions = [
    { title: '开始写作', description: '使用AI助手创作新作品', path: '/writing', icon: '✍️' },
    { title: '浏览社区', description: '发现优秀作品与作者', path: '/community', icon: '👥' },
    { title: '个人中心', description: '管理作品与账户设置', path: '/profile', icon: '👤' }
  ]

  return (
    <div>
      <div style={{ marginBottom: 32 }}>
        <Title level={2}>欢迎回来！</Title>
        <Paragraph type="secondary">
          今日推荐：尝试使用AI写作助手生成一篇关于科技与人文的文章。
        </Paragraph>
      </div>

      {/* 数据统计 */}
      <Row gutter={[16, 16]} style={{ marginBottom: 32 }}>
        {stats.map((stat, index) => (
          <Col xs={24} sm={12} md={6} key={index}>
            <Card>
              <Statistic
                title={stat.title}
                value={stat.value}
                prefix={stat.icon}
                valueStyle={{ color: stat.color }}
              />
            </Card>
          </Col>
        ))}
      </Row>

      {/* 快速操作 */}
      <Title level={3}>快速开始</Title>
      <Row gutter={[16, 16]} style={{ marginBottom: 32 }}>
        {quickActions.map((action, index) => (
          <Col xs={24} sm={12} md={8} key={index}>
            <Card
              hoverable
              onClick={() => navigate(action.path)}
              style={{ height: '100%' }}
            >
              <Space direction="vertical" size="middle" style={{ width: '100%' }}>
                <div style={{ fontSize: 32 }}>{action.icon}</div>
                <div>
                  <Title level={4} style={{ margin: 0 }}>{action.title}</Title>
                  <Paragraph type="secondary" style={{ margin: 0 }}>
                    {action.description}
                  </Paragraph>
                </div>
                <Button type="link" icon={<ArrowRightOutlined />}>
                  立即前往
                </Button>
              </Space>
            </Card>
          </Col>
        ))}
      </Row>

      {/* 最近作品 */}
      <Card
        title="最近作品"
        extra={<Button type="link">查看全部</Button>}
      >
        <Paragraph type="secondary">
          暂无最近作品，<Button type="link" onClick={() => navigate('/writing')}>开始创作</Button>你的第一篇作品吧！
        </Paragraph>
      </Card>
    </div>
  )
}

export default HomePage