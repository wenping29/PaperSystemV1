import React, { useState } from 'react'
import { Layout, Menu, theme, Avatar, Dropdown, Button, Breadcrumb, Typography } from 'antd'
import {
  DashboardOutlined,
  UserOutlined,
  FileTextOutlined,
  CommentOutlined,
  MessageOutlined,
  DollarOutlined,
  SettingOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  LogoutOutlined,
  SafetyOutlined,
  CrownOutlined,
} from '@ant-design/icons'
import { Link, useLocation, useNavigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import { ROUTES } from '@/core/constants/routes'
import { formatDate } from '@/core/utils/formatters'
import styles from './AdminLayout.module.css'

const { Header, Sider, Content } = Layout
const { Text } = Typography

const AdminLayout: React.FC = () => {
  const [collapsed, setCollapsed] = useState(false)
  const location = useLocation()
  const navigate = useNavigate()
  const { user, logout } = useAuthStore()
  const {
    token: { colorBgContainer, borderRadiusLG },
  } = theme.useToken()

  // 菜单项
  const menuItems = [
    {
      key: ROUTES.DASHBOARD,
      icon: <DashboardOutlined />,
      label: <Link to={ROUTES.DASHBOARD}>仪表板</Link>,
    },
    {
      key: 'user-group',
      icon: <UserOutlined />,
      label: '用户管理',
      children: [
        {
          key: ROUTES.USERS,
          icon: <UserOutlined />,
          label: <Link to={ROUTES.USERS}>普通用户</Link>,
        },
        {
          key: ROUTES.ADMIN_USERS,
          icon: <SafetyOutlined />,
          label: <Link to={ROUTES.ADMIN_USERS}>管理员账户</Link>,
        },
      ],
    },
    {
      key: ROUTES.WORKS,
      icon: <FileTextOutlined />,
      label: <Link to={ROUTES.WORKS}>作品管理</Link>,
    },
    {
      key: ROUTES.COMMENTS,
      icon: <CommentOutlined />,
      label: <Link to={ROUTES.COMMENTS}>评论管理</Link>,
    },
    {
      key: ROUTES.MESSAGES,
      icon: <MessageOutlined />,
      label: <Link to={ROUTES.MESSAGES}>消息管理</Link>,
    },
    {
      key: ROUTES.PAYMENTS,
      icon: <DollarOutlined />,
      label: <Link to={ROUTES.PAYMENTS}>支付管理</Link>,
    },
    {
      key: ROUTES.SETTINGS,
      icon: <SettingOutlined />,
      label: <Link to={ROUTES.SETTINGS}>系统设置</Link>,
    },
  ]

  // 用户下拉菜单
  const userMenuItems = [
    {
      key: 'logout',
      label: '退出登录',
      icon: <LogoutOutlined />,
      danger: true,
    },
  ]

  // 处理用户菜单点击
  const handleUserMenuClick = ({ key }: { key: string }) => {
    if (key === 'logout') {
      logout()
      navigate(ROUTES.LOGIN)
    }
  }

  // 面包屑配置
  const getBreadcrumbItems = () => {
    const pathMap: Record<string, string> = {
      [ROUTES.DASHBOARD]: '仪表板',
      [ROUTES.USERS]: '普通用户',
      [ROUTES.ADMIN_USERS]: '管理员账户',
      [ROUTES.WORKS]: '作品管理',
      [ROUTES.COMMENTS]: '评论管理',
      [ROUTES.MESSAGES]: '消息管理',
      [ROUTES.PAYMENTS]: '支付管理',
      [ROUTES.SETTINGS]: '系统设置',
    }

    const items = [{ title: <Link to={ROUTES.DASHBOARD}>首页</Link> }]

    if (location.pathname !== ROUTES.DASHBOARD) {
      // 如果是用户管理相关页面，添加父级
      if (location.pathname === ROUTES.USERS || location.pathname === ROUTES.ADMIN_USERS) {
        items.push({ title: '用户管理' })
      }
      items.push({ title: pathMap[location.pathname] || '未知页面' })
    }

    return items
  }

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        trigger={null}
        collapsible
        collapsed={collapsed}
        theme="dark"
        className={styles.sider}
        width={220}
      >
        <div className={styles.logo}>
          {collapsed ? (
            <span className={styles.logoCollapsed}>PS</span>
          ) : (
            <span className={styles.logoFull}>PaperSystem</span>
          )}
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[location.pathname]}
          items={menuItems}
        />
      </Sider>
      <Layout style={{ marginLeft: collapsed ? 80 : 0, transition: 'all 0.2s' }}>
        <Header className={styles.header}>
          <div className={styles.headerLeft}>
            <Button
              type="text"
              icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
              onClick={() => setCollapsed(!collapsed)}
              className={styles.collapseBtn}
            />
            <Breadcrumb items={getBreadcrumbItems()} className={styles.breadcrumb} />
          </div>
          <div className={styles.headerRight}>
            <Dropdown
              menu={{
                items: userMenuItems,
                onClick: handleUserMenuClick,
              }}
              placement="bottomRight"
            >
              <div className={styles.userInfo}>
                <Avatar
                  src={user?.avatarUrl}
                  icon={<UserOutlined />}
                  size="small"
                />
                <div className={styles.userDetails}>
                  <Text strong className={styles.username}>
                    {user?.username}
                  </Text>
                  <Text type="secondary" className={styles.role}>
                    {user?.role === 'superadmin' ? '超级管理员' : '管理员'}
                  </Text>
                </div>
              </div>
            </Dropdown>
          </div>
        </Header>
        <Content className={styles.content}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}

export default AdminLayout
