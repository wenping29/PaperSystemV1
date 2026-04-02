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
  InputNumber,
  Switch,
} from 'antd'
import {
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  EyeOutlined,
  CheckOutlined,
  CloseOutlined,
  LikeOutlined,
  FileTextOutlined,
} from '@ant-design/icons'
import { workApi } from '@/core/api'
import { showError } from '@/core/utils/error-handler'
import { formatDate, truncateText } from '@/core/utils/formatters'
import type { Work, WorkStatus, WorkVisibility } from '@/core/types/entities'
import type { CreateWorkRequest, UpdateWorkRequest } from '@/core/api/endpoints/workApi'

const { TextArea } = Input
const { Option } = Select

const Works: React.FC = () => {
  const [works, setWorks] = useState<any[]>([])
  const [loading, setLoading] = useState(false)
  const [pagination, setPagination] = useState({ current: 1, pageSize: 20, total: 0 })
  const [searchParams, setSearchParams] = useState<any>({})
  const [selectedWork, setSelectedWork] = useState<Work | null>(null)
  const [modalVisible, setModalVisible] = useState(false)
  const [modalType, setModalType] = useState<'view' | 'edit' | 'create'>('view')
  const [form] = Form.useForm()

  useEffect(() => {
    fetchWorks()
  }, [pagination.current, pagination.pageSize, searchParams])

  const fetchWorks = async () => {
    try {
      setLoading(true)
      const params = {
        page: pagination.current,
        pageSize: pagination.pageSize,
        ...searchParams,
      }

      const response = await workApi.getWorks(params)
      const data = response.data?.data || response.data || []
      const totalCount = response.data?.totalCount || data.length * 5

      setWorks(Array.isArray(data) ? data : [])
      setPagination((prev) => ({ ...prev, total: totalCount }))
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
    if (values.visibility) params.visibility = values.visibility
    if (values.category) params.category = values.category

    setSearchParams(params)
    setPagination((prev) => ({ ...prev, current: 1 }))
  }

  const handleViewWork = (work: any) => {
    setSelectedWork(work)
    setModalType('view')
    setModalVisible(true)
  }

  const handleEditWork = (work: any) => {
    setSelectedWork(work)
    setModalType('edit')
    form.setFieldsValue({
      title: work.title,
      content: work.content,
      category: work.category,
      tags: work.tags?.join(', '),
      status: work.status,
      visibility: work.visibility,
      isPublished: work.isPublished,
    })
    setModalVisible(true)
  }

  const handleCreateWork = () => {
    setSelectedWork(null)
    setModalType('create')
    form.resetFields()
    setModalVisible(true)
  }

  const handleDeleteWork = async (id: number) => {
    try {
      await workApi.deleteWork(id)
      message.success('作品删除成功')
      fetchWorks()
    } catch (error) {
      showError(error)
    }
  }

  const handlePublishWork = async (id: number) => {
    try {
      await workApi.publishWork(id)
      message.success('作品发布成功')
      fetchWorks()
    } catch (error) {
      showError(error)
    }
  }

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields()

      const tagsArray = values.tags
        ? values.tags.split(',').map((tag: string) => tag.trim()).filter(Boolean)
        : []

      if (modalType === 'create') {
        const createData: CreateWorkRequest = {
          title: values.title,
          content: values.content,
          category: values.category,
          tags: tagsArray,
        }
        await workApi.createWork(createData)
        message.success('作品创建成功')
      } else if (modalType === 'edit' && selectedWork) {
        const updateData: UpdateWorkRequest = {
          title: values.title,
          content: values.content,
          category: values.category,
          tags: tagsArray,
          status: values.status,
          visibility: values.visibility,
          isPublished: values.isPublished,
        }
        await workApi.updateWork(selectedWork.id, updateData)
        message.success('作品更新成功')
      }

      setModalVisible(false)
      fetchWorks()
    } catch (error) {
      showError(error)
    }
  }

  const handleModalCancel = () => {
    setModalVisible(false)
    setSelectedWork(null)
    form.resetFields()
  }

  const getStatusColor = (status?: WorkStatus) => {
    switch (status) {
      case 'published':
        return 'green'
      case 'draft':
        return 'orange'
      case 'archived':
        return 'default'
      default:
        return 'default'
    }
  }

  const getVisibilityColor = (visibility?: WorkVisibility) => {
    switch (visibility) {
      case 'public':
        return 'blue'
      case 'private':
        return 'red'
      case 'followers':
        return 'purple'
      default:
        return 'default'
    }
  }

  const getStatusText = (status?: WorkStatus, isPublished?: boolean) => {
    if (isPublished) return '已发布'
    switch (status) {
      case 'published':
        return '已发布'
      case 'draft':
        return '草稿'
      case 'archived':
        return '已归档'
      default:
        return '未知'
    }
  }

  const getVisibilityText = (visibility?: WorkVisibility) => {
    switch (visibility) {
      case 'public':
        return '公开'
      case 'private':
        return '私密'
      case 'followers':
        return '仅关注者'
      default:
        return '公开'
    }
  }

  const columns = [
    {
      title: '标题',
      dataIndex: 'title',
      key: 'title',
      render: (text: string, record: any) => (
        <div>
          <div style={{ fontWeight: 500 }}>{text}</div>
          <div style={{ fontSize: 12, color: '#999' }}>
            {truncateText(record.excerpt || record.content || '', 50)}
          </div>
        </div>
      ),
    },
    {
      title: '作者',
      dataIndex: 'author',
      key: 'author',
      render: (author: string, record: any) => author || record.authorName || '-',
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      render: (status: WorkStatus, record: any) => (
        <Tag color={getStatusColor(status)}>
          {record.isPublished ? <CheckOutlined /> : <CloseOutlined />}
          {getStatusText(status, record.isPublished)}
        </Tag>
      ),
    },
    {
      title: '可见性',
      dataIndex: 'visibility',
      key: 'visibility',
      render: (visibility: WorkVisibility) => (
        <Tag color={getVisibilityColor(visibility)}>
          {getVisibilityText(visibility)}
        </Tag>
      ),
    },
    {
      title: '字数',
      dataIndex: 'wordCount',
      key: 'wordCount',
      render: (count: number) => (count || 0).toLocaleString(),
    },
    {
      title: '点赞',
      dataIndex: 'likes',
      key: 'likes',
      render: (likes: number, record: any) => (
        <Space>
          <LikeOutlined />
          {likes || record.likeCount || 0}
        </Space>
      ),
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
      render: (_: any, record: any) => (
        <Space size="small">
          <Tooltip title="查看详情">
            <Button type="link" icon={<EyeOutlined />} onClick={() => handleViewWork(record)} />
          </Tooltip>
          <Tooltip title="编辑">
            <Button type="link" icon={<EditOutlined />} onClick={() => handleEditWork(record)} />
          </Tooltip>
          {!record.isPublished && record.status !== 'published' && (
            <Tooltip title="发布">
              <Button
                type="link"
                icon={<CheckOutlined />}
                onClick={() => handlePublishWork(record.id)}
              />
            </Tooltip>
          )}
          <Popconfirm
            title="确定要删除这个作品吗？"
            onConfirm={() => handleDeleteWork(record.id)}
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
    view: '作品详情',
    edit: '编辑作品',
    create: '创建作品',
  }

  return (
    <div>
      {/* 统计卡片 */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="总作品数"
              value={pagination.total}
              prefix={<FileTextOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="已发布"
              value={works.filter((w) => w.isPublished || w.status === 'published').length}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="草稿"
              value={works.filter((w) => w.status === 'draft' && !w.isPublished).length}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="总字数"
              value={works.reduce((sum, w) => sum + (w.wordCount || 0), 0)}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
      </Row>

      {/* 搜索表单 */}
      <Card style={{ marginBottom: 16 }}>
        <Form layout="inline" onFinish={handleSearch}>
          <Form.Item name="search" label="搜索">
            <Input placeholder="标题/内容" allowClear style={{ width: 200 }} />
          </Form.Item>
          <Form.Item name="status" label="状态">
            <Select placeholder="状态" allowClear style={{ width: 120 }}>
              <Option value="draft">草稿</Option>
              <Option value="published">已发布</Option>
              <Option value="archived">已归档</Option>
            </Select>
          </Form.Item>
          <Form.Item name="visibility" label="可见性">
            <Select placeholder="可见性" allowClear style={{ width: 120 }}>
              <Option value="public">公开</Option>
              <Option value="private">私密</Option>
              <Option value="followers">仅关注者</Option>
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

      {/* 作品表格 */}
      <Card>
        <div style={{ marginBottom: 16 }}>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleCreateWork}>
            创建作品
          </Button>
        </div>
        <Table
          columns={columns}
          dataSource={works}
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
        width={modalType === 'view' ? 700 : 600}
      >
        {modalType === 'view' && selectedWork && (
          <div>
            <Row gutter={[16, 16]}>
              <Col span={24}>
                <h3 style={{ marginBottom: 8 }}>{selectedWork.title}</h3>
                <Space wrap style={{ marginBottom: 16 }}>
                  <Tag color={getStatusColor(selectedWork.status)}>
                    {getStatusText(selectedWork.status, selectedWork.isPublished)}
                  </Tag>
                  <Tag color={getVisibilityColor(selectedWork.visibility)}>
                    {getVisibilityText(selectedWork.visibility)}
                  </Tag>
                </Space>
              </Col>
              <Col span={12}>
                <p>
                  <strong>作者:</strong> {selectedWork.author || selectedWork.authorName || '-'}
                </p>
                <p>
                  <strong>分类:</strong> {selectedWork.category || '-'}
                </p>
                <p>
                  <strong>字数:</strong> {(selectedWork.wordCount || 0).toLocaleString()}
                </p>
              </Col>
              <Col span={12}>
                <p>
                  <strong>点赞:</strong> {selectedWork.likes || selectedWork.likeCount || 0}
                </p>
                <p>
                  <strong>浏览:</strong> {selectedWork.views || 0}
                </p>
                <p>
                  <strong>创建时间:</strong> {formatDate(selectedWork.createdAt)}
                </p>
              </Col>
              {selectedWork.tags && selectedWork.tags.length > 0 && (
                <Col span={24}>
                  <p>
                    <strong>标签:</strong>{' '}
                    {selectedWork.tags.map((tag: string, i: number) => (
                      <Tag key={i}>{tag}</Tag>
                    ))}
                  </p>
                </Col>
              )}
              {(selectedWork.excerpt || selectedWork.content) && (
                <Col span={24}>
                  <p>
                    <strong>内容摘要:</strong>
                  </p>
                  <p style={{ color: '#666', whiteSpace: 'pre-wrap' }}>
                    {selectedWork.excerpt || truncateText(selectedWork.content, 300)}
                  </p>
                </Col>
              )}
            </Row>
          </div>
        )}

        {(modalType === 'edit' || modalType === 'create') && (
          <Form form={form} layout="vertical">
            <Form.Item
              name="title"
              label="标题"
              rules={[{ required: true, message: '请输入标题' }]}
            >
              <Input placeholder="请输入作品标题" />
            </Form.Item>
            <Form.Item name="category" label="分类">
              <Input placeholder="请输入分类" />
            </Form.Item>
            <Form.Item name="tags" label="标签">
              <Input placeholder="多个标签用逗号分隔" />
            </Form.Item>
            <Form.Item name="content" label="内容">
              <TextArea rows={8} placeholder="请输入作品内容" />
            </Form.Item>
            {modalType === 'edit' && (
              <>
                <Form.Item name="status" label="状态">
                  <Select>
                    <Option value="draft">草稿</Option>
                    <Option value="published">已发布</Option>
                    <Option value="archived">已归档</Option>
                  </Select>
                </Form.Item>
                <Form.Item name="visibility" label="可见性">
                  <Select>
                    <Option value="public">公开</Option>
                    <Option value="private">私密</Option>
                    <Option value="followers">仅关注者</Option>
                  </Select>
                </Form.Item>
                <Form.Item name="isPublished" label="已发布" valuePropName="checked">
                  <Switch />
                </Form.Item>
              </>
            )}
          </Form>
        )}
      </Modal>
    </div>
  )
}

export default Works
