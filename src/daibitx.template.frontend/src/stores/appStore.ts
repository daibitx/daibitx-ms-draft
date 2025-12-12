import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'

export interface AppConfig {
  title: string
  logo: string
  theme: 'light' | 'dark'
  sidebarCollapsed: boolean
  sidebarWidth: number
  sidebarCollapsedWidth: number
}

export const useAppStore = defineStore('app', () => {
  // 从 localStorage 读取配置
  const savedConfig = localStorage.getItem('app-config')
  const defaultConfig: AppConfig = {
    title: '后台管理系统',
    logo: '/logo.ico',
    theme: 'light',
    sidebarCollapsed: false,
    sidebarWidth: 240,
    sidebarCollapsedWidth: 64
  }
  
  const config = ref<AppConfig>(savedConfig ? JSON.parse(savedConfig) : defaultConfig)

  const isDark = computed(() => config.value.theme === 'dark')
  const sidebarWidth = computed(() => 
    config.value.sidebarCollapsed 
      ? config.value.sidebarCollapsedWidth 
      : config.value.sidebarWidth
  )

  const toggleSidebar = () => {
    config.value.sidebarCollapsed = !config.value.sidebarCollapsed
  }

  const setTheme = (theme: 'light' | 'dark') => {
    config.value.theme = theme
    document.documentElement.setAttribute('data-theme', theme)
    // 切换 Element-Plus 主题
    if (theme === 'dark') {
      document.documentElement.classList.add('dark')
    } else {
      document.documentElement.classList.remove('dark')
    }
  }

  // 初始化主题
  const initTheme = () => {
    const savedTheme = config.value.theme
    setTheme(savedTheme)
  }

  const setTitle = (title: string) => {
    config.value.title = title
    document.title = title
  }

  // 监听配置变化，自动保存到 localStorage
  watch(config, (newConfig) => {
    localStorage.setItem('app-config', JSON.stringify(newConfig))
  }, { deep: true })

  // 重置配置
  const resetConfig = () => {
    config.value = { ...defaultConfig }
    localStorage.removeItem('app-config')
  }

  return {
    config,
    isDark,
    sidebarWidth,
    toggleSidebar,
    setTheme,
    initTheme,
    setTitle,
    resetConfig
  }
})