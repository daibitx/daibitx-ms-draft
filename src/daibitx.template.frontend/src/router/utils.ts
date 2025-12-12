import type { RouteRecordRaw } from 'vue-router'
import router from './index'

export interface MenuItem {
  path: string
  name: string
  title: string
  icon?: string
  children?: MenuItem[]
}

/**
 * 从路由配置生成菜单数据
 */
export function generateMenuFromRoutes(routes: readonly RouteRecordRaw[] = []): MenuItem[] {
  const menu: MenuItem[] = []
  
  // 获取布局路由的子路由
  const layoutRoute = routes.find(route => route.path === '/')
  if (!layoutRoute || !layoutRoute.children) {
    return menu
  }
  
  // 遍历子路由生成菜单
  for (const route of layoutRoute.children) {
    // 只处理有 meta.title 的路由
    if (route.meta?.title) {
      const menuItem: MenuItem = {
        path: route.path,
        name: route.name as string,
        title: route.meta.title as string,
        icon: route.meta.icon as string
      }
      
      // 如果有子路由，递归处理
      if (route.children && route.children.length > 0) {
        menuItem.children = generateMenuFromRoutes([{
          path: '/',
          children: route.children
        } as RouteRecordRaw])
      }
      
      menu.push(menuItem)
    }
  }
  
  return menu
}

/**
 * 获取所有菜单项（包括嵌套的）
 */
export function getAllMenuItems(): MenuItem[] {
  return generateMenuFromRoutes(router.getRoutes())
}