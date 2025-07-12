<template>
  <div class="led-container">
    <div 
      class="led"
      :class="{ 'led-on': isOn }"
      :style="{ backgroundColor: isOn ? color : '#ccc' }"
    >
      <div class="led-light" v-if="isOn"></div>
    </div>
    <div class="led-label">{{ label }}</div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  isOn: boolean
  color?: string
  label?: string
}

withDefaults(defineProps<Props>(), {
  color: '#00ff00',
  label: 'LED'
})
</script>

<style scoped>
.led-container {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.led {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: 2px solid #999;
  position: relative;
  transition: all 0.3s ease;
}

.led-on {
  box-shadow: 0 0 10px currentColor;
}

.led-light {
  position: absolute;
  top: 4px;
  left: 4px;
  width: 8px;
  height: 8px;
  background: rgba(255, 255, 255, 0.8);
  border-radius: 50%;
}

.led-label {
  font-size: 12px;
  color: #666;
}
</style>
