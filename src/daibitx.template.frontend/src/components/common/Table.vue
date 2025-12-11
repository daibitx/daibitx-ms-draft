<template>
  <div class="table-container">
    <el-table
      v-loading="loading"
      :data="tableData"
      :border="border"
      :stripe="stripe"
      :size="size"
      :height="height"
      :max-height="maxHeight"
      :row-key="rowKey"
      @selection-change="handleSelectionChange"
      @sort-change="handleSortChange"
      @filter-change="handleFilterChange"
    >
      <!-- 选择列 -->
      <el-table-column
        v-if="showSelection"
        type="selection"
        width="55"
        :reserve-selection="reserveSelection"
      />
      
      <!-- 索引列 -->
      <el-table-column
        v-if="showIndex"
        type="index"
        :label="indexLabel"
        :width="indexWidth"
        :index="indexMethod"
      />
      
      <!-- 动态列 -->
      <el-table-column
        v-for="column in columns"
        :key="column.prop"
        :prop="column.prop"
        :label="column.label"
        :width="column.width"
        :min-width="column.minWidth"
        :fixed="column.fixed"
        :sortable="column.sortable"
        :filters="column.filters"
        :filter-method="column.filterMethod"
        :filter-multiple="column.filterMultiple !== false"
        :formatter="column.formatter"
        :show-overflow-tooltip="column.showOverflowTooltip !== false"
      >
        <template #header v-if="column.headerSlot">
          <slot :name="`${column.prop}-header`" :column="column">
            {{ column.label }}
          </slot>
        </template>
        
        <template #default="{ row, $index }">
          <!-- 自定义插槽 -->
          <slot
            v-if="column.slot"
            :name="column.prop"
            :row="row"
            :index="$index"
            :value="row[column.prop]"
          >
            {{ row[column.prop] }}
          </slot>
          
          <!-- 标签显示 -->
          <el-tag
            v-else-if="column.type === 'tag'"
            :type="getTagType(row, column)"
            :size="size"
          >
            {{ formatValue(row, column) }}
          </el-tag>
          
          <!-- 状态显示 -->
          <el-switch
            v-else-if="column.type === 'switch'"
            v-model="row[column.prop]"
            :active-value="column.activeValue"
            :inactive-value="column.inactiveValue"
            :disabled="column.disabled"
            @change="handleSwitchChange(row, column, $event)"
          />
          
          <!-- 图片显示 -->
          <el-image
            v-else-if="column.type === 'image'"
            :src="row[column.prop]"
            :preview-src-list="column.preview ? [row[column.prop]] : undefined"
            :style="column.imageStyle || { width: '40px', height: '40px' }"
            fit="cover"
          />
          
          <!-- 链接显示 -->
          <a
            v-else-if="column.type === 'link'"
            :href="row[column.prop]"
            target="_blank"
            class="table-link"
          >
            {{ formatValue(row, column) }}
          </a>
          
          <!-- 默认显示 -->
          <span v-else>
            {{ formatValue(row, column) }}
          </span>
        </template>
      </el-table-column>
      
      <!-- 操作列 -->
      <el-table-column
        v-if="showOperation"
        :label="operationLabel"
        :width="operationWidth"
        :fixed="operationFixed"
        :align="operationAlign"
      >
        <template #default="{ row, $index }">
          <slot name="operation" :row="row" :index="$index">
            <el-button
              v-if="showEdit"
              link
              type="primary"
              size="small"
              @click="handleEdit(row, $index)"
            >
              编辑
            </el-button>
            <el-button
              v-if="showDelete"
              link
              type="danger"
              size="small"
              @click="handleDelete(row, $index)"
            >
              删除
            </el-button>
          </slot>
        </template>
      </el-table-column>
    </el-table>
    
    <!-- 分页 -->
    <div v-if="showPagination" class="pagination-container">
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="pageSizes"
        :layout="paginationLayout"
        :background="paginationBackground"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import type { PropType } from 'vue'
import { ElMessage } from 'element-plus'

export interface TableColumn {
  prop: string
  label: string
  width?: string | number
  minWidth?: string | number
  fixed?: boolean | string
  sortable?: boolean | string
  filters?: Array<{ text: string; value: any }>
  filterMethod?: (value: any, row: any) => boolean
  filterMultiple?: boolean
  formatter?: (row: any, column: any, cellValue: any) => string
  showOverflowTooltip?: boolean
  slot?: boolean
  headerSlot?: boolean
  type?: 'tag' | 'switch' | 'image' | 'link'
  tagType?: string | ((row: any) => string)
  tagFormatter?: (row: any) => string
  activeValue?: any
  inactiveValue?: any
  disabled?: boolean
  preview?: boolean
  imageStyle?: any
}

const props = defineProps({
  // 表格数据
  data: {
    type: Array as PropType<any[]>,
    default: () => []
  },
  // 列配置
  columns: {
    type: Array as PropType<TableColumn[]>,
    required: true
  },
  // 加载状态
  loading: {
    type: Boolean,
    default: false
  },
  // 是否显示边框
  border: {
    type: Boolean,
    default: true
  },
  // 是否显示斑马纹
  stripe: {
    type: Boolean,
    default: true
  },
  // 表格尺寸
  size: {
    type: String as PropType<'large' | 'default' | 'small'>,
    default: 'default'
  },
  // 表格高度
  height: {
    type: [String, Number],
    default: undefined
  },
  // 最大高度
  maxHeight: {
    type: [String, Number],
    default: undefined
  },
  // 行数据的 Key
  rowKey: {
    type: String,
    default: 'id'
  },
  // 是否显示选择列
  showSelection: {
    type: Boolean,
    default: false
  },
  // 是否保留选择状态
  reserveSelection: {
    type: Boolean,
    default: false
  },
  // 是否显示索引列
  showIndex: {
    type: Boolean,
    default: true
  },
  // 索引列标签
  indexLabel: {
    type: String,
    default: '序号'
  },
  // 索引列宽度
  indexWidth: {
    type: [String, Number],
    default: 60
  },
  // 索引方法
  indexMethod: {
    type: Function,
    default: (index: number) => index + 1
  },
  // 是否显示操作列
  showOperation: {
    type: Boolean,
    default: true
  },
  // 操作列标签
  operationLabel: {
    type: String,
    default: '操作'
  },
  // 操作列宽度
  operationWidth: {
    type: [String, Number],
    default: 200
  },
  // 操作列固定
  operationFixed: {
    type: [Boolean, String],
    default: 'right'
  },
  // 操作列对齐方式
  operationAlign: {
    type: String as PropType<'left' | 'center' | 'right'>,
    default: 'center'
  },
  // 是否显示编辑按钮
  showEdit: {
    type: Boolean,
    default: true
  },
  // 是否显示删除按钮
  showDelete: {
    type: Boolean,
    default: true
  },
  // 是否显示分页
  showPagination: {
    type: Boolean,
    default: true
  },
  // 总条数
  total: {
    type: Number,
    default: 0
  },
  // 当前页
  currentPage: {
    type: Number,
    default: 1
  },
  // 每页条数
  pageSize: {
    type: Number,
    default: 10
  },
  // 每页条数选项
  pageSizes: {
    type: Array as PropType<number[]>,
    default: () => [10, 20, 50, 100]
  },
  // 分页布局
  paginationLayout: {
    type: String,
    default: 'total, sizes, prev, pager, next, jumper'
  },
  // 分页背景色
  paginationBackground: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits([
  'update:currentPage',
  'update:pageSize',
  'selection-change',
  'sort-change',
  'filter-change',
  'edit',
  'delete',
  'switch-change',
  'size-change',
  'current-change'
])

// 本地数据
const tableData = ref<any[]>([])
const currentPage = ref(props.currentPage)
const pageSize = ref(props.pageSize)

// 监听数据变化
watch(() => props.data, (newData) => {
  tableData.value = newData
}, { immediate: true })

// 监听分页变化
watch(() => props.currentPage, (val) => {
  currentPage.value = val
})

watch(() => props.pageSize, (val) => {
  pageSize.value = val
})

// 格式化值
const formatValue = (row: any, column: TableColumn) => {
  if (column.formatter) {
    return column.formatter(row, column, row[column.prop])
  }
  if (column.tagFormatter) {
    return column.tagFormatter(row)
  }
  return row[column.prop]
}

// 获取标签类型
const getTagType = (row: any, column: TableColumn) => {
  if (typeof column.tagType === 'function') {
    return column.tagType(row)
  }
  return column.tagType || 'info'
}

// 处理选择变化
const handleSelectionChange = (selection: any[]) => {
  emit('selection-change', selection)
}

// 处理排序变化
const handleSortChange = ({ column, prop, order }: any) => {
  emit('sort-change', { column, prop, order })
}

// 处理筛选变化
const handleFilterChange = (filters: any) => {
  emit('filter-change', filters)
}

// 处理编辑
const handleEdit = (row: any, index: number) => {
  emit('edit', row, index)
}

// 处理删除
const handleDelete = (row: any, index: number) => {
  emit('delete', row, index)
}

// 处理开关变化
const handleSwitchChange = (row: any, column: TableColumn, value: any) => {
  emit('switch-change', { row, column, value })
}

// 处理每页条数变化
const handleSizeChange = (size: number) => {
  emit('update:pageSize', size)
  emit('size-change', size)
}

// 处理当前页变化
const handleCurrentChange = (page: number) => {
  emit('update:currentPage', page)
  emit('current-change', page)
}
</script>

<style scoped lang="scss">
.table-container {
  .pagination-container {
    margin-top: 20px;
    display: flex;
    justify-content: flex-end;
  }
}

.table-link {
  color: var(--el-color-primary);
  text-decoration: none;
  
  &:hover {
    text-decoration: underline;
  }
}
</style>