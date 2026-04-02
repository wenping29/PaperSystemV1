import React from 'react'
import { Card, Typography, Alert } from 'antd'

const { Title } = Typography

const Messages: React.FC = () => {
  return (
    <div>
      <Card>
        <Title level={4}>消息管理</Title>
        <Alert
          message="功能开发中"
          description="消息管理功能正在开发中，敬请期待。"
          type="info"
          showIcon
          style={{ marginTop: 16 }}
        />
      </Card>
    </div>
  )
}

export default Messages
