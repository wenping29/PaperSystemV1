import React, { useEffect, useState } from 'react'
import { Card, Row, Col, Statistic, Table, DatePicker, Space, Spin, Alert, Typography, Tag } from 'antd'
import {
  UserOutlined,
  FileTextOutlined,
  RobotOutlined,
  DollarOutlined,
  LikeOutlined,
  CommentOutlined,
  EyeOutlined,
  RiseOutlined,
} from '@ant-design/icons'
import ReactECharts from 'echarts-for-react'
import dayjs from 'dayjs'
import { userApi, workApi } from '@/core/api'
import { showError } from '@/core/utils/error-handler'
import { formatDate } from '@/core/utils/formatters'
import type { User, Work, DashboardStats } from '@/core/types/entities'

const { RangePicker } = DatePicker
const { Title } = Typography

const Dashboard: React.FC = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [users, setUsers] = useState<User[]>([])
  const [totalUsers, setTotalUsers] = useState(0)
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(7, 'days'),
    dayjs(),
  ])

  useEffect(() => {
    fetchDashboardData()
  }, [])

  const fetchDashboardData = async () => {
    try {
      setLoading(true)
      setError(null)

      // 获取用户数据
      const [usersResponse, countResponse] = await Promise.all([
        userApi.getUsers({ page: 1, pageSize: 5 }),
        userApi.getUsersCount(),
      ])

      setUsers(usersResponse.data || [])
      setTotalUsers(countResponse.data?.count || 0)
    } catch (err) {
      console.error('Failed to fetch dashboard data:', err)
      setError(showError(err))
    } finally {
      setLoading(false)
    }
  }

  const handleDateRangeChange = (dates: any) => {
    if (dates) {
      setDateRange(dates)
    }
  }

  // 用户增长趋势图表配置
  const userGrowthOption = {
    title: { text: '用户增长趋势', left: 'center' },
    tooltip: { trigger: 'axis' },
    legend: { data: ['新增用户', '活跃用户'], bottom: 10 },
    xAxis: {
      type: 'category',
      data: Array.from({ length: 7 }, (_, i) =>
        dayjs().subtract(6 - i, 'day').format('MM-DD')
      ),
    },
    yAxis: { type: 'value' },
    series: [
      {
        name: '新增用户',
        type: 'line',
        smooth: true,
        data: [12, 19, 15, 25, 22, 30, 28],
        areaStyle: { opacity: 0.3 },
        itemStyle: { color: '#1890ff' },
      },
      {
        name: '活跃用户',
        type: 'line',
        smooth: true,
        data: [45, 52, 48, 60, 55, 68, 65],
        areaStyle: { opacity: 0.3 },
        itemStyle: { color: '#52c41a' },
      },
    ],
  }

  // 内容统计图表配置
  const contentStatsOption = {
    title: { text: '内容分布', left: 'center' },
    tooltip: { trigger: 'item' },
    legend: { orient: 'vertical', left: 'left' },
    series: [
      {
        name: '内容类型',
        type: 'pie',
        radius: ['40%', '70%'],
        data: [
          { value: 1048, name: '文章', itemStyle: { color: '#1890ff' } },
          { value: 735, name: '评论', itemStyle: { color: '#52c41a' } },
          { value: 580, name: '点赞', itemStyle: { color: '#faad14' } },
          { value: 484, name: '收藏', itemStyle: { color: '#722ed1' } },
        ],
      },
    ],
  }

  // 作品趋势图表配置
  const workTrendOption = {
    title: { text: '作品发布趋势', left: 'center' },
    tooltip: { trigger: 'axis' },
    legend: { data: ['发布作品', 'AI 辅助创作'], bottom: 10 },
    xAxis: {
      type: 'category',
      data: Array.from({ length: 7 }, (_, i) =>
        dayjs().subtract(6 - i, 'day').format('MM-DD')
      ),
    },
    yAxis: { type: 'value' },
    series: [
      {
        name: '发布作品',
        type: 'bar',
        data: [20, 25, 22, 30, 28, 35, 32],
        itemStyle: { color: '#1890ff' },
      },
      {
        name: 'AI 辅助创作',
        type: 'bar',
        data: [15, 18, 16, 22, 20, 26, 24],
        itemStyle: { color: '#722ed1' },
      },
    ],
  }

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: '50px' }}>
        <Spin size="large" tip="加载仪表板数据..." />
      </div>
    )
  }

  if (error) {
    return <Alert message="错误" description={error} type="error" showIcon />
  }

  // 最近活动数据
  const recentColumns = [
    {
      title: '时间',
      dataIndex: 'time',
      key: 'time',
    },
    {
      title: '类型',
      dataIndex: 'type',
      key: 'type',
      render: (type: string) => {
        const colors: Record<string, string> = {
          '用户注册': 'blue',
          '文章发布': 'green',
          'AI使用': 'purple',
          '打赏': 'gold',
          '评论': 'orange',
        }
        return <Tag color={colors[type] || 'default'}>{type}</Tag>
      },
    },
    {
      title: '描述',
      dataIndex: 'description',
      key: 'description',
    },
    {
      title: '用户',
      dataIndex: 'user',
      key: 'user',
    },
  ]

  const recentActivities = [
    { key: '1', time: '10分钟前', type: '用户注册', description: '新用户注册', user: '张三' },
    { key: '2', time: '30分钟前', type: '文章发布', description: '发布新文章', user: '李四' },
    { key: '3', time: '1小时前', type: 'AI使用', description: 'AI 写作建议', user: '王五' },
    { key: '4', time: '2小时前', type: '打赏', description: '文章打赏', user: '赵六' },
    { key: '5', time: '3小时前', type: '评论', description: '文章评论', user: '孙七' },
  ]

  // 热门作品
  const topWorks = [
    { key: '1', title: '如何高效使用 AI 写作', author: '张三', views: 1250, likes: 89, createdAt: '2024-03-28' },
    { key: '2', title: '我的创作之旅', author: '李四', views: 980, likes: 65, createdAt: '2024-03-27' },
    { key: '3', title: '写作技巧分享', author: '王五', views: 756, likes: 48, createdAt: '2024-03-26' },
  ]

  return (
    <div>
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        {/* 过滤器 */}
        <Card>
          <Space>
            <span>时间范围：</span>
            <RangePicker value={dateRange} onChange={handleDateRangeChange} />
          </Space>
        </Card>

        {/* 统计卡片 */}
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="总用户数"
                value={totalUsers}
                prefix={<UserOutlined />}
                valueStyle={{ color: '#3f8600' }}
              />
              <div style={{ marginTop: 8, fontSize: 12, color: '#999' }}>
                <Space>
                  <RiseOutlined />
                  今日新增: 28
                </Space>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="总作品数"
                value={156}
                prefix={<FileTextOutlined />}
                valueStyle={{ color: '#1890ff' }}
              />
              <div style={{ marginTop: 8, fontSize: 12, color: '#999' }}>
                <Space>
                  <RiseOutlined />
                  今日新增: 12
                </Space>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="AI使用次数"
                value={892}
                prefix={<RobotOutlined />}
                valueStyle={{ color: '#722ed1' }}
              />
              <div style={{ marginTop: 8, fontSize: 12, color: '#999' }}>
                <Space>
                  <RiseOutlined />
                  今日新增: 45
                </Space>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="总收入"
                value={12580}
                prefix={<DollarOutlined />}
                valueStyle={{ color: '#52c41a' }}
                precision={2}
              />
              <div style={{ marginTop: 8, fontSize: 12, color: '#999' }}>
                <Space>
                  <RiseOutlined />
                  今日收入: ¥328.00
                </Space>
              </div>
            </Card>
          </Col>
        </Row>

        {/* 第二行统计 */}
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="总评论数"
                value={428}
                prefix={<CommentOutlined />}
                valueStyle={{ color: '#faad14' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="总点赞数"
                value={1856}
                prefix={<LikeOutlined />}
                valueStyle={{ color: '#eb2f96' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="总浏览量"
                value={25680}
                prefix={<EyeOutlined />}
                valueStyle={{ color: '#13c2c2' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="系统状态"
                value="正常"
                prefix={<RiseOutlined />}
                valueStyle={{ color: '#52c41a' }}
              />
            </Card>
          </Col>
        </Row>

        {/* 图表 */}
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <Card>
              <ReactECharts option={userGrowthOption} style={{ height: 300 }} />
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card>
              <ReactECharts option={workTrendOption} style={{ height: 300 }} />
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]}>
          <Col xs={24} lg={8}>
            <Card>
              <ReactECharts option={contentStatsOption} style={{ height: 300 }} />
            </Card>
          </Col>
          <Col xs={24} lg={16}>
            {/* 热门作品 */}
            <Card title="热门作品">
              <Table
                columns={[
                  { title: '标题', dataIndex: 'title', key: 'title' },
                  { title: '作者', dataIndex: 'author', key: 'author' },
                  { title: '浏览', dataIndex: 'views', key: 'views', render: (v: number) => v.toLocaleString() },
                  { title: '点赞', dataIndex: 'likes', key: 'likes' },
                  { title: '发布时间', dataIndex: 'createdAt', key: 'createdAt' },
                ]}
                dataSource={topWorks}
                pagination={false}
                size="small"
              />
            </Card>
          </Col>
        </Row>

        {/* 最近注册用户 */}
        <Card title="最近注册用户">
          <Table
            columns={[
              { title: '用户名', dataIndex: 'username', key: 'username' },
              { title: '邮箱', dataIndex: 'email', key: 'email' },
              { title: '角色', dataIndex: 'role', key: 'role' },
              { title: '状态', dataIndex: 'status', key: 'status' },
              { title: '注册时间', dataIndex: 'createdAt', key: 'createdAt', render: (date: string) => formatDate(date, 'YYYY-MM-DD HH:mm') },
            ]}
            dataSource={users}
            rowKey="id"
            pagination={{ pageSize: 5 }}
            size="small"
          />
        </Card>

        {/* 最近活动 */}
        <Card title="最近活动">
          <Table
            columns={recentColumns}
            dataSource={recentActivities}
            pagination={{ pageSize: 5 }}
            size="small"
          />
        </Card>
      </Space>
    </div>
  )
}

export default Dashboard
