<template>
  <div class="test-history-container">
    <!-- Header -->
    <div class="header-section">
      <div class="title-row">
        <h2>测试历史记录</h2>
        <div class="header-actions">
          <el-button type="primary" @click="refreshData" :loading="loading">
            <el-icon><Refresh /></el-icon>
            刷新
          </el-button>
          <el-button @click="exportHistory" :loading="exporting">
            <el-icon><Download /></el-icon>
            导出CSV
          </el-button>
        </div>
      </div>

      <!-- Statistics Cards -->
      <div class="stats-cards" v-if="statistics">
        <el-card class="stat-card">
          <div class="stat-content">
            <div class="stat-number">{{ statistics.totalExecutions }}</div>
            <div class="stat-label">总执行次数</div>
          </div>
        </el-card>
        <el-card class="stat-card">
          <div class="stat-content">
            <div class="stat-number success">{{ statistics.successfulExecutions }}</div>
            <div class="stat-label">成功次数</div>
          </div>
        </el-card>
        <el-card class="stat-card">
          <div class="stat-content">
            <div class="stat-number">{{ statistics.successRate.toFixed(1) }}%</div>
            <div class="stat-label">成功率</div>
          </div>
        </el-card>
        <el-card class="stat-card">
          <div class="stat-content">
            <div class="stat-number">{{ statistics.averageCodeQuality.toFixed(1) }}</div>
            <div class="stat-label">平均代码质量</div>
          </div>
        </el-card>
      </div>
    </div>

    <!-- Filters -->
    <el-card class="filter-card">
      <div class="filter-section">
        <el-row :gutter="16">
          <el-col :span="6">
            <el-select v-model="filters.deviceType" placeholder="设备类型" clearable>
              <el-option label="全部" value="" />
              <el-option 
                v-for="device in deviceTypes" 
                :key="device" 
                :label="device" 
                :value="device" 
              />
            </el-select>
          </el-col>
          <el-col :span="6">
            <el-select v-model="filters.testType" placeholder="测试类型" clearable>
              <el-option label="全部" value="" />
              <el-option 
                v-for="test in testTypes" 
                :key="test" 
                :label="test" 
                :value="test" 
              />
            </el-select>
          </el-col>
          <el-col :span="6">
            <el-select v-model="filters.aiProvider" placeholder="AI提供商" clearable>
              <el-option label="全部" value="" />
              <el-option 
                v-for="provider in aiProviders" 
                :key="provider" 
                :label="provider" 
                :value="provider" 
              />
            </el-select>
          </el-col>
          <el-col :span="6">
            <el-select v-model="filters.success" placeholder="执行状态" clearable>
              <el-option label="全部" value="" />
              <el-option label="成功" :value="true" />
              <el-option label="失败" :value="false" />
            </el-select>
          </el-col>
        </el-row>
        <el-row :gutter="16" style="margin-top: 16px;">
          <el-col :span="8">
            <el-date-picker
              v-model="dateRange"
              type="datetimerange"
              range-separator="至"
              start-placeholder="开始时间"
              end-placeholder="结束时间"
              format="YYYY-MM-DD HH:mm:ss"
              value-format="YYYY-MM-DD HH:mm:ss"
            />
          </el-col>
          <el-col :span="6">
            <el-input-number
              v-model="filters.minCodeQualityScore"
              placeholder="最低代码质量"
              :min="0"
              :max="100"
              :precision="0"
            />
          </el-col>
          <el-col :span="6">
            <el-select v-model="filters.sortOrder" placeholder="排序方式">
              <el-option label="最新优先" :value="0" />
              <el-option label="最旧优先" :value="1" />
              <el-option label="质量评分高->低" :value="2" />
              <el-option label="质量评分低->高" :value="3" />
              <el-option label="执行时间长->短" :value="4" />
              <el-option label="执行时间短->长" :value="5" />
            </el-select>
          </el-col>
          <el-col :span="4">
            <el-button type="primary" @click="applyFilters" :loading="loading">
              <el-icon><Search /></el-icon>
              搜索
            </el-button>
          </el-col>
        </el-row>
      </div>
    </el-card>

    <!-- Execution History Table -->
    <el-card class="table-card">
      <el-table 
        :data="executions" 
        v-loading="loading"
        @row-click="viewExecutionDetails"
        style="cursor: pointer;"
      >
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="testRequirement" label="测试需求" width="200">
          <template #default="{ row }">
            <el-tooltip :content="row.testRequirement" placement="top">
              <div class="truncate">{{ row.testRequirement }}</div>
            </el-tooltip>
          </template>
        </el-table-column>
        <el-table-column prop="deviceType" label="设备类型" width="120" />
        <el-table-column prop="testType" label="测试类型" width="120" />
        <el-table-column prop="aiProvider" label="AI提供商" width="100" />
        <el-table-column prop="success" label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.success ? 'success' : 'danger'">
              {{ row.success ? '成功' : '失败' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="codeQualityScore" label="代码质量" width="100">
          <template #default="{ row }">
            <el-progress 
              :percentage="row.codeQualityScore || 0" 
              :format="() => `${row.codeQualityScore || 0}`"
              :stroke-width="8"
            />
          </template>
        </el-table-column>
        <el-table-column prop="executionTimeMs" label="执行时间" width="100">
          <template #default="{ row }">
            {{ row.executionTimeMs ? `${row.executionTimeMs}ms` : '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="tokensUsed" label="Token使用" width="100" />
        <el-table-column prop="createdAt" label="创建时间" width="180">
          <template #default="{ row }">
            {{ formatDateTime(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click.stop="viewDetails(row)">
              <el-icon><View /></el-icon>
            </el-button>
            <el-button 
              size="small" 
              type="danger" 
              @click.stop="deleteExecution(row)"
            >
              <el-icon><Delete /></el-icon>
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- Pagination -->
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="totalExecutions"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </el-card>

    <!-- Execution Details Dialog -->
    <el-dialog 
      v-model="detailsDialogVisible" 
      title="执行详情" 
      width="80%"
      :close-on-click-modal="false"
    >
      <div v-if="selectedExecution" class="execution-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="ID">{{ selectedExecution.id }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="selectedExecution.success ? 'success' : 'danger'">
              {{ selectedExecution.success ? '成功' : '失败' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="设备类型">{{ selectedExecution.deviceType || '-' }}</el-descriptions-item>
          <el-descriptions-item label="测试类型">{{ selectedExecution.testType || '-' }}</el-descriptions-item>
          <el-descriptions-item label="AI提供商">{{ selectedExecution.aiProvider || '-' }}</el-descriptions-item>
          <el-descriptions-item label="代码质量评分">{{ selectedExecution.codeQualityScore || '-' }}</el-descriptions-item>
          <el-descriptions-item label="执行时间">{{ selectedExecution.executionTimeMs ? `${selectedExecution.executionTimeMs}ms` : '-' }}</el-descriptions-item>
          <el-descriptions-item label="Token使用">{{ selectedExecution.tokensUsed || '-' }}</el-descriptions-item>
          <el-descriptions-item label="创建时间" :span="2">{{ formatDateTime(selectedExecution.createdAt) }}</el-descriptions-item>
        </el-descriptions>

        <div class="detail-section">
          <h4>测试需求</h4>
          <el-input 
            :model-value="selectedExecution.testRequirement" 
            type="textarea" 
            :rows="3" 
            readonly 
          />
        </div>

        <div class="detail-section" v-if="selectedExecution.errorMessage">
          <h4>错误信息</h4>
          <el-alert :title="selectedExecution.errorMessage" type="error" :closable="false" />
        </div>

        <div class="detail-section">
          <h4>生成的代码</h4>
          <el-input 
            :model-value="selectedExecution.generatedCode" 
            type="textarea" 
            :rows="10" 
            readonly 
          />
        </div>

        <div class="detail-section" v-if="selectedExecution.resultData">
          <h4>执行结果</h4>
          <el-input 
            :model-value="selectedExecution.resultData" 
            type="textarea" 
            :rows="5" 
            readonly 
          />
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Refresh, Download, Search, View, Delete } from '@element-plus/icons-vue'
import { TestHistoryService } from '@/services/TestHistoryService'
import type { 
  TestExecutionRecord, 
  TestExecutionHistoryQuery, 
  TestExecutionStatistics,
  TestExecutionSortOrder
} from '@/types/test-execution'

// Data
const loading = ref(false)
const exporting = ref(false)
const executions = ref<TestExecutionRecord[]>([])
const statistics = ref<TestExecutionStatistics | null>(null)
const detailsDialogVisible = ref(false)
const selectedExecution = ref<TestExecutionRecord | null>(null)

// Pagination
const currentPage = ref(1)
const pageSize = ref(20)
const totalExecutions = ref(0)

// Filters
const filters = reactive({
  deviceType: '',
  testType: '',
  aiProvider: '',
  success: '' as boolean | '',
  minCodeQualityScore: 0,
  sortOrder: 0 as TestExecutionSortOrder
})

const dateRange = ref<[string, string] | null>(null)

// Computed properties
const deviceTypes = computed(() => {
  if (!statistics.value) return []
  return Object.keys(statistics.value.executionsByDeviceType)
})

const testTypes = computed(() => {
  if (!statistics.value) return []
  return Object.keys(statistics.value.executionsByTestType)
})

const aiProviders = computed(() => {
  if (!statistics.value) return []
  return Object.keys(statistics.value.executionsByAIProvider)
})

// Methods
const loadExecutions = async () => {
  loading.value = true
  try {
    const query: TestExecutionHistoryQuery = {
      maxCount: pageSize.value,
      sortOrder: filters.sortOrder
    }

    if (filters.deviceType) query.deviceType = filters.deviceType
    if (filters.testType) query.testType = filters.testType
    if (filters.aiProvider) query.aiProvider = filters.aiProvider
    if (filters.success !== '') query.success = filters.success as boolean
    if (filters.minCodeQualityScore > 0) query.minCodeQualityScore = filters.minCodeQualityScore
    if (dateRange.value) {
      query.startDate = dateRange.value[0]
      query.endDate = dateRange.value[1]
    }

    const results = await TestHistoryService.getExecutionHistory(query)
    executions.value = results
    totalExecutions.value = results.length // In real implementation, this should come from API
  } catch (error) {
    ElMessage.error(`加载执行记录失败: ${error}`)
  } finally {
    loading.value = false
  }
}

const loadStatistics = async () => {
  try {
    const endDate = new Date()
    const startDate = new Date()
    startDate.setDate(startDate.getDate() - 30) // Last 30 days

    statistics.value = await TestHistoryService.getExecutionStatistics(startDate, endDate)
  } catch (error) {
    ElMessage.error(`加载统计数据失败: ${error}`)
  }
}

const refreshData = async () => {
  await Promise.all([loadExecutions(), loadStatistics()])
}

const applyFilters = () => {
  currentPage.value = 1
  loadExecutions()
}

const handleSizeChange = (newSize: number) => {
  pageSize.value = newSize
  loadExecutions()
}

const handleCurrentChange = (newPage: number) => {
  currentPage.value = newPage
  loadExecutions()
}

const viewExecutionDetails = (row: TestExecutionRecord) => {
  viewDetails(row)
}

const viewDetails = (execution: TestExecutionRecord) => {
  selectedExecution.value = execution
  detailsDialogVisible.value = true
}

const deleteExecution = async (execution: TestExecutionRecord) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除执行记录 ${execution.id} 吗？此操作不可恢复。`,
      '确认删除',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      }
    )

    await TestHistoryService.deleteExecutionRecord(execution.id)
    ElMessage.success('删除成功')
    await refreshData()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(`删除失败: ${error}`)
    }
  }
}

const exportHistory = async () => {
  exporting.value = true
  try {
    const query: TestExecutionHistoryQuery = {
      maxCount: 10000, // Export more records
      sortOrder: filters.sortOrder
    }

    if (filters.deviceType) query.deviceType = filters.deviceType
    if (filters.testType) query.testType = filters.testType
    if (filters.aiProvider) query.aiProvider = filters.aiProvider
    if (filters.success !== '') query.success = filters.success as boolean
    if (filters.minCodeQualityScore > 0) query.minCodeQualityScore = filters.minCodeQualityScore
    if (dateRange.value) {
      query.startDate = dateRange.value[0]
      query.endDate = dateRange.value[1]
    }

    const blob = await TestHistoryService.exportExecutionHistory(query)
    
    // Download file
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `test-history-${new Date().toISOString().split('T')[0]}.csv`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

    ElMessage.success('导出成功')
  } catch (error) {
    ElMessage.error(`导出失败: ${error}`)
  } finally {
    exporting.value = false
  }
}

const formatDateTime = (dateString: string) => {
  return new Date(dateString).toLocaleString('zh-CN')
}

// Lifecycle
onMounted(() => {
  refreshData()
})
</script>

<style scoped>
.test-history-container {
  padding: 20px;
}

.header-section {
  margin-bottom: 20px;
}

.title-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.title-row h2 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.stats-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 20px;
}

.stat-card {
  text-align: center;
}

.stat-content {
  padding: 20px;
}

.stat-number {
  font-size: 28px;
  font-weight: bold;
  color: #409eff;
  margin-bottom: 8px;
}

.stat-number.success {
  color: #67c23a;
}

.stat-label {
  font-size: 14px;
  color: #909399;
}

.filter-card {
  margin-bottom: 20px;
}

.filter-section {
  padding: 20px;
}

.table-card {
  margin-bottom: 20px;
}

.truncate {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 180px;
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

.execution-details {
  padding: 20px;
}

.detail-section {
  margin: 20px 0;
}

.detail-section h4 {
  margin-bottom: 10px;
  color: #303133;
  font-weight: 600;
}

:deep(.el-table tbody tr:hover > td) {
  background-color: #f5f7fa !important;
}

:deep(.el-table tbody tr) {
  cursor: pointer;
}
</style>