import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import type { RouteLocationNormalized } from 'vue-router'

export interface TabItem {
  name: string
  path: string
  title: string
  icon?: string
  closable?: boolean
}

export const useTabsStore = defineStore('tabs', () => {
  // 从 localStorage 读取标签页数据
  const savedTabs = localStorage.getItem('tabs-data')
  const savedActiveTab = localStorage.getItem('tabs-active')
  
  const tabs = ref<TabItem[]>(
    savedTabs
      ? JSON.parse(savedTabs)
      : [
          {
            name: 'Dashboard',
            path: '/dashboard',
            title: '仪表盘',
            icon: 'House',
            closable: false
          }
        ]
  )
  
  const activeTab = ref<string>(savedActiveTab || '/dashboard')

  const addTab = (route: RouteLocationNormalized) => {
    if (!route.meta.title) return
    
    const existingTab = tabs.value.find(tab => tab.path === route.path)
    if (!existingTab) {
      tabs.value.push({
        name: route.name as string,
        path: route.path,
        title: route.meta.title as string,
        icon: route.meta.icon as string,
        closable: route.path !== '/dashboard'
      })
    }
    
    activeTab.value = route.path
  }

  const removeTab = (path: string) => {
    if (path === '/dashboard') return
    
    const index = tabs.value.findIndex(tab => tab.path === path)
    if (index !== -1) {
      tabs.value.splice(index, 1)
      
      if (activeTab.value === path) {
        const lastTab = tabs.value[tabs.value.length - 1]
        activeTab.value = lastTab.path
      }
    }
  }

  const setActiveTab = (path: string) => {
    activeTab.value = path
  }

  // 清空所有标签页（保留仪表盘）
  const clearTabs = () => {
    tabs.value = [
      {
        name: 'Dashboard',
        path: '/dashboard',
        title: '仪表盘',
        icon: 'House',
        closable: false
      }
    ]
    activeTab.value = '/dashboard'
  }

  // 关闭其他标签页
  const closeOtherTabs = (path: string) => {
    const currentTab = tabs.value.find(tab => tab.path === path)
    if (currentTab) {
      tabs.value = [
        {
          name: 'Dashboard',
          path: '/dashboard',
          title: '仪表盘',
          icon: 'House',
          closable: false
        },
        currentTab
      ]
      activeTab.value = path
    }
  }

  // 关闭所有标签页
  const closeAllTabs = () => {
    clearTabs()
  }

  // 监听标签页变化，自动保存到 localStorage
  watch(tabs, (newTabs) => {
    localStorage.setItem('tabs-data', JSON.stringify(newTabs))
  }, { deep: true })

  // 监听活跃标签页变化
  watch(activeTab, (newActiveTab) => {
    localStorage.setItem('tabs-active', newActiveTab)
  })

  return {
    tabs,
    activeTab,
    addTab,
    removeTab,
    setActiveTab,
    clearTabs,
    closeOtherTabs,
    closeAllTabs
  }
})