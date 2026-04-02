import React, { useState, useRef } from 'react'
import { Card, Button, Space, Typography, Input, Select, Form, message, Row, Col } from 'antd'
import { SaveOutlined, EyeOutlined, AIOutlined, DownloadOutlined } from '@ant-design/icons'
import ReactQuill from 'react-quill'
import 'react-quill/dist/quill.snow.css'

const { Title, Text } = Typography
const { TextArea } = Input
const { Option } = Select

const WritingEditorPage: React.FC = () => {
  const [title, setTitle] = useState('')
  const [content, setContent] = useState('')
  const [category, setCategory] = useState('article')
  const [tags, setTags] = useState<string[]>([])
  const [summary, setSummary] = useState('')
  const [loading, setLoading] = useState(false)
  const quillRef = useRef<any>(null)

  const categories = [
    { value: 'article', label: '文章' },
    { value: 'story', label: '小说' },
    { value: 'poetry', label: '诗歌' },
    { value: 'essay', label: '散文' },
    { value: 'report', label: '报告' }
  ]

  const tagOptions = [
    '文学', '科技', '生活', '教育', '财经', '健康', '娱乐', '旅游', '美食', '体育'
  ]

  const modules = {
    toolbar: [
      [{ 'header': [1, 2, 3, false] }],
      ['bold', 'italic', 'underline', 'strike'],
      [{ 'list': 'ordered' }, { 'list': 'bullet' }],
      ['link', 'image', 'video'],
      ['clean']
    ]
  }

  const formats = [
    'header', 'bold', 'italic', 'underline', 'strike',
    'list', 'bullet', 'link', 'image', 'video'
  ]

  const handleSave = async () => {
    if (!title.trim()) {
      message.warning('请输入标题')
      return
    }
    if (!content.trim()) {
      message.warning('请输入内容')
      return
    }

    setLoading(true)
    try {
      // 模拟保存API调用
      await new Promise(resolve => setTimeout(resolve, 1000))
      message.success('保存成功！')
      console.log('Saved:', { title, content, category, tags, summary })
    } catch (error) {
      message.error('保存失败')
      console.error('Save error:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleAIAssist = async () => {
    message.info('AI助手功能开发中...')
    // 后续集成AI生成功能
  }

  const handleExport = () => {
    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${title || 'untitled'}.txt`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
    message.success('导出成功！')
  }

  return (
    <div>
      <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
        <Col>
          <Title level={2}>写作编辑器</Title>
        </Col>
        <Col>
          <Space>
            <Button icon={<AIOutlined />} onClick={handleAIAssist}>
              AI助手
            </Button>
            <Button icon={<EyeOutlined />}>
              预览
            </Button>
            <Button icon={<DownloadOutlined />} onClick={handleExport}>
              导出
            </Button>
            <Button
              type="primary"
              icon={<SaveOutlined />}
              loading={loading}
              onClick={handleSave}
            >
              保存
            </Button>
          </Space>
        </Col>
      </Row>

      <Row gutter={[24, 24]}>
        <Col span={24}>
          <Card>
            <Form layout="vertical">
              <Form.Item label="标题">
                <Input
                  size="large"
                  placeholder="请输入文章标题"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
              </Form.Item>

              <Form.Item label="分类">
                <Select
                  value={category}
                  onChange={setCategory}
                  style={{ width: 200 }}
                >
                  {categories.map(cat => (
                    <Option key={cat.value} value={cat.value}>{cat.label}</Option>
                  ))}
                </Select>
              </Form.Item>

              <Form.Item label="标签">
                <Select
                  mode="tags"
                  placeholder="选择或输入标签"
                  value={tags}
                  onChange={setTags}
                  style={{ width: '100%' }}
                >
                  {tagOptions.map(tag => (
                    <Option key={tag} value={tag}>{tag}</Option>
                  ))}
                </Select>
              </Form.Item>

              <Form.Item label="内容">
                <ReactQuill
                  ref={quillRef}
                  theme="snow"
                  value={content}
                  onChange={setContent}
                  modules={modules}
                  formats={formats}
                  style={{ height: 400, marginBottom: 50 }}
                />
              </Form.Item>

              <Form.Item label="摘要">
                <TextArea
                  rows={3}
                  placeholder="请输入文章摘要（可选）"
                  value={summary}
                  onChange={(e) => setSummary(e.target.value)}
                  maxLength={200}
                  showCount
                />
              </Form.Item>
            </Form>
          </Card>
        </Col>

        <Col span={8}>
          <Card title="写作统计">
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text type="secondary">字数统计</Text>
                <Title level={3}>{content.replace(/<[^>]*>/g, '').length}</Title>
              </div>
              <div>
                <Text type="secondary">段落数</Text>
                <Title level={3}>{(content.match(/<p>/g) || []).length}</Title>
              </div>
              <div>
                <Text type="secondary">预计阅读时间</Text>
                <Title level={3}>{Math.ceil(content.replace(/<[^>]*>/g, '').length / 500)} 分钟</Title>
              </div>
            </Space>
          </Card>
        </Col>

        <Col span={16}>
          <Card title="AI写作建议">
            <Text type="secondary">
              尝试使用AI助手来：
              <ul>
                <li>生成文章大纲</li>
                <li>优化句子表达</li>
                <li>检查语法错误</li>
                <li>扩展内容段落</li>
              </ul>
            </Text>
            <Button type="dashed" block onClick={handleAIAssist}>
              使用AI助手
            </Button>
          </Card>
        </Col>
      </Row>
    </div>
  )
}

export default WritingEditorPage