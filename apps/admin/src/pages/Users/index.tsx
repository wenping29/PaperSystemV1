import React, { useEffect, useState } from 'react'
import {
  Table,
  Button,
  Space,
  Modal,
  Form,
  Input,
  Select,
  DatePicker,
  Tag,
  Card,
  Row,
  Col,
  Statistic,
  message,
  Popconfirm,
  Avatar,
  Tooltip,
} from 'antd'
import {
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  EyeOutlined,
  LockOutlined,
  UnlockOutlined,
  CrownOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { userApi } from '@/core/api'
import { showError } from '@/core/utils/error-handler'
import { formatDate } from '@/core/utils/formatters'
import dayjs from 'dayjs'
import type { User, UserRole, UserStatus } from '@/core/types/entities'
import type { UpdateUserRequest, UpdateUserRoleRequest } from '@/core/types/auth.types'

const { RangePicker } = DatePicker
const { Option } = Select

const Users: React.FC = () => {
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(false)
  const [pagination, setPagination] = useState({ current: 1, pageSize: 20, total: 0 })
  const [searchParams, setSearchParams] = useState<any>({})
  const [selectedUser, setSelectedUser] = useState<User | null>(null)
  const [modalVisible, setModalVisible] = useState(false)
  const [modalType, setModalType] = useState<'view' | 'edit' | 'ban' | 'role'>('view')
  const [form] = Form.useForm()

  useEffect(() => {
    fetchUsers()
  }, [pagination.current, pagination.pageSize, searchParams])

  const fetchUsers = async () => {
    try {
      setLoading(true)
      const params = {
        page: pagination.current,
        pageSize: pagination.pageSize,
        ...searchParams,
      }

      const [usersResponse, countResponse] = await Promise.all([
        userApi.getUsers(params),
        userApi.getUsersCount(searchParams.search),
      ])

      setUsers(usersResponse.data || [])
      setPagination((prev) => ({ ...prev, total: countResponse.data?.count || 0 }))
    } catch (error) {
      showError(error)
    } finally {
      setLoading(false)
    }
  }

  const handleTableChange = (newPagination: any) => {
    setPagination(newPagination)
  }

  const handleSearch = (values: any) => {
    const params: any = {}
    if (values.username) params.search = values.username
    if (values.role) params.role = values.role
    if (values.status) params.status = values.status

    setSearchParams(params)
    setPagination((prev) => ({ ...prev, current: 1 }))
  }

  const handleViewUser = (user: User) => {
    setSelectedUser(user)
    setModalType('view')
    setModalVisible(true)
  }

  const handleEditUser = (user: User) => {
    setSelectedUser(user)
    setModalType('edit')
    form.setFieldsValue({
      email: user.email,
      phone: user.phone,
      bio: user.bio,
      location: user.location,
      website: user.website,
    })
    setModalVisible(true)
  }

  const handleUpdateRole = (user: User) => {
    setSelectedUser(user)
    setModalType('role')
    form.setFieldsValue({
      role: user.role,
      reason: '',
    })
    setModalVisible(true)
  }

  const handleDeleteUser = async (userId: number) => {
    try {
      await userApi.deleteUser(userId)
      message.success('用户删除成功')
      fetchUsers()
    } catch (error) {
      showError(error)
    }
  }

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields()

      if (modalType === 'edit' && selectedUser) {
        const updateData: UpdateUserRequest = {
          email: values.email,
          phone: values.phone,
          bio: values.bio,
          location: values.location,
          website: values.website,
        }
        await userApi.updateUser(selectedUser.id, updateData)
        message.success('用户信息更新成功')
      } else if (modalType === 'role' && selectedUser) {
        const roleData: UpdateUserRoleRequest = {
          role: values.role,
          reason: values.reason,
        }
        await userApi.updateUserRole(selectedUser.id, roleData)
        message.success('用户角色更新成功')
      }

      setModalVisible(false)
      fetchUsers()
    } catch (error) {
      showError(error)
    }
  }

  const handleModalCancel = () => {
    setModalVisible(false)
    setSelectedUser(null)
    form.resetFields()
  }

  const getRoleColor = (role: UserRole) => {
    switch (role) {
      case 'superadmin':
        return 'red'
      case 'admin':
        return 'orange'
      case 'vip':
        return 'gold'
      default:
        return 'blue'
    }
  }

  const getStatusColor = (status: UserStatus) => {
    switch (status) {
      case 'active':
        return 'green'
      case 'inactive':
        return 'default'
      case 'banned':
      case 'suspended':
        return 'red'
      default:
        return 'default'
    }
  }

  const getStatusText = (status: UserStatus) => {
    const statusMap: Record<UserStatus, string> = {
      active: '活跃',
      inactive: '未激活',
      banned: '已封禁',
      suspended: '已暂停',
    }
    return statusMap[status] || status
  }

  const columns = [
    {
      title: '用户',
      dataIndex: 'username',
      key: 'username',
      render: (text: string, record: User) => (
        <Space>
          <Avatar src={record.avatarUrl} icon={<UserOutlined />} />
          <div>
            <div>{text}</div>
            <div style={{ fontSize: 12, color: '#999' }}>{record.email}</div>
          </div>
        </Space>
      ),
    },
    {
      title: '角色',
      dataIndex: 'role',
      key: 'role',
      render: (role: UserRole) => (
        <Tag color={getRoleColor(role)}>
          {role === 'superadmin' || role === 'admin' ? <CrownOutlined /> : null}
          {role === 'superadmin' ? '超级管理员' : role === 'admin' ? '管理员' : role.toUpperCase()}
        </Tag>
      ),
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      render: (status: UserStatus) => (
        <Tag color={getStatusColor(status)}>
          {status === 'banned' || status === 'suspended' ? <LockOutlined /> : null}
          {getStatusText(status)}
        </Tag>
      ),
    },
    {
      title: '最后登录',
      dataIndex: 'lastLoginAt',
      key: 'lastLoginAt',
      render: (date: string) => (date ? formatDate(date, 'YYYY-MM-DD HH:mm') : '从未登录'),
    },
    {
      title: '注册时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => formatDate(date, 'YYYY-MM-DD'),
    },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: User) => (
        <Space size="small">
          <Tooltip title="查看详情">
            <Button type="link" icon={<EyeOutlined />} onClick={() => handleViewUser(record)} />
          </Tooltip>
          <Tooltip title="编辑">
            <Button type="link" icon={<EditOutlined />} onClick={() => handleEditUser(record)} />
          </Tooltip>
          <Tooltip title="修改角色">
            <Button type="link" icon={<CrownOutlined />} onClick={() => handleUpdateRole(record)} />
          </Tooltip>
          <Popconfirm
            title="确定要删除这个用户吗？"
            onConfirm={() => handleDeleteUser(record.id)}
            okText="确定"
            cancelText="取消"
          >
            <Tooltip title="删除">
              <Button type="link" danger icon={<DeleteOutlined />} />
            </Tooltip>
          </Popconfirm>
        </Space>
      ),
    },
  ]

  const modalTitles = {
    view: '用户详情',
    edit: '编辑用户',
    ban: '封禁用户',
    role: '修改角色',
  }

  return (
    <div>
      {/* 统计卡片 */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="总用户数"
              value={pagination.total}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="活跃用户"
              value={users.filter((u) => u.status === 'active').length}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="管理员"
              value={users.filter((u) => u.role === 'admin' || u.role === 'superadmin').length}
              prefix={<CrownOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="已封禁"
              value={users.filter((u) => u.status === 'banned' || u.status === 'suspended').length}
              prefix={<LockOutlined />}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Card>
        </Col>
      </Row>

      {/* 搜索表单 */}
      <Card style={{ marginBottom: 16 }}>
        <Form layout="inline" onFinish={handleSearch}>
          <Form.Item name="username" label="用户名">
            <Input placeholder="请输入用户名" allowClear />
          </Form.Item>
          <Form.Item name="role" label="角色">
            <Select placeholder="请选择角色" allowClear style={{ width: 120 }}>
              <Option value="user">普通用户</Option>
              <Option value="vip">VIP用户</Option>
              <Option value="admin">管理员</Option>
              <Option value="superadmin">超级管理员</Option>
            </Select>
          </Form.Item>
          <Form.Item name="status" label="状态">
            <Select placeholder="请选择状态" allowClear style={{ width: 120 }}>
              <Option value="active">活跃</Option>
              <Option value="inactive">未激活</Option>
              <Option value="banned">已封禁</Option>
              <Option value="suspended">已暂停</Option>
            </Select>
          </Form.Item>
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" icon={<SearchOutlined />}>
                搜索
              </Button>
              <Button
                onClick={() => {
                  form.resetFields()
                  setSearchParams({})
                }}
              >
                重置
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>

      {/* 用户表格 */}
      <Card>
        <div style={{ marginBottom: 16 }}>
          <Button type="primary" icon={<PlusOutlined />}>
            添加用户
          </Button>
        </div>
        <Table
          columns={columns}
          dataSource={users}
          rowKey="Id"
          loading={loading}
          pagination={pagination}
          onChange={handleTableChange}
        />
      </Card>

      {/* 模态框 */}
      <Modal
        title={modalTitles[modalType]}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={handleModalCancel}
        width={modalType === 'view' ? 600 : 500}
      >
        {modalType === 'view' && selectedUser && (
          <div>
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Avatar size={80} src={selectedUser.avatarUrl} icon={<UserOutlined />} />
              </Col>
              <Col span={16}>
                <p>
                  <strong>用户名:</strong> {selectedUser.username}
                </p>
                <p>
                  <strong>邮箱:</strong> {selectedUser.email || '未设置'}
                </p>
                <p>
                  <strong>手机:</strong> {selectedUser.phone || '未设置'}
                </p>
                <p>
                  <strong>角色:</strong>{' '}
                  <Tag color={getRoleColor(selectedUser.role)}>{selectedUser.role}</Tag>
                </p>
                <p>
                  <strong>状态:</strong>{' '}
                  <Tag color={getStatusColor(selectedUser.status)}>
                    {getStatusText(selectedUser.status)}
                  </Tag>
                </p>
                <p>
                  <strong>简介:</strong> {selectedUser.bio || '未设置'}
                </p>
                <p>
                  <strong>最后登录:</strong>{' '}
                  {selectedUser.lastLoginAt ? formatDate(selectedUser.lastLoginAt) : '从未登录'}
                </p>
                <p>
                  <strong>注册时间:</strong> {formatDate(selectedUser.createdAt)}
                </p>
              </Col>
            </Row>
          </div>
        )}

        {modalType === 'edit' && selectedUser && (
          <Form form={form} layout="vertical">
            <Form.Item
              name="email"
              label="邮箱"
              rules={[{ type: 'email', message: '请输入有效的邮箱地址' }]}
            >
              <Input />
            </Form.Item>
            <Form.Item name="phone" label="手机">
              <Input />
            </Form.Item>
            <Form.Item name="bio" label="简介">
              <Input.TextArea rows={3} />
            </Form.Item>
            <Form.Item name="location" label="所在地">
              <Input />
            </Form.Item>
            <Form.Item name="website" label="个人网站">
              <Input />
            </Form.Item>
          </Form>
        )}

        {modalType === 'role' && selectedUser && (
          <Form form={form} layout="vertical">
            <Form.Item
              name="role"
              label="新角色"
              rules={[{ required: true, message: '请选择角色' }]}
            >
              <Select>
                <Option value="user">普通用户</Option>
                <Option value="vip">VIP用户</Option>
                <Option value="admin">管理员</Option>
                <Option value="superadmin">超级管理员</Option>
              </Select>
            </Form.Item>
            <Form.Item name="reason" label="修改原因">
              <Input.TextArea rows={3} placeholder="请输入修改角色的原因..." />
            </Form.Item>
          </Form>
        )}
      </Modal>
    </div>
  )
}

export default Users
