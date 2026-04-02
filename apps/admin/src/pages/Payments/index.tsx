import React from 'react'
import { Card, Typography, Alert } from 'antd'

const { Title } = Typography

const Payments: React.FC = () => {
  return (
    <div>
      <Card>
        <Title level={4}>支付管理</Title>
        <Alert
          message="功能开发中"
          description="支付管理功能正在开发中，敬请期待。"
          type="info"
          showIcon
          style={{ marginTop: 16 }}
        />
      </Card>
    </div>
  )
}

export default Payments
