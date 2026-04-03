/// <reference types="vite/client" />
import { useCallback, useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { useLoginMutation, useLogoutMutation, useGetProfileQuery, LoginRequest } from '@/store/api/authApi'
import { clearCredentials, setCredentials, setUser, UserProfile } from '@/store/slices/authSlice'
import type { RootState, AppDispatch } from '@/store'

export const useAuth = () => {
  const dispatch = useDispatch<AppDispatch>()
  const [loginMutation] = useLoginMutation()
  const [logoutMutation] = useLogoutMutation()
  const authState = useSelector((state: RootState) => state.auth)
  const { data: profile, refetch: refetchProfile } = useGetProfileQuery(
    authState.user?.id ? parseInt(authState.user.id) : 0,
    { skip: !authState.user?.id }
  )
  //console.log(profile);

  useEffect(() => {
    if (profile) {
      dispatch(setUser(profile))
    }
  }, [profile, dispatch])

  const login = useCallback(async (credentials: LoginRequest) => {
    try {
      console.log(import.meta.env.VITE_API_BASE_URL, credentials);
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
    console.log('Updating user profile:', profile)
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