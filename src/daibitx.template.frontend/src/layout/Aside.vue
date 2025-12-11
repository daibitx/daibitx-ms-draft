<template>
  <el-aside
    class="layout-aside"
    :width="appStore.sidebarWidth + 'px'"
  >
    <div class="logo-container">
      <img :src="appStore.config.logo" alt="logo" class="logo" />
      <h1 v-show="!appStore.config.sidebarCollapsed" class="logo-text">
        {{ appStore.config.title }}
      </h1>
    </div>
    
    <el-menu
      :default-active="activeMenu"
      :collapse="appStore.config.sidebarCollapsed"
      :collapse-transition="false"
      router
      class="aside-menu"
    >
      <template v-for="menu in menuList" :key="menu.path">
        <!-- 有子菜单的项 -->
        <el-sub-menu
          v-if="menu.children && menu.children.length > 0"
          :index="menu.path"
        >
          <template #title>
            <el-icon v-if="menu.icon">
              <component :is="menu.icon" />
            </el-icon>
            <span>{{ menu.title }}</span>
          </template>
          <el-menu-item
            v-for="child in menu.children"
            :key="child.path"
            :index="child.path"
          >
            <el-icon v-if="child.icon">
              <component :is="child.icon" />
            </el-icon>
            <template #title>{{ child.title }}</template>
          </el-menu-item>
        </el-sub-menu>
        
        <!-- 没有子菜单的项 -->
        <el-menu-item v-else :index="menu.path">
          <el-icon v-if="menu.icon">
            <component :is="menu.icon" />
          </el-icon>
          <template #title>{{ menu.title }}</template>
        </el-menu-item>
      </template>
    </el-menu>
  </el-aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { generateMenuFromRoutes } from '@/router/utils'
import router from '@/router'
import { useAppStore } from '@/stores/appStore'

const route = useRoute()
const appStore = useAppStore()

const activeMenu = computed(() => route.path)
const menuList = computed(() => generateMenuFromRoutes(router.options.routes))
</script>

<style scoped lang="scss">
.layout-aside {
  background-color: var(--el-bg-color);
  border-right: 1px solid var(--el-border-color);
  transition: width 0.3s;
  overflow: hidden;
}

.logo-container {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 60px;
  border-bottom: 1px solid var(--el-border-color);
  gap: 8px;
}

.logo {
  width: 32px;
  height: 32px;
}

.logo-text {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  white-space: nowrap;
}

.aside-menu {
  border-right: none;
  height: calc(100vh - 60px);
  overflow-y: auto;
}
</style>