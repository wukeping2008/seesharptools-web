<template>
  <button 
    class="industrial-button"
    :class="{ 'button-pressed': isPressed }"
    @mousedown="handleMouseDown"
    @mouseup="handleMouseUp"
    @mouseleave="handleMouseUp"
  >
    <span class="button-text">{{ text }}</span>
  </button>
</template>

<script setup lang="ts">
import { ref } from 'vue'

interface Props {
  text?: string
}

const props = withDefaults(defineProps<Props>(), {
  text: '按钮'
})

const emit = defineEmits<{
  click: []
}>()

const isPressed = ref(false)

const handleMouseDown = () => {
  isPressed.value = true
}

const handleMouseUp = () => {
  if (isPressed.value) {
    emit('click')
  }
  isPressed.value = false
}
</script>

<style scoped>
.industrial-button {
  padding: 12px 24px;
  background: linear-gradient(135deg, #f0f0f0 0%, #d0d0d0 100%);
  border: 2px solid #999;
  border-radius: 4px;
  cursor: pointer;
  user-select: none;
  transition: all 0.1s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.industrial-button:hover {
  background: linear-gradient(135deg, #f5f5f5 0%, #d5d5d5 100%);
}

.button-pressed {
  background: linear-gradient(135deg, #d0d0d0 0%, #b0b0b0 100%);
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.3);
  transform: translateY(1px);
}

.button-text {
  font-size: 14px;
  font-weight: 600;
  color: #333;
}
</style>
