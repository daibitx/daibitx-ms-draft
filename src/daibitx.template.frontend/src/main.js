// 先引入 Element Plus 样式
import 'element-plus/dist/index.css'
// 再引入自定义全局样式
import './styles/global.scss'
// 最后引入基础样式（已清理冲突）
import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import zhCn from 'element-plus/dist/locale/zh-cn.mjs'
import App from './App.vue'
import router from './router'
import { useAppStore } from './stores/appStore'

const app = createApp(App)

// 注册所有图标
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.use(createPinia())
app.use(router)
app.use(ElementPlus, {
  locale: zhCn,
  // 配置 Element-Plus 主题变量
  size: 'default',
  zIndex: 3000
})

app.mount('#app')

// 初始化应用主题
const appStore = useAppStore()
appStore.initTheme()
