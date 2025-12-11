import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { loginApi } from '@/api/index'
import { ElMessage } from 'element-plus'

export interface UserInfo {
  id: number
  username: string
  nickname: string
  email: string
  avatar: string
  roles: string[]
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>(localStorage.getItem('token') || '')
  const userInfo = ref<UserInfo | null>(null)
  const permissions = ref<string[]>([])

  const isAuthenticated = computed(() => !!token.value)

  const login = async (username: string, password: string) => {
    try {
      const response = await loginApi.login(username, password)
      const { token: authToken, userInfo: info } = response
      
      token.value = authToken
      localStorage.setItem('token', authToken)
      userInfo.value = info
      
      // 获取用户权限
      await fetchUserPermissions()
      
      ElMessage.success('登录成功')
      return Promise.resolve(response)
    } catch (error) {
      ElMessage.error('登录失败，请检查用户名和密码')
      return Promise.reject(error)
    }
  }

  const fetchUserPermissions = async () => {
    try {
      const response = await loginApi.getUserInfo()
      permissions.value = response.permissions || []
    } catch (error) {
      console.error('获取用户权限失败:', error)
      permissions.value = []
    }
  }

  const logout = async () => {
    try {
      await loginApi.logout()
    } catch (error) {
      console.error('退出登录失败:', error)
    } finally {
      token.value = ''
      userInfo.value = null
      permissions.value = []
      localStorage.removeItem('token')
    }
  }

  const hasPermission = (permission: string) => {
    return permissions.value.includes(permission)
  }

  const updateUserInfo = (info: UserInfo) => {
    userInfo.value = info
  }

  // 初始化时获取用户信息
  const initAuth = async () => {
    if (token.value && !userInfo.value) {
      try {
        await fetchUserPermissions()
      } catch (error) {
        console.error('初始化认证信息失败:', error)
        await logout()
      }
    }
  }

  return {
    token,
    userInfo,
    permissions,
    isAuthenticated,
    login,
    logout,
    hasPermission,
    updateUserInfo,
    initAuth,
    fetchUserPermissions
  }
})