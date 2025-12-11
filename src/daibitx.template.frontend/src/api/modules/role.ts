import { http } from '@/api/index'
import type { ApiResponse } from '@/api/index'

export interface Role {
  id: number
  name: string
  code: string
  description: string
  permissions: string[]
  status: 'active' | 'inactive'
  createTime: string
  updateTime: string
}

export interface RoleQuery {
  name?: string
  code?: string
  status?: string
  page: number
  pageSize: number
}

export interface RoleCreate {
  name: string
  code: string
  description: string
  permissions: string[]
  status: 'active' | 'inactive'
}

export interface RoleUpdate {
  name?: string
  description?: string
  permissions?: string[]
  status?: 'active' | 'inactive'
}

// 获取角色列表
export const getRoleList = (params: RoleQuery) => {
  return http.get<{
    list: Role[]
    total: number
  }>('/roles', { params })
}

// 获取角色详情
export const getRoleDetail = (id: number) => {
  return http.get<Role>(`/roles/${id}`)
}

// 创建角色
export const createRole = (data: RoleCreate) => {
  return http.post<Role>('/roles', data)
}

// 更新角色
export const updateRole = (id: number, data: RoleUpdate) => {
  return http.put<Role>(`/roles/${id}`, data)
}

// 删除角色
export const deleteRole = (id: number) => {
  return http.delete(`/roles/${id}`)
}

// 批量删除角色
export const batchDeleteRoles = (ids: number[]) => {
  return http.post('/roles/batch-delete', { ids })
}

// 更新角色状态
export const updateRoleStatus = (id: number, status: 'active' | 'inactive') => {
  return http.put(`/roles/${id}/status`, { status })
}

// 获取角色权限
export const getRolePermissions = (id: number) => {
  return http.get<string[]>(`/roles/${id}/permissions`)
}

// 更新角色权限
export const updateRolePermissions = (id: number, permissions: string[]) => {
  return http.put(`/roles/${id}/permissions`, { permissions })
}

// 获取所有权限列表
export const getAllPermissions = () => {
  return http.get<{
    list: Array<{
      id: number
      name: string
      code: string
      parentId: number | null
    }>
  }>('/permissions')
}