<template>
  <div class="waveform-display">
    <canvas ref="canvas" :width="width" :height="height"></canvas>
    <div class="waveform-controls">
      <div class="control-group">
        <label>Y Scale</label>
        <input type="range" v-model="yScale" min="0.1" max="10" step="0.1" />
        <span>{{ yScale.toFixed(1) }}x</span>
      </div>
      <div class="control-group">
        <label>Time Scale</label>
        <input type="range" v-model="timeScale" min="0.5" max="5" step="0.1" />
        <span>{{ timeScale.toFixed(1) }}x</span>
      </div>
      <div class="control-group">
        <label>Grid</label>
        <input type="checkbox" v-model="showGrid" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';

interface Props {
  width?: number;
  height?: number;
  channels?: ChannelConfig[];
  maxPoints?: number;
}

interface ChannelConfig {
  id: number;
  name: string;
  color: string;
  enabled: boolean;
  unit: string;
  range: number;
}

const props = withDefaults(defineProps<Props>(), {
  width: 800,
  height: 400,
  maxPoints: 1000,
  channels: () => [
    { id: 0, name: 'Ch 0', color: '#00ff00', enabled: true, unit: 'V', range: 10 }
  ]
});

const canvas = ref<HTMLCanvasElement>();
const ctx = ref<CanvasRenderingContext2D | null>(null);
const dataBuffers = ref<Map<number, number[]>>(new Map());
const yScale = ref(1.0);
const timeScale = ref(1.0);
const showGrid = ref(true);

let animationFrame: number | null = null;

onMounted(() => {
  if (canvas.value) {
    ctx.value = canvas.value.getContext('2d');
    if (ctx.value) {
      // Set up canvas for smooth drawing
      ctx.value.imageSmoothingEnabled = true;
      ctx.value.lineJoin = 'round';
      ctx.value.lineCap = 'round';
    }
    
    // Initialize data buffers for each channel
    props.channels.forEach(ch => {
      dataBuffers.value.set(ch.id, []);
    });
    
    startAnimation();
  }
});

onUnmounted(() => {
  if (animationFrame) {
    cancelAnimationFrame(animationFrame);
  }
});

/**
 * Add new data points to the display
 */
function addData(channelId: number, values: number[]) {
  const buffer = dataBuffers.value.get(channelId);
  if (!buffer) return;
  
  // Add new values
  buffer.push(...values);
  
  // Limit buffer size based on timeScale
  const maxBufferSize = Math.floor(props.maxPoints * timeScale.value);
  if (buffer.length > maxBufferSize) {
    buffer.splice(0, buffer.length - maxBufferSize);
  }
}

/**
 * Clear all data
 */
function clearData() {
  dataBuffers.value.forEach(buffer => buffer.length = 0);
}

/**
 * Draw the waveform
 */
function draw() {
  if (!ctx.value || !canvas.value) return;
  
  const width = canvas.value.width;
  const height = canvas.value.height;
  
  // Clear canvas
  ctx.value.fillStyle = '#000000';
  ctx.value.fillRect(0, 0, width, height);
  
  // Draw grid if enabled
  if (showGrid.value) {
    drawGrid();
  }
  
  // Draw each channel
  props.channels.forEach(channel => {
    if (!channel.enabled) return;
    
    const data = dataBuffers.value.get(channel.id);
    if (!data || data.length < 2) return;
    
    drawChannel(channel, data);
  });
  
  // Draw info overlay
  drawInfo();
}

/**
 * Draw grid lines
 */
function drawGrid() {
  if (!ctx.value || !canvas.value) return;
  
  const width = canvas.value.width;
  const height = canvas.value.height;
  
  ctx.value.strokeStyle = '#333333';
  ctx.value.lineWidth = 1;
  ctx.value.setLineDash([2, 2]);
  
  // Vertical lines (time divisions)
  const timeDiv = 10;
  for (let i = 0; i <= timeDiv; i++) {
    const x = (i / timeDiv) * width;
    ctx.value.beginPath();
    ctx.value.moveTo(x, 0);
    ctx.value.lineTo(x, height);
    ctx.value.stroke();
  }
  
  // Horizontal lines (voltage divisions)
  const voltDiv = 8;
  for (let i = 0; i <= voltDiv; i++) {
    const y = (i / voltDiv) * height;
    ctx.value.beginPath();
    ctx.value.moveTo(0, y);
    ctx.value.lineTo(width, y);
    ctx.value.stroke();
  }
  
  // Center lines
  ctx.value.strokeStyle = '#555555';
  ctx.value.setLineDash([]);
  
  // Center horizontal
  ctx.value.beginPath();
  ctx.value.moveTo(0, height / 2);
  ctx.value.lineTo(width, height / 2);
  ctx.value.stroke();
  
  // Center vertical
  ctx.value.beginPath();
  ctx.value.moveTo(width / 2, 0);
  ctx.value.lineTo(width / 2, height);
  ctx.value.stroke();
  
  ctx.value.setLineDash([]);
}

/**
 * Draw a single channel
 */
function drawChannel(channel: ChannelConfig, data: number[]) {
  if (!ctx.value || !canvas.value) return;
  
  const width = canvas.value.width;
  const height = canvas.value.height;
  const centerY = height / 2;
  
  ctx.value.strokeStyle = channel.color;
  ctx.value.lineWidth = 2;
  ctx.value.setLineDash([]);
  
  ctx.value.beginPath();
  
  for (let i = 0; i < data.length; i++) {
    const x = (i / data.length) * width;
    const normalizedValue = (data[i] / channel.range) * yScale.value;
    const y = centerY - (normalizedValue * height / 2);
    
    if (i === 0) {
      ctx.value.moveTo(x, y);
    } else {
      ctx.value.lineTo(x, y);
    }
  }
  
  ctx.value.stroke();
}

/**
 * Draw info overlay
 */
function drawInfo() {
  if (!ctx.value || !canvas.value) return;
  
  ctx.value.fillStyle = '#ffffff';
  ctx.value.font = '12px monospace';
  
  let yPos = 20;
  props.channels.forEach(channel => {
    if (!channel.enabled) return;
    
    const data = dataBuffers.value.get(channel.id);
    if (!data || data.length === 0) return;
    
    const lastValue = data[data.length - 1];
    const text = `${channel.name}: ${lastValue.toFixed(3)} ${channel.unit}`;
    
    // Draw background for better readability
    ctx.value.fillStyle = 'rgba(0, 0, 0, 0.7)';
    const metrics = ctx.value.measureText(text);
    ctx.value.fillRect(10, yPos - 12, metrics.width + 10, 16);
    
    // Draw text
    ctx.value.fillStyle = channel.color;
    ctx.value.fillText(text, 15, yPos);
    
    yPos += 20;
  });
}

/**
 * Animation loop
 */
function startAnimation() {
  const animate = () => {
    draw();
    animationFrame = requestAnimationFrame(animate);
  };
  animate();
}

// Watch for scale changes
watch([yScale, timeScale, showGrid], () => {
  draw();
});

// Expose methods for parent component
defineExpose({
  addData,
  clearData
});
</script>

<style scoped>
.waveform-display {
  background: #1a1a1a;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 1rem;
}

canvas {
  display: block;
  width: 100%;
  background: #000;
  border: 1px solid #333;
  border-radius: 4px;
}

.waveform-controls {
  display: flex;
  gap: 2rem;
  margin-top: 1rem;
  padding: 0.5rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 4px;
}

.control-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.control-group label {
  color: #999;
  font-size: 0.875rem;
  font-weight: 500;
}

.control-group input[type="range"] {
  width: 100px;
}

.control-group input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.control-group span {
  color: #fff;
  font-size: 0.875rem;
  font-family: monospace;
  min-width: 40px;
}
</style>