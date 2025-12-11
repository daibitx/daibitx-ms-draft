import type { RouteRecordRaw } from 'vue-router'

const systemRoutes: RouteRecordRaw[] = [
  {
    path: '/user',
    name: 'User',
    component: () => import('@/pages/user.vue'),
    meta: { 
      title: '用户管理', 
      icon: 'User',
      requiresAuth: true
    }
  },
  {
    path: '/role',
    name: 'Role',
    component: () => import('@/pages/role.vue'),
    meta: { 
      title: '角色管理', 
      icon: 'Key',
      requiresAuth: true
    }
  }
]

export default systemRoutes