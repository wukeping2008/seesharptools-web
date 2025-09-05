<template>
  <div class="container fade-in">
    <div class="hero">
      <h1 class="hero-title gradient-text">{{ t('hero.dashboardTitle') }}</h1>
      <p class="hero-subtitle">{{ t('hero.dashboardSubtitle') }}</p>
    </div>

    <div class="dashboard-grid">
      <!-- Connection Status Card -->
      <div class="status-card card">
        <div class="card-header">
          <h3>{{ t('dashboard.connectionStatus') }}</h3>
          <div class="status-indicator" :class="connectionStatus">
            <span class="status-dot"></span>
            <span class="status-text">{{ connectionStatusText }}</span>
          </div>
        </div>
        <div class="status-details">
          <div class="detail-row">
            <span class="detail-label">{{ t('dashboard.server') }}:</span>
            <span class="detail-value">{{ serverUrl }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">{{ t('dashboard.uptime') }}:</span>
            <span class="detail-value">{{ uptime }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">{{ t('dashboard.messages') }}:</span>
            <span class="detail-value">{{ messageCount }}</span>
          </div>
        </div>
        <button @click="toggleConnection" class="btn-primary full-width">
          {{ isConnected ? t('dashboard.disconnect') : t('dashboard.connect') }}
        </button>
      </div>

      <!-- Data Stream Card -->
      <div class="stream-card card">
        <div class="card-header">
          <h3>{{ t('dashboard.liveDataStream') }}</h3>
          <button @click="clearStream" class="btn-secondary">{{ t('dashboard.clear') }}</button>
        </div>
        <div class="stream-container">
          <div v-for="(message, index) in dataStream" :key="index" class="stream-message fade-in">
            <span class="timestamp">{{ message.timestamp }}</span>
            <span class="message-type" :class="`type-${message.type}`">{{ message.type }}</span>
            <span class="message-content">{{ message.content }}</span>
          </div>
          <div v-if="dataStream.length === 0" class="empty-state">
            <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" opacity="0.3">
              <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"></path>
            </svg>
            <p>{{ t('dashboard.noData') }}</p>
          </div>
        </div>
      </div>

      <!-- Statistics Card -->
      <div class="stats-card card">
        <div class="card-header">
          <h3>{{ t('dashboard.statistics') }}</h3>
        </div>
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-value">{{ avgValue.toFixed(2) }}</div>
            <div class="stat-label">{{ t('dashboard.average') }}</div>
          </div>
          <div class="stat-item">
            <div class="stat-value">{{ maxValue.toFixed(2) }}</div>
            <div class="stat-label">{{ t('dashboard.maximum') }}</div>
          </div>
          <div class="stat-item">
            <div class="stat-value">{{ minValue.toFixed(2) }}</div>
            <div class="stat-label">{{ t('dashboard.minimum') }}</div>
          </div>
          <div class="stat-item">
            <div class="stat-value">{{ dataPoints }}</div>
            <div class="stat-label">{{ t('dashboard.dataPoints') }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Control Panel -->
    <div class="control-panel card">
      <h3>{{ t('dashboard.controlPanel') }}</h3>
      <div class="control-grid">
        <div class="form-group">
          <label for="sampleRate">{{ t('dashboard.sampleRate') }}</label>
          <input 
            id="sampleRate"
            type="number" 
            v-model="sampleRate" 
            min="1" 
            max="1000"
            :disabled="isConnected"
          />
        </div>
        <div class="form-group">
          <label for="bufferSize">{{ t('dashboard.bufferSize') }}</label>
          <input 
            id="bufferSize"
            type="number" 
            v-model="bufferSize" 
            min="100" 
            max="10000"
            :disabled="isConnected"
          />
        </div>
        <div class="form-group">
          <label for="channel">{{ t('dashboard.channel') }}</label>
          <select id="channel" v-model="selectedChannel" :disabled="isConnected">
            <option v-for="i in 16" :key="i" :value="i-1">{{ t('dashboard.channel') }} {{ i-1 }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>{{ t('dashboard.autoScroll') }}</label>
          <label class="switch">
            <input type="checkbox" v-model="autoScroll">
            <span class="slider"></span>
          </label>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onUnmounted, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

// Connection state
const isConnected = ref(false);
const connectionStatus = computed(() => isConnected.value ? 'connected' : 'disconnected');
const connectionStatusText = computed(() => isConnected.value ? t('dashboard.connected') : t('dashboard.disconnected'));
const serverUrl = ref('http://localhost:5100/dataHub');
const uptime = ref('00:00:00');
const messageCount = ref(0);

// Data stream
const dataStream = ref<Array<{timestamp: string, type: string, content: string}>>([]);
const autoScroll = ref(true);

// Statistics
const avgValue = ref(0);
const maxValue = ref(0);
const minValue = ref(0);
const dataPoints = ref(0);

// Control parameters
const sampleRate = ref(100);
const bufferSize = ref(1000);
const selectedChannel = ref(0);

// Timer for uptime
let uptimeTimer: number | null = null;
let dataTimer: number | null = null;
let startTime: Date | null = null;

// Data for statistics
const dataValues: number[] = [];

// Calculate statistics
const calculateStatistics = () => {
  if (dataValues.length === 0) {
    avgValue.value = 0;
    maxValue.value = 0;
    minValue.value = 0;
    dataPoints.value = 0;
    return;
  }
  
  const sum = dataValues.reduce((a, b) => a + b, 0);
  avgValue.value = sum / dataValues.length;
  maxValue.value = Math.max(...dataValues);
  minValue.value = Math.min(...dataValues);
  dataPoints.value = dataValues.length;
};

// Toggle connection
const toggleConnection = () => {
  if (isConnected.value) {
    disconnect();
  } else {
    connect();
  }
};

// Connect to data source
const connect = () => {
  isConnected.value = true;
  startTime = new Date();
  messageCount.value = 0;
  
  // Start uptime timer
  uptimeTimer = window.setInterval(() => {
    if (startTime) {
      const diff = new Date().getTime() - startTime.getTime();
      const hours = Math.floor(diff / 3600000);
      const minutes = Math.floor((diff % 3600000) / 60000);
      const seconds = Math.floor((diff % 60000) / 1000);
      uptime.value = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }
  }, 1000);
  
  // Simulate data reception
  simulateDataStream();
};

// Disconnect from data source
const disconnect = () => {
  isConnected.value = false;
  if (uptimeTimer) {
    clearInterval(uptimeTimer);
    uptimeTimer = null;
  }
  if (dataTimer) {
    clearTimeout(dataTimer);
    dataTimer = null;
  }
};

// Simulate data stream
const simulateDataStream = () => {
  if (!isConnected.value) return;
  
  // Generate simulated data
  const value = Math.sin(Date.now() / 1000) * 50 + Math.random() * 20 + 50;
  const timestamp = new Date().toLocaleTimeString();
  
  // Add to stream
  dataStream.value.push({
    timestamp,
    type: value > 75 ? 'high' : value < 25 ? 'low' : 'normal',
    content: `${t('dashboard.channel')} ${selectedChannel.value}: ${value.toFixed(2)}`
  });
  
  // Limit stream size
  if (dataStream.value.length > 100) {
    dataStream.value.shift();
  }
  
  // Update data values for statistics
  dataValues.push(value);
  if (dataValues.length > bufferSize.value) {
    dataValues.shift();
  }
  
  // Update statistics
  calculateStatistics();
  messageCount.value++;
  
  // Auto scroll
  if (autoScroll.value) {
    nextTick(() => {
      const container = document.querySelector('.stream-container');
      if (container) {
        container.scrollTop = container.scrollHeight;
      }
    });
  }
  
  // Continue streaming
  dataTimer = window.setTimeout(() => simulateDataStream(), 1000 / sampleRate.value);
};

// Clear stream
const clearStream = () => {
  dataStream.value = [];
  dataValues.length = 0;
  messageCount.value = 0;
  calculateStatistics();
};

onUnmounted(() => {
  disconnect();
});
</script>

<style scoped>
.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.status-card .card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.status-card h3 {
  margin: 0;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.status-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  animation: pulse 2s infinite;
}

.status-indicator.connected .status-dot {
  background: #10b981;
}

.status-indicator.disconnected .status-dot {
  background: #ef4444;
  animation: none;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.status-text {
  font-weight: 600;
}

.status-details {
  margin-bottom: 1.5rem;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--border-color);
}

.detail-label {
  color: var(--light-text);
}

.detail-value {
  font-weight: 500;
}

.stream-card .card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.stream-card h3 {
  margin: 0;
}

.stream-container {
  height: 400px;
  overflow-y: auto;
  background: var(--surface);
  border-radius: 8px;
  padding: 1rem;
}

.stream-message {
  display: flex;
  gap: 1rem;
  padding: 0.5rem;
  margin-bottom: 0.5rem;
  background: white;
  border-radius: 6px;
  font-family: 'Monaco', 'Courier New', monospace;
  font-size: 0.875rem;
}

.timestamp {
  color: var(--light-text);
  white-space: nowrap;
}

.message-type {
  padding: 0.125rem 0.5rem;
  border-radius: 4px;
  font-weight: 600;
  white-space: nowrap;
}

.type-normal {
  background: rgba(0, 102, 204, 0.1);
  color: var(--primary-color);
}

.type-high {
  background: rgba(239, 68, 68, 0.1);
  color: #dc2626;
}

.type-low {
  background: rgba(245, 158, 11, 0.1);
  color: #d97706;
}

.message-content {
  flex: 1;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  text-align: center;
  color: var(--light-text);
}

.empty-state svg {
  margin-bottom: 1rem;
}

.stats-card h3 {
  margin-bottom: 1.5rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
}

.stat-item {
  text-align: center;
  padding: 1rem;
  background: var(--surface);
  border-radius: 8px;
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: var(--primary-color);
  margin-bottom: 0.5rem;
}

.stat-label {
  color: var(--light-text);
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.control-panel {
  margin-top: 2rem;
}

.control-panel h3 {
  margin-bottom: 1.5rem;
}

.control-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
}

.full-width {
  width: 100%;
}

.switch {
  position: relative;
  display: inline-block;
  width: 50px;
  height: 24px;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: var(--border-color);
  transition: var(--transition);
  border-radius: 24px;
}

.slider:before {
  position: absolute;
  content: "";
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: var(--transition);
  border-radius: 50%;
}

input:checked + .slider {
  background-color: var(--primary-color);
}

input:checked + .slider:before {
  transform: translateX(26px);
}

.btn-secondary {
  background: white;
  color: var(--primary-color);
  border: 2px solid var(--primary-color);
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: var(--transition);
}

.btn-secondary:hover {
  background: var(--primary-color);
  color: white;
}

@media (max-width: 768px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
  
  .control-grid {
    grid-template-columns: 1fr;
  }
}
</style>