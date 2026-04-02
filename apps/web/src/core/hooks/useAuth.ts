import { useCallback } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { useLoginMutation, useLogoutMutation, useGetProfileQuery } from '@/store/api/authApi'
import { clearCredentials, setCredentials, setUser } from '@/store/slices/authSlice'
import type { RootState, AppDispatch } from '@/store'

export const useAuth = () => {
  const dispatch = useDispatch<AppDispatch>()
  const [loginMutation] = useLoginMutation()
  const [logoutMutation] = useLogoutMutation()
  const { data: profile, refetch: refetchProfile } = useGetProfileQuery()

  const authState = useSelector((state: RootState) => state.auth)

  const login = useCallback(async (credentials: LoginRequest) => {
    try {
      const response = await loginMutation(credentials).unwrap()
      dispatch(setCredentials(response))
      return response
    } catch (error) {
      throw error
    }
  }, [dispatch, loginMutation])

  const logout = useCallback(async () => {
    try {
      await logoutMutation().unwrap()
    } finally {
      dispatch(clearCredentials())
    }
  }, [dispatch, logoutMutation])

  const updateProfile = useCallback((profile: UserProfile) => {
    dispatch(setUser(profile))
  }, [dispatch])

  return {
    ...authState,
    profile,
    login,
    logout,
    updateProfile,
    refetchProfile,
  }
}