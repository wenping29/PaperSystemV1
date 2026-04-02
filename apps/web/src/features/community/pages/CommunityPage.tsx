import React, { useState } from 'react'
import { Card, Row, Col, Input, Button, Avatar, Typography, Tag, List, Space, Tabs } from 'antd'
import { SearchOutlined, FireOutlined, StarOutlined, MessageOutlined, EyeOutlined } from '@ant-design/icons'

const { Title, Text, Paragraph } = Typography
const { Search } = Input
const { TabPane } = Tabs

interface CommunityPost {
  id: string
  title: string
  author: {
    name: string
    avatar: string
    level: number
  }
  content: string
  tags: string[]
  likes: number
  comments: number
  views: number
  createdAt: string
  isHot: boolean
  isFeatured: boolean
}

const CommunityPage: React.FC = () => {
  const [searchText, setSearchText] = useState('')
  const [activeTab, setActiveTab] = useState('hot')

  const mockPosts: CommunityPost[] = [
    {
      id: '1',
      title: '如何用AI写作提升创作效率',
      author: {
        name: '张三',
        avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=张三',
        level: 5
      },
      content: '分享一些使用AI写作工具的技巧和经验，帮助大家提升创作效率...',
      tags: ['AI写作', '效率', '技巧'],
      likes: 124,
      comments: 23,
      views: 1567,
      createdAt: '2023-10-15',
      isHot: true,
      isFeatured: true
    },
    {
      id: '2',
      title: '文学创作中的情感表达',
      author: {
        name: '李四',
        avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=李四',
        level: 3
      },
      content: '探讨文学作品中情感表达的重要性及实现方法...',
      tags: ['文学', '情感', '创作'],
      likes: 89,
      comments: 12,
      views: 987,
      createdAt: '2023-10-14',
      isHot: true,
      isFeatured: false
    },
    {
      id: '3',
      title: '科技类文章的写作要点',
      author: {
        name: '王五',
        avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=王五',
        level: 7
      },
      content: '总结科技类文章的写作特点和注意事项...',
      tags: ['科技', '写作', '要点'],
      likes: 67,
      comments: 8,
      views: 654,
      createdAt: '2023-10-13',
      isHot: false,
      isFeatured: true
    },
    {
      id: '4',
      title: '诗歌创作的心得体会',
      author: {
        name: '赵六',
        avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=赵六',
        level: 4
      },
      content: '分享个人在诗歌创作过程中的一些心得体会...',
      tags: ['诗歌', '创作', '心得'],
      likes: 45,
      comments: 6,
      views: 432,
      createdAt: '2023-10-12',
      isHot: false,
      isFeatured: false
    }
  ]

  const hotAuthors = [
    { name: '张三', posts: 42, likes: 1245, avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=张三1' },
    { name: '李四', posts: 28, likes: 987, avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=李四1' },
    { name: '王五', posts: 35, likes: 876, avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=王五1' },
    { name: '赵六', posts: 19, likes: 654, avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=赵六1' }
  ]

  const handleSearch = (value: string) => {
    console.log('Search:', value)
    setSearchText(value)
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Title level={2}>创作社区</Title>
        <Paragraph type="secondary">
          与万千创作者交流分享，发现优秀作品，提升创作技能
        </Paragraph>
      </div>

      <Row gutter={[24, 24]}>
        <Col span={18}>
          <Card>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
              <Search
                placeholder="搜索文章、作者或话题"
                allowClear
                enterButton={<Button type="primary" icon={<SearchOutlined />}>搜索</Button>}
                size="large"
                style={{ width: 400 }}
                onSearch={handleSearch}
              />
              <Button type="primary">发布文章</Button>
            </div>

            <Tabs activeKey={activeTab} onChange={setActiveTab}>
              <TabPane tab="热门" key="hot" icon={<FireOutlined />} />
              <TabPane tab="最新" key="newest" />
              <TabPane tab="精华" key="featured" icon={<StarOutlined />} />
              <TabPane tab="关注" key="following" />
            </Tabs>

            <List
              itemLayout="vertical"
              dataSource={mockPosts}
              renderItem={(post) => (
                <List.Item
                  key={post.id}
                  actions={[
                    <Space key="likes">
                      <Text type="secondary"><FireOutlined /> {post.likes}</Text>
                    </Space>,
                    <Space key="comments">
                      <Text type="secondary"><MessageOutlined /> {post.comments}</Text>
                    </Space>,
                    <Space key="views">
                      <Text type="secondary"><EyeOutlined /> {post.views}</Text>
                    </Space>
                  ]}
                  extra={
                    post.isHot && <Tag color="red" icon={<FireOutlined />}>热门</Tag> ||
                    post.isFeatured && <Tag color="gold" icon={<StarOutlined />}>精华</Tag>
                  }
                >
                  <List.Item.Meta
                    avatar={<Avatar src={post.author.avatar} size="large" />}
                    title={
                      <Space>
                        <a href={`/post/${post.id}`}>{post.title}</a>
                        <Tag>Lv.{post.author.level}</Tag>
                      </Space>
                    }
                    description={
                      <Space>
                        <Text type="secondary">{post.author.name}</Text>
                        <Text type="secondary">发布于 {post.createdAt}</Text>
                        {post.tags.map(tag => (
                          <Tag key={tag} color="blue">{tag}</Tag>
                        ))}
                      </Space>
                    }
                  />
                  <Paragraph ellipsis={{ rows: 2 }}>
                    {post.content}
                  </Paragraph>
                </List.Item>
              )}
            />
          </Card>
        </Col>

        <Col span={6}>
          <Card title="热门作者" style={{ marginBottom: 24 }}>
            <List
              dataSource={hotAuthors}
              renderItem={(author) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={<Avatar src={author.avatar} />}
                    title={<a href={`/user/${author.name}`}>{author.name}</a>}
                    description={
                      <Space direction="vertical" size={0}>
                        <Text type="secondary">作品: {author.posts}</Text>
                        <Text type="secondary">获赞: {author.likes}</Text>
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          </Card>

          <Card title="热门话题">
            <Space direction="vertical" style={{ width: '100%' }}>
              {['AI写作', '文学创作', '诗歌', '散文', '科技文章', '生活随笔'].map((topic) => (
                <Button key={topic} type="text" block style={{ textAlign: 'left' }}>
                  #{topic}
                </Button>
              ))}
            </Space>
          </Card>

          <Card title="社区指南" style={{ marginTop: 24 }}>
            <Paragraph type="secondary">
              <ul>
                <li>尊重原创，禁止抄袭</li>
                <li>友善交流，文明发言</li>
                <li>遵守法律法规</li>
                <li>发现违规内容请举报</li>
              </ul>
            </Paragraph>
          </Card>
        </Col>
      </Row>
    </div>
  )
}

export default CommunityPage