import axios from 'axios'
import type { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'
import service from '@/api/request'

export interface ApiResponse<T = any> {
  code: number
  message: string
  data: T
}

class HttpClient {
  private service: AxiosInstance

  constructor() {
    this.service = service
  }

  // GET请求
  get<T = any>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return this.service.get(url, config)
  }

  // POST请求
  post<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    return this.service.post(url, data, config)
  }

  // PUT请求
  put<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    return this.service.put(url, data, config)
  }

  // DELETE请求
  delete<T = any>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return this.service.delete(url, config)
  }

  // PATCH请求
  patch<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    return this.service.patch(url, data, config)
  }

  // 文件上传
  upload<T = any>(url: string, file: File, data?: any): Promise<T> {
    const formData = new FormData()
    formData.append('file', file)
    
    if (data) {
      Object.keys(data).forEach(key => {
        formData.append(key, data[key])
      })
    }

    return this.service.post(url, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  }

  // 文件下载
  download(url: string, params?: any, filename?: string): Promise<void> {
    return this.service.get(url, {
      params,
      responseType: 'blob'
    }).then(response => {
      const blob = new Blob([response.data])
      const downloadUrl = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = downloadUrl
      link.download = filename || 'download'
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(downloadUrl)
    })
  }
}

// 导出http实例
export const http = new HttpClient()

// 导出默认实例
export default service

// 登录相关接口
export const loginApi = {
  // 用户登录
  login: (username: string, password: string) => {
    // 模拟登录接口，实际项目中应该调用真实API
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        if (username === 'admin' && password === '123456') {
          resolve({
            token: 'mock-token-' + Date.now(),
            userInfo: {
              id: 1,
              username: 'admin',
              nickname: '管理员',
              email: 'admin@example.com',
              avatar: '',
              roles: ['admin']
            }
          })
        } else {
          reject(new Error('用户名或密码错误'))
        }
      }, 500)
    })
  },

  // 获取用户信息
  getUserInfo: () => {
    // 模拟获取用户信息
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve({
          id: 1,
          username: 'admin',
          nickname: '管理员',
          email: 'admin@example.com',
          avatar: '',
          roles: ['admin'],
          permissions: ['user:view', 'user:edit', 'role:view', 'role:edit']
        })
      }, 300)
    })
  },

  // 刷新token
  refreshToken: () => {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve({
          token: 'mock-token-' + Date.now()
        })
      }, 300)
    })
  },

  // 退出登录
  logout: () => {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve({ message: '退出成功' })
      }, 300)
    })
  }
}

// 导出所有API模块
export * from './modules/user'
export * from './modules/role'