<template>
  <div class="linear-gauge-test">
    <h1>线性仪表测试</h1>
    
    <div style="margin: 20px 0;">
      <h3>水平线性仪表</h3>
      <LinearGauge
        :data="horizontalData"
        :options="horizontalOptions"
        :height="150"
      />
    </div>
    
    <div style="margin: 20px 0;">
      <h3>垂直线性仪表</h3>
      <LinearGauge
        :data="verticalData"
        :options="verticalOptions"
        :height="250"
      />
    </div>
    
    <div style="margin: 20px 0;">
      <button @click="updateValues">更新数值</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import LinearGauge from '@/components/professional/gauges/LinearGauge.vue'
import type { GaugeData, LinearGaugeOptions } from '@/types/gauge'

const horizontalData = ref<GaugeData>({
  value: 45,
  min: 0,
  max: 100,
  unit: '%',
  title: '水平进度'
})

const horizontalOptions = ref<Partial<LinearGaugeOptions>>({
  orientation: 'horizontal',
  width: 20,
  showScale: true,
  showPointer: true,
  showProgress: true,
  animation: true,
  theme: 'light'
})

const verticalData = ref<GaugeData>({
  value: 30,
  min: 0,
  max: 100,
  unit: 'L',
  title: '液位高度'
})

const verticalOptions = ref<Partial<LinearGaugeOptions>>({
  orientation: 'vertical',
  width: 25,
  showScale: true,
  showPointer: true,
  showProgress: true,
  animation: true,
  theme: 'light'
})

const updateValues = () => {
  horizontalData.value.value = Math.random() * 100
  verticalData.value.value = Math.random() * 100
}
</script>

<style scoped>
.linear-gauge-test {
  padding: 20px;
}
</style>
