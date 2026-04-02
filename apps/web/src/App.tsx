import React, { Suspense } from 'react'
import { Routes, Route } from 'react-router-dom'
import { Spin } from 'antd'
import MainLayout from '@/components/layout/MainLayout'
import PrivateRoute from '@/core/components/PrivateRoute'

// 懒加载页面组件
const HomePage = React.lazy(() => import('@/pages/HomePage'))
const LoginPage = React.lazy(() => import('@/features/auth/pages/LoginPage'))
const RegisterPage = React.lazy(() => import('@/features/auth/pages/RegisterPage'))
const WritingEditorPage = React.lazy(() => import('@/features/writing/pages/WritingEditorPage'))
const CommunityPage = React.lazy(() => import('@/features/community/pages/CommunityPage'))
const ProfilePage = React.lazy(() => import('@/features/profile/pages/ProfilePage'))

function App() {
  return (
    <Suspense fallback={
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <Spin size="large" />
      </div>
    }>
      <Routes>
        {/* 公共路由 */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* 需要认证的路由 */}
        <Route path="/" element={
          <PrivateRoute>
            <MainLayout />
          </PrivateRoute>
        }>
          <Route index element={<HomePage />} />
          <Route path="writing" element={<WritingEditorPage />} />
          <Route path="writing/:workId" element={<WritingEditorPage />} />
          <Route path="community" element={<CommunityPage />} />
          <Route path="profile" element={<ProfilePage />} />
          {/* 更多路由... */}
        </Route>

        {/* 404页面 */}
        <Route path="*" element={<div>404 - 页面未找到</div>} />
      </Routes>
    </Suspense>
  )
}

export default App