<template>
  <div class="digital-display-test example-page">
    <div class="page-header">
      <h1>数码管显示测试</h1>
      <p class="description">
        测试数码管显示控件的基本功能
      </p>
    </div>

    <div class="example-section">
      <h2 class="section-title">基础数码管测试</h2>
      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>简单数码管</span>
                <el-button size="small" @click="updateValue">
                  <el-icon><Refresh /></el-icon>
                  更新数值
                </el-button>
              </div>
            </template>
            <div class="display-container">
              <DigitalDisplay
                :data="testData"
                :options="testOptions"
                @value-change="handleValueChange"
              />
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="12">
          <el-card>
            <template #header>
              <span>控制面板</span>
            </template>
            <div class="controls">
              <div class="control-item">
                <label>数值:</label>
                <el-input-number
                  v-model="testData.value"
                  :precision="1"
                  :step="0.1"
                  :min="0"
                  :max="9999"
                />
              </div>
              
              <div class="control-item">
                <label>数字位数:</label>
                <el-slider
                  v-model="testOptions.digitCount"
                  :min="1"
                  :max="8"
                  :step="1"
                />
              </div>
              
              <div class="control-item">
                <label>小数位数:</label>
                <el-slider
                  v-model="testOptions.decimalPlaces"
                  :min="0"
                  :max="4"
                  :step="1"
                />
              </div>
              
              <div class="control-item">
                <label>大小:</label>
                <el-slider
                  v-model="testOptions.size"
                  :min="30"
                  :max="100"
                  :step="5"
                />
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Refresh } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import DigitalDisplay from '@/components/professional/indicators/DigitalDisplay.vue'

// 测试数据
const testData = ref({
  value: 123.4,
  title: '测试数码管',
  unit: ''
})

const testOptions = ref({
  digitCount: 4,
  decimalPlaces: 1,
  size: 60,
  color: '#00ff00',
  backgroundColor: '#000000',
  segmentColor: '#003300',
  activeColor: '#00ff00',
  showControls: false,
  blinkInterval: 500,
  padding: 20,
  borderRadius: 8
})

// 方法
const updateValue = () => {
  testData.value.value = Math.random() * 999.9
  ElMessage.info(`数值更新为: ${testData.value.value.toFixed(1)}`)
}

const handleValueChange = (value: number) => {
  ElMessage.info(`数值变更为: ${value}`)
}
</script>

<style lang="scss" scoped>
.digital-display-test {
  .display-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 200px;
    background: #f5f7fa;
    border-radius: 8px;
    margin: 20px 0;
  }
  
  .controls {
    .control-item {
      display: flex;
      align-items: center;
      margin-bottom: 16px;
      
      label {
        width: 80px;
        font-size: 14px;
        color: #606266;
        margin-right: 12px;
      }
    }
  }
  
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
}
</style>
