import React, { useEffect, useState } from 'react'
import {
  Table,
  Button,
  Space,
  Modal,
  Form,
  Input,
  Select,
  Tag,
  Card,
  Row,
  Col,
  Statistic,
  message,
  Popconfirm,
  Tooltip,
  Avatar,
} from 'antd'
import {
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  EyeOutlined,
  CrownOutlined,
  UserOutlined,
  SafetyOutlined,
} from '@ant-design/icons'
import { userApi } from '@/core/api'
import { showError } from '@/core/utils/error-handler'
import { formatDate } from '@/core/utils/formatters'
import type { User, UserRole, UserStatus } from '@/core/types/entities'
import type { CreateUserRequest, UpdateUserRoleRequest } from '@/core/types/auth.types'
import { ROUTES } from '@/core/constants/routes'
import { useNavigate } from 'react-router-dom'

const { Option } = Select

const AdminUsers: React.FC = () => {
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(false)
  const [pagination, setPagination] = useState({ current: 1, pageSize: 20, total: 0 })
  const [searchParams, setSearchParams] = useState<any>({})
  const [selectedUser, setSelectedUser] = useState<User | null>(null)
  const [modalVisible, setModalVisible] = useState(false)
  const [modalType, setModalType] = useState<'view' | 'create' | 'role'>('view')
  const [form] = Form.useForm()
  const navigate = useNavigate()

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

      const allUsers = usersResponse.data || []
      // 筛选管理员用户
      const adminUsers = allUsers.filter(
        (u: User) => u.role.toLowerCase() === 'admin' || u.role.toLowerCase() === 'superadmin'
      )

      setUsers(Array.isArray(adminUsers) ? adminUsers : [])
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

    setSearchParams(params)
    setPagination((prev) => ({ ...prev, current: 1 }))
  }

  const handleViewUser = (user: User) => {
    setSelectedUser(user)
    setModalType('view')
    setModalVisible(true)
  }

  const handleCreateAdmin = () => {
    setSelectedUser(null)
    setModalType('create')
    form.resetFields()
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

  const handleDeleteAdmin = async (id: number) => {
    try {
      await userApi.deleteUser(id)
      message.success('管理员删除成功')
      fetchUsers()
    } catch (error) {
      showError(error)
    }
  }

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields()

      if (modalType === 'create') {
        const createData: CreateUserRequest = {
          username: values.username,
          email: values.email,
          password: values.password,
        }
        const newUser = await userApi.createUser(createData)
        // 赋予管理员角色
        if (newUser.data?.id) {
          await userApi.updateUserRole(newUser.data.id, {
            role: values.role || 'admin',
            reason: '创建管理员账户',
          })
        }
        message.success('管理员创建成功')
      } else if (modalType === 'role' && selectedUser) {
        const roleData: UpdateUserRoleRequest = {
          role: values.role,
          reason: values.reason,
        }
        await userApi.updateUserRole(selectedUser.id, roleData)
        message.success('管理员角色更新成功')
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
    switch (role.toLocaleLowerCase()) {
      case 'superadmin':
        return 'red'
      case 'admin':
        return 'orange'
      default:
        return 'blue'
    }
  }

  const getRoleText = (role: UserRole) => {
    switch (role.toLocaleLowerCase()) {
      case 'superadmin':
        return '超级管理员'
      case 'admin':
        return '管理员'
      default:
        return role.toUpperCase()
    }
  }

  const columns = [
    {
      title: '管理员',
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
          <CrownOutlined />
          {getRoleText(role)}
        </Tag>
      ),
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      render: (status: UserStatus) => (
        <Tag color={status === 'active' ? 'green' : 'red'}>
          {status === 'active' ? '正常' : '禁用'}
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
      title: '创建时间',
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
          <Tooltip title="修改角色">
            <Button type="link" icon={<CrownOutlined />} onClick={() => handleUpdateRole(record)} />
          </Tooltip>
          {record.role !== 'superadmin' && (
            <Popconfirm
              title="确定要删除这个管理员账户吗？"
              onConfirm={() => handleDeleteAdmin(record.id)}
              okText="确定"
              cancelText="取消"
            >
              <Tooltip title="删除">
                <Button type="link" danger icon={<DeleteOutlined />} />
              </Tooltip>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ]

  const modalTitles = {
    view: '管理员详情',
    create: '创建管理员',
    role: '修改角色',
  }

  // 统计管理员数量
  const adminCount = users.filter((u) => u.role.toLowerCase() === 'admin').length
  const superAdminCount = users.filter((u) => u.role.toLowerCase() === 'superadmin').length

  return (
    <div>
      {/* 统计卡片 */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="管理员总数"
              value={adminCount + superAdminCount}
              prefix={<SafetyOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="超级管理员"
              value={superAdminCount}
              prefix={<CrownOutlined />}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="普通管理员"
              value={adminCount}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Button type="default" onClick={() => navigate(ROUTES.USERS)}>
              返回用户管理
            </Button>
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
              <Option value="admin">管理员</Option>
              <Option value="superadmin">超级管理员</Option>
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

      {/* 管理员表格 */}
      <Card>
        <div style={{ marginBottom: 16 }}>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleCreateAdmin}>
            添加管理员
          </Button>
        </div>
        <Table
          columns={columns}
          dataSource={users}
          rowKey="id"
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
                  <strong>角色:</strong>{' '}
                  <Tag color={getRoleColor(selectedUser.role)}>
                    {getRoleText(selectedUser.role)}
                  </Tag>
                </p>
                <p>
                  <strong>状态:</strong>{' '}
                  <Tag color={selectedUser.status === 'active' ? 'green' : 'red'}>
                    {selectedUser.status === 'active' ? '正常' : '禁用'}
                  </Tag>
                </p>
                <p>
                  <strong>最后登录:</strong>{' '}
                  {selectedUser.lastLoginAt ? formatDate(selectedUser.lastLoginAt) : '从未登录'}
                </p>
                <p>
                  <strong>创建时间:</strong> {formatDate(selectedUser.createdAt)}
                </p>
              </Col>
            </Row>
          </div>
        )}

        {modalType === 'create' && (
          <Form form={form} layout="vertical">
            <Form.Item
              name="username"
              label="用户名"
              rules={[
                { required: true, message: '请输入用户名' },
                { min: 4, message: '用户名至少4个字符' },
              ]}
            >
              <Input placeholder="请输入用户名" />
            </Form.Item>
            <Form.Item
              name="email"
              label="邮箱"
              rules={[
                { required: true, message: '请输入邮箱' },
                { type: 'email', message: '请输入有效的邮箱地址' },
              ]}
            >
              <Input placeholder="请输入邮箱" />
            </Form.Item>
            <Form.Item
              name="password"
              label="密码"
              rules={[
                { required: true, message: '请输入密码' },
                { min: 6, message: '密码至少6个字符' },
              ]}
            >
              <Input.Password placeholder="请输入密码" />
            </Form.Item>
            <Form.Item name="role" label="角色" initialValue="admin">
              <Select>
                <Option value="admin">管理员</Option>
                <Option value="superadmin">超级管理员</Option>
              </Select>
            </Form.Item>
          </Form>
        )}

        {modalType === 'role' && selectedUser && (
          <Form form={form} layout="vertical">
            <Form.Item label="当前用户">
              <Input value={selectedUser.username} disabled />
            </Form.Item>
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

export default AdminUsers
