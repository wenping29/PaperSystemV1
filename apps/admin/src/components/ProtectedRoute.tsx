import React from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import { ROUTES } from '@/core/constants/routes'

interface ProtectedRouteProps {
  children: React.ReactNode
  requireAdmin?: boolean
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requireAdmin = true,
}) => {
  const { isAuthenticated, user } = useAuthStore()
  const location = useLocation()

  if (!isAuthenticated) {
    //return <Navigate to={ROUTES.LOGIN} state={{ from: location }} replace />
  }

  if (requireAdmin && user) {
    const isAdmin = user.role === 'admin' || user.role === 'superadmin'
    if (!isAdmin) {
      //return <Navigate to={ROUTES.LOGIN} replace />
    }
  }

  return <>{children}</>
}

export default ProtectedRoute
