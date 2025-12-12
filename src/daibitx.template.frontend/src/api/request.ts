import axios from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useAuthStore } from '@/stores/authStore'

// 创建axios实例
const service = axios.create({
  baseURL: '/api',
  timeout: 30000
})

// 请求拦截器
service.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore()
    const token = authStore.token
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
service.interceptors.response.use(
  (response) => {
    const { data } = response
    
    // 假设后端返回格式: { code: 200, message: 'success', data: {} }
    if (data.code === 200) {
      return data.data
    } else {
      ElMessage.error(data.message || '请求失败')
      return Promise.reject(new Error(data.message || '请求失败'))
    }
  },
  (error) => {
    const { response } = error
    
    if (response?.status === 401) {
      ElMessageBox.confirm('登录已过期，请重新登录', '提示', {
        confirmButtonText: '重新登录',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        const authStore = useAuthStore()
        authStore.logout()
        window.location.href = '/login'
      })
    } else if (response?.status === 403) {
      ElMessage.error('没有权限访问')
    } else if (response?.status === 404) {
      ElMessage.error('请求的资源不存在')
    } else if (response?.status >= 500) {
      ElMessage.error('服务器错误，请稍后重试')
    } else {
      ElMessage.error(error.message || '请求失败')
    }
    
    return Promise.reject(error)
  }
)

export default service