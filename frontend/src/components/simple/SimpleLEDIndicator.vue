<template>
  <div class="simple-led-indicator" :style="containerStyle">
    <div class="led-container">
      <div 
        class="led-light"
        :class="{ 'led-on': state, 'led-off': !state }"
        :style="ledStyle"
        @click="toggle"
      >
        <div class="led-glow" v-if="state"></div>
      </div>
      <span v-if="label" class="led-label">{{ label }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  state?: boolean
  color?: string
  size?: number
  label?: string
  clickable?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  state: false,
  color: 'green',
  size: 20,
  label: '',
  clickable: true
})

const emit = defineEmits<{
  'update:state': [state: boolean]
  'click': [state: boolean]
}>()

const containerStyle = computed(() => ({
  display: 'inline-flex',
  alignItems: 'center',
  gap: '8px'
}))

const ledStyle = computed(() => ({
  width: `${props.size}px`,
  height: `${props.size}px`,
  backgroundColor: props.state ? props.color : '#ccc',
  cursor: props.clickable ? 'pointer' : 'default'
}))

const toggle = () => {
  if (props.clickable) {
    const newState = !props.state
    emit('update:state', newState)
    emit('click', newState)
  }
}
</script>

<style scoped>
.simple-led-indicator {
  user-select: none;
}

.led-container {
  display: flex;
  align-items: center;
  gap: 8px;
}

.led-light {
  border-radius: 50%;
  border: 2px solid #666;
  position: relative;
  transition: all 0.3s ease;
  box-shadow: inset 0 2px 4px rgba(0,0,0,0.2);
}

.led-light.led-on {
  box-shadow: 
    inset 0 2px 4px rgba(0,0,0,0.2),
    0 0 10px currentColor,
    0 0 20px currentColor;
}

.led-glow {
  position: absolute;
  top: 20%;
  left: 20%;
  width: 30%;
  height: 30%;
  background: rgba(255,255,255,0.8);
  border-radius: 50%;
  filter: blur(1px);
}

.led-label {
  font-size: 14px;
  color: #333;
  font-weight: 500;
}
</style>
