<template>
  <div class="dashboard-container">
    <el-row :gutter="20" class="statistic-row">
      <el-col :span="6" v-for="item in statistics" :key="item.title">
        <el-card class="statistic-card" shadow="hover">
          <div class="statistic-content">
            <div class="statistic-icon" :style="{ backgroundColor: item.color }">
              <el-icon :size="24" color="white">
                <component :is="item.icon" />
              </el-icon>
            </div>
            <div class="statistic-info">
              <div class="statistic-value">{{ item.value }}</div>
              <div class="statistic-title">{{ item.title }}</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20" class="chart-row">
      <el-col :span="12">
        <el-card class="chart-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <span>用户增长趋势</span>
              <el-tag type="success">+12.5%</el-tag>
            </div>
          </template>
          <div class="chart-placeholder">
            <el-icon :size="48" color="#909399"><TrendCharts /></el-icon>
            <p>用户增长趋势图表</p>
          </div>
        </el-card>
      </el-col>
      
      <el-col :span="12">
        <el-card class="chart-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <span>系统访问统计</span>
              <el-tag type="primary">实时</el-tag>
            </div>
          </template>
          <div class="chart-placeholder">
            <el-icon :size="48" color="#909399"><DataAnalysis /></el-icon>
            <p>系统访问统计图表</p>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :span="24">
        <el-card class="todo-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <span>待办事项</span>
              <el-button type="primary" size="small" @click="handleAddTodo">
                添加事项
              </el-button>
            </div>
          </template>
          <el-table :data="todoList" style="width: 100%">
            <el-table-column prop="title" label="事项" />
            <el-table-column prop="priority" label="优先级" width="100">
              <template #default="{ row }">
                <el-tag :type="getPriorityType(row.priority)">
                  {{ row.priority }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="deadline" label="截止日期" width="120" />
            <el-table-column label="操作" width="120">
              <template #default="{ row }">
                <el-button 
                  link 
                  type="primary" 
                  size="small"
                  @click="handleComplete(row)"
                >
                  完成
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { ElMessage } from 'element-plus'
import { User, Goods, ShoppingCart, Money, TrendCharts, DataAnalysis } from '@element-plus/icons-vue'

interface TodoItem {
  title: string
  priority: '高' | '中' | '低'
  deadline: string
}

const statistics = ref([
  {
    title: '总用户数',
    value: '2,456',
    icon: 'User',
    color: '#409EFF'
  },
  {
    title: '商品总数',
    value: '1,234',
    icon: 'Goods',
    color: '#67C23A'
  },
  {
    title: '订单总数',
    value: '3,567',
    icon: 'ShoppingCart',
    color: '#E6A23C'
  },
  {
    title: '销售额',
    value: '¥89,234',
    icon: 'Money',
    color: '#F56C6C'
  }
])

const todoList = ref<TodoItem[]>([
  { title: '审核新用户注册申请', priority: '高', deadline: '2025-12-11' },
  { title: '更新系统文档', priority: '中', deadline: '2025-12-12' },
  { title: '备份数据库', priority: '高', deadline: '2025-12-13' },
  { title: '优化系统性能', priority: '低', deadline: '2025-12-15' }
])

const getPriorityType = (priority: string) => {
  const types: Record<string, string> = {
    '高': 'danger',
    '中': 'warning',
    '低': 'info'
  }
  return types[priority] || 'info'
}

const handleAddTodo = () => {
  ElMessage.info('添加待办事项功能开发中...')
}

const handleComplete = (row: TodoItem) => {
  const index = todoList.value.findIndex(item => item.title === row.title)
  if (index !== -1) {
    todoList.value.splice(index, 1)
    ElMessage.success('事项已完成')
  }
}
</script>

<style scoped lang="scss">
.dashboard-container {
  padding: 20px;
}

.statistic-row {
  margin-bottom: 20px;
}

.statistic-card {
  .statistic-content {
    display: flex;
    align-items: center;
    gap: 16px;
  }

  .statistic-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 48px;
    height: 48px;
    border-radius: 8px;
  }

  .statistic-info {
    flex: 1;
  }

  .statistic-value {
    font-size: 24px;
    font-weight: 600;
    color: var(--el-text-color-primary);
    margin-bottom: 4px;
  }

  .statistic-title {
    font-size: 14px;
    color: var(--el-text-color-secondary);
  }
}

.chart-row {
  margin-bottom: 20px;
}

.chart-card {
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .chart-placeholder {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 300px;
    color: #909399;
    background-color: var(--el-fill-color-light);
    border-radius: 4px;

    p {
      margin-top: 16px;
      font-size: 14px;
    }
  }
}

.todo-card {
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
}
</style>