<template>
  <el-main class="layout-main">
    <el-tabs
      v-model="tabsStore.activeTab"
      type="card"
      class="tabs-container"
      @tab-remove="handleTabRemove"
      @tab-click="handleTabClick"
    >
      <el-tab-pane
        v-for="tab in tabsStore.tabs"
        :key="tab.path"
        :name="tab.path"
        :label="tab.title"
        :closable="tab.closable"
      >
        <template #label>
          <el-icon v-if="tab.icon" class="tab-icon">
            <component :is="tab.icon" />
          </el-icon>
          {{ tab.title }}
        </template>
      </el-tab-pane>
    </el-tabs>
    
    <div class="main-content">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </div>
  </el-main>
</template>

<script setup lang="ts">
import { watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTabsStore } from '@/stores/tabsStore'

const route = useRoute()
const router = useRouter()
const tabsStore = useTabsStore()

watch(
  () => route.path,
  () => {
    tabsStore.addTab(route)
  },
  { immediate: true }
)

const handleTabRemove = (path: string) => {
  tabsStore.removeTab(path)
  router.push(tabsStore.activeTab)
}

const handleTabClick = (pane: any) => {
  const path = pane.props.name
  if (path !== route.path) {
    router.push(path)
  }
}
</script>

<style scoped lang="scss">
.layout-main {
  padding: 0;
  background-color: var(--el-bg-color-page);
}

.tabs-container {
  background-color: var(--el-bg-color);
  border-bottom: 1px solid var(--el-border-color);
  
  :deep(.el-tabs__header) {
    margin: 0;
    border-bottom: none;
  }
  
  :deep(.el-tabs__nav) {
    border: none;
  }
  
  :deep(.el-tabs__item) {
    border: none;
    border-right: 1px solid var(--el-border-color);
    
    &.is-active {
      background-color: var(--el-bg-color-page);
    }
  }
}

.tab-icon {
  margin-right: 4px;
  vertical-align: middle;
}

.main-content {
  padding: 20px;
  height: calc(100vh - 120px);
  overflow-y: auto;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>