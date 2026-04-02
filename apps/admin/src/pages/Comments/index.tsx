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
} from 'antd'
import {
  SearchOutlined,
  EditOutlined,
  DeleteOutlined,
  EyeOutlined,
  CheckOutlined,
  CloseOutlined,
  LikeOutlined,
  CommentOutlined,
} from '@ant-design/icons'
import { commentApi } from '@/core/api'
import { showError } from '@/core/utils/error-handler'
import { formatDate, truncateText } from '@/core/utils/formatters'
import type { Comment, CommentStatus } from '@/core/types/entities'
import type { UpdateCommentRequest } from '@/core/api/endpoints/commentApi'

const { TextArea } = Input
const { Option } = Select

const Comments: React.FC = () => {
  const [comments, setComments] = useState<any[]>([])
  const [loading, setLoading] = useState(false)
  const [pagination, setPagination] = useState({ current: 1, pageSize: 20, total: 0 })
  const [searchParams, setSearchParams] = useState<any>({})
  const [selectedComment, setSelectedComment] = useState<Comment | null>(null)
  const [modalVisible, setModalVisible] = useState(false)
  const [modalType, setModalType] = useState<'view' | 'edit'>('view')
  const [form] = Form.useForm()

  useEffect(() => {
    fetchComments()
  }, [pagination.current, pagination.pageSize, searchParams])

  const fetchComments = async () => {
    try {
      setLoading(true)
      const params = {
        page: pagination.current,
        pageSize: pagination.pageSize,
        ...searchParams,
      }

      const response = await commentApi.getComments(params)
      const data = response.data?.data || response.data || []

      setComments(Array.isArray(data) ? data : [])
      setPagination((prev) => ({ ...prev, total: data.length * 5 }))
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
    if (values.search) params.search = values.search
    if (values.status) params.status = values.status

    setSearchParams(params)
    setPagination((prev) => ({ ...prev, current: 1 }))
  }

  const handleViewComment = (comment: any) => {
    setSelectedComment(comment)
    setModalType('view')
    setModalVisible(true)
  }

  const handleEditComment = (comment: any) => {
    setSelectedComment(comment)
    setModalType('edit')
    form.setFieldsValue({
      content: comment.content,
      status: comment.status,
    })
    setModalVisible(true)
  }

  const handleDeleteComment = async (id: number) => {
    try {
      await commentApi.deleteComment(id)
      message.success('评论删除成功')
      fetchComments()
    } catch (error) {
      showError(error)
    }
  }

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields()

      if (modalType === 'edit' && selectedComment) {
        const updateData: UpdateCommentRequest = {
          content: values.content,
          status: values.status,
        }
        await commentApi.updateComment(selectedComment.id, updateData)
        message.success('评论更新成功')
      }

      setModalVisible(false)
      fetchComments()
    } catch (error) {
      showError(error)
    }
  }

  const handleModalCancel = () => {
    setModalVisible(false)
    setSelectedComment(null)
    form.resetFields()
  }

  const getStatusColor = (status?: CommentStatus) => {
    switch (status) {
      case 'published':
        return 'green'
      case 'hidden':
        return 'orange'
      case 'deleted':
        return 'red'
      default:
        return 'default'
    }
  }

  const getStatusText = (status?: CommentStatus) => {
    switch (status) {
      case 'published':
        return '已发布'
      case 'hidden':
        return '已隐藏'
      case 'deleted':
        return '已删除'
      default:
        return '已发布'
    }
  }

  const columns = [
    {
      title: '评论内容',
      dataIndex: 'content',
      key: 'content',
      width: 300,
      render: (text: string) => (
        <div>
          <div>{truncateText(text, 60)}</div>
        </div>
      ),
    },
    {
      title: '作者',
      dataIndex: 'authorName',
      key: 'authorName',
      width: 120,
      render: (name: string, record: any) => name || record.authorId || '-',
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status: CommentStatus, record: any) => (
        <Tag color={getStatusColor(status)}>
          {record.isDeleted ? <CloseOutlined /> : <CheckOutlined />}
          {record.isDeleted ? '已删除' : getStatusText(status)}
        </Tag>
      ),
    },
    {
      title: '点赞',
      dataIndex: 'likeCount',
      key: 'likeCount',
      width: 80,
      render: (count: number) => (
        <Space>
          <LikeOutlined />
          {count || 0}
        </Space>
      ),
    },
    {
      title: '评论时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 150,
      render: (date: string) => formatDate(date, 'YYYY-MM-DD HH:mm'),
    },
    {
      title: '操作',
      key: 'action',
      width: 150,
      render: (_: any, record: any) => (
        <Space size="small">
          <Tooltip title="查看详情">
            <Button type="link" icon={<EyeOutlined />} onClick={() => handleViewComment(record)} />
          </Tooltip>
          <Tooltip title="编辑">
            <Button type="link" icon={<EditOutlined />} onClick={() => handleEditComment(record)} />
          </Tooltip>
          <Popconfirm
            title="确定要删除这条评论吗？"
            onConfirm={() => handleDeleteComment(record.id)}
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
    view: '评论详情',
    edit: '编辑评论',
  }

  return (
    <div>
      {/* 统计卡片 */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="总评论数"
              value={pagination.total}
              prefix={<CommentOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="已发布"
              value={comments.filter((c) => c.status === 'published' && !c.isDeleted).length}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="已隐藏"
              value={comments.filter((c) => c.status === 'hidden').length}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="待审核"
              value={0}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Card>
        </Col>
      </Row>

      {/* 搜索表单 */}
      <Card style={{ marginBottom: 16 }}>
        <Form layout="inline" onFinish={handleSearch}>
          <Form.Item name="search" label="搜索">
            <Input placeholder="评论内容" allowClear style={{ width: 200 }} />
          </Form.Item>
          <Form.Item name="status" label="状态">
            <Select placeholder="状态" allowClear style={{ width: 120 }}>
              <Option value="published">已发布</Option>
              <Option value="hidden">已隐藏</Option>
              <Option value="deleted">已删除</Option>
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

      {/* 评论表格 */}
      <Card>
        <Table
          columns={columns}
          dataSource={comments}
          rowKey="id"
          loading={loading}
          pagination={pagination}
          onChange={handleTableChange}
          scroll={{ x: 900 }}
        />
      </Card>

      {/* 模态框 */}
      <Modal
        title={modalTitles[modalType]}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={handleModalCancel}
        width={600}
      >
        {modalType === 'view' && selectedComment && (
          <div>
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <p>
                  <strong>作者:</strong> {selectedComment.authorName || selectedComment.authorId || '-'}
                </p>
                <p>
                  <strong>状态:</strong>{' '}
                  <Tag color={getStatusColor(selectedComment.status)}>
                    {selectedComment.isDeleted ? '已删除' : getStatusText(selectedComment.status)}
                  </Tag>
                </p>
              </Col>
              <Col span={12}>
                <p>
                  <strong>点赞:</strong> {selectedComment.likeCount || 0}
                </p>
                <p>
                  <strong>评论时间:</strong> {formatDate(selectedComment.createdAt)}
                </p>
              </Col>
              <Col span={24}>
                <p>
                  <strong>评论内容:</strong>
                </p>
                <p style={{ color: '#666', whiteSpace: 'pre-wrap' }}>
                  {selectedComment.content}
                </p>
              </Col>
            </Row>
          </div>
        )}

        {modalType === 'edit' && selectedComment && (
          <Form form={form} layout="vertical">
            <Form.Item
              name="content"
              label="评论内容"
              rules={[{ required: true, message: '请输入评论内容' }]}
            >
              <TextArea rows={6} placeholder="请输入评论内容" />
            </Form.Item>
            <Form.Item name="status" label="状态">
              <Select>
                <Option value="published">已发布</Option>
                <Option value="hidden">已隐藏</Option>
                <Option value="deleted">已删除</Option>
              </Select>
            </Form.Item>
          </Form>
        )}
      </Modal>
    </div>
  )
}

export default Comments
