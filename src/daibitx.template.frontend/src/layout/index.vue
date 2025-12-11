<template>
  <el-container class="layout-container">
    <Aside />
    <el-container class="main-container">
      <Header />
      <Main />
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import Aside from './Aside.vue'
import Header from './Header.vue'
import Main from './Main.vue'
</script>

<style scoped lang="scss">
.layout-container {
  height: 100vh;
}

.main-container {
  flex-direction: column;
}
</style>