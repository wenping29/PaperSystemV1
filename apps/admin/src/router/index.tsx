import React, { lazy, Suspense } from 'react'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { Spin } from 'antd'
import ProtectedRoute from '@/components/ProtectedRoute'
import AdminLayout from '@/components/Layout/AdminLayout'
import { ROUTES } from '@/core/constants/routes'

// 页面组件
const Login = lazy(() => import('@/pages/Login'))
const Dashboard = lazy(() => import('@/pages/Dashboard'))
const Users = lazy(() => import('@/pages/Users'))
const AdminUsers = lazy(() => import('@/pages/AdminUsers'))
const Works = lazy(() => import('@/pages/Works'))
const Comments = lazy(() => import('@/pages/Comments'))
const Messages = lazy(() => import('@/pages/Messages'))
const Payments = lazy(() => import('@/pages/Payments'))
const Settings = lazy(() => import('@/pages/Settings'))

const LoadingFallback = () => (
  <div style={{
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '100vh'
  }}>
    <Spin size="large" tip="加载中..." />
  </div>
)

const PageFallback = () => (
  <div style={{
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '100%'
  }}>
    <Spin size="large" tip="加载中..." />
  </div>
)

export const router = createBrowserRouter([
  {
    path: ROUTES.LOGIN,
    element: (
      <Suspense fallback={<LoadingFallback />}>
        <Login />
      </Suspense>
    ),
  },
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <AdminLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <Navigate to={ROUTES.DASHBOARD} replace />,
      },
      {
        path: ROUTES.DASHBOARD,
        element: (
          <Suspense fallback={<PageFallback />}>
            <Dashboard />
          </Suspense>
        ),
      },
      {
        path: ROUTES.USERS,
        element: (
          <Suspense fallback={<PageFallback />}>
            <Users />
          </Suspense>
        ),
      },
      {
        path: ROUTES.ADMIN_USERS,
        element: (
          <Suspense fallback={<PageFallback />}>
            <AdminUsers />
          </Suspense>
        ),
      },
      {
        path: ROUTES.WORKS,
        element: (
          <Suspense fallback={<PageFallback />}>
            <Works />
          </Suspense>
        ),
      },
      {
        path: ROUTES.COMMENTS,
        element: (
          <Suspense fallback={<PageFallback />}>
            <Comments />
          </Suspense>
        ),
      },
      {
        path: ROUTES.MESSAGES,
        element: (
          <Suspense fallback={<PageFallback />}>
            <Messages />
          </Suspense>
        ),
      },
      {
        path: ROUTES.PAYMENTS,
        element: (
          <Suspense fallback={<PageFallback />}>
            <Payments />
          </Suspense>
        ),
      },
      {
        path: ROUTES.SETTINGS,
        element: (
          <Suspense fallback={<PageFallback />}>
            <Settings />
          </Suspense>
        ),
      },
    ],
  },
  {
    path: '*',
    element: <Navigate to={ROUTES.DASHBOARD} replace />,
  },
])
