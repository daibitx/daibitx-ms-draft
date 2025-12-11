import { http } from '@/api/index'
import type { ApiResponse } from '@/api/index'

export interface User {
  id: number
  username: string
  nickname: string
  email: string
  phone: string
  avatar: string
  roles: string[]
  status: 'active' | 'inactive'
  createTime: string
  updateTime: string
}

export interface UserQuery {
  username?: string
  nickname?: string
  email?: string
  phone?: string
  status?: string
  page: number
  pageSize: number
}

export interface UserCreate {
  username: string
  nickname: string
  email: string
  phone?: string
  roles: string[]
  status: 'active' | 'inactive'
}

export interface UserUpdate {
  nickname?: string
  email?: string
  phone?: string
  roles?: string[]
  status?: 'active' | 'inactive'
}

// 获取用户列表
export const getUserList = (params: UserQuery) => {
  return http.get<{
    list: User[]
    total: number
  }>('/users', { params })
}

// 获取用户详情
export const getUserDetail = (id: number) => {
  return http.get<User>(`/users/${id}`)
}

// 创建用户
export const createUser = (data: UserCreate) => {
  return http.post<User>('/users', data)
}

// 更新用户
export const updateUser = (id: number, data: UserUpdate) => {
  return http.put<User>(`/users/${id}`, data)
}

// 删除用户
export const deleteUser = (id: number) => {
  return http.delete(`/users/${id}`)
}

// 批量删除用户
export const batchDeleteUsers = (ids: number[]) => {
  return http.post('/users/batch-delete', { ids })
}

// 重置用户密码
export const resetUserPassword = (id: number) => {
  return http.post(`/users/${id}/reset-password`)
}

// 更新用户状态
export const updateUserStatus = (id: number, status: 'active' | 'inactive') => {
  return http.put(`/users/${id}/status`, { status })
}

// 上传用户头像
export const uploadUserAvatar = (file: File) => {
  return http.upload<{ url: string }>('/users/avatar', file)
}