<template>
  <div class="login-container">
    <div class="login-form">
      <div class="login-header">
        <img src="/favicon.ico" alt="logo" class="login-logo" />
        <h2 class="login-title">后台管理系统</h2>
        <p class="login-subtitle">欢迎回来，请登录您的账户</p>
      </div>
      
      <el-form
        ref="loginFormRef"
        :model="loginForm"
        :rules="loginRules"
        class="form-content"
        @submit.prevent="handleLogin"
      >
        <el-form-item prop="username">
          <el-input
            v-model="loginForm.username"
            placeholder="请输入用户名"
            :prefix-icon="User"
            size="large"
          />
        </el-form-item>
        
        <el-form-item prop="password">
          <el-input
            v-model="loginForm.password"
            type="password"
            placeholder="请输入密码"
            :prefix-icon="Lock"
            size="large"
            show-password
          />
        </el-form-item>
        
        <el-form-item>
          <el-button
            type="primary"
            size="large"
            :loading="loading"
            class="login-button"
            native-type="submit"
          >
            登录
          </el-button>
        </el-form-item>
      </el-form>
      
      <div class="login-footer">
        <el-checkbox v-model="rememberMe">记住我</el-checkbox>
        <el-button link type="primary" @click="handleForgotPassword">
          忘记密码？
        </el-button>
      </div>
    </div>
    
    <div class="login-background">
      <div class="bg-circle circle-1"></div>
      <div class="bg-circle circle-2"></div>
      <div class="bg-circle circle-3"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/authStore'

const router = useRouter()
const authStore = useAuthStore()

const loginFormRef = ref()
const loading = ref(false)
const rememberMe = ref(false)

const loginForm = reactive({
  username: '',
  password: ''
})

// 初始化记住我功能
onMounted(() => {
  const savedUsername = localStorage.getItem('remembered-username')
  if (savedUsername) {
    loginForm.username = savedUsername
    rememberMe.value = true
  }
})

const loginRules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 20, message: '长度在 3 到 20 个字符', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, max: 20, message: '长度在 6 到 20 个字符', trigger: 'blur' }
  ]
}

const handleLogin = async () => {
  if (!loginFormRef.value) return
  
  try {
    await loginFormRef.value.validate()
    loading.value = true
    
    // 处理记住我功能
    if (rememberMe.value) {
      localStorage.setItem('remembered-username', loginForm.username)
    } else {
      localStorage.removeItem('remembered-username')
    }
    
    // 模拟登录请求
    await authStore.login(loginForm.username, loginForm.password)
    
    ElMessage.success('登录成功')
    router.push('/')
  } catch (error) {
    console.error('登录失败:', error)
  } finally {
    loading.value = false
  }
}

const handleForgotPassword = () => {
  ElMessage.info('密码重置功能开发中...')
}
</script>

<style scoped lang="scss">
.login-container {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  min-height: 100vh !important;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  position: relative !important;
  overflow: hidden !important;
  flex-direction: row !important;
}

.login-form {
  position: relative !important;
  z-index: 1 !important;
  width: 400px !important;
  padding: 40px !important;
  background: rgba(255, 255, 255, 0.95) !important;
  border-radius: 12px !important;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1) !important;
  backdrop-filter: blur(10px) !important;
}

.login-header {
  text-align: center !important;
  margin-bottom: 32px !important;
}

.login-logo {
  width: 64px !important;
  height: 64px !important;
  margin-bottom: 16px !important;
}

.login-title {
  margin: 0 0 8px 0 !important;
  font-size: 24px !important;
  color: var(--el-text-color-primary) !important;
}

.login-subtitle {
  margin: 0 !important;
  font-size: 14px !important;
  color: var(--el-text-color-secondary) !important;
}

.form-content {
  margin-bottom: 24px !important;
}

.login-button {
  width: 100% !important;
  font-size: 16px !important;
}

.login-footer {
  display: flex !important;
  justify-content: space-between !important;
  align-items: center !important;
}

.login-background {
  position: absolute !important;
  top: 0 !important;
  left: 0 !important;
  width: 100% !important;
  height: 100% !important;
  overflow: hidden !important;
}

.bg-circle {
  position: absolute !important;
  border-radius: 50% !important;
  background: rgba(255, 255, 255, 0.1) !important;
  animation: float 6s ease-in-out infinite !important;
}

.circle-1 {
  width: 200px !important;
  height: 200px !important;
  top: -100px !important;
  left: -100px !important;
  animation-delay: 0s !important;
}

.circle-2 {
  width: 300px !important;
  height: 300px !important;
  bottom: -150px !important;
  right: -150px !important;
  animation-delay: 2s !important;
}

.circle-3 {
  width: 150px !important;
  height: 150px !important;
  top: 50% !important;
  left: 80% !important;
  animation-delay: 4s !important;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px) rotate(0deg) !important;
  }
  50% {
    transform: translateY(-20px) rotate(180deg) !important;
  }
}

@media (max-width: 480px) {
  .login-form {
    width: 90% !important;
    padding: 30px 20px !important;
  }
}
</style>