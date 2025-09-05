<template>
  <div class="container fade-in">
    <div class="hero">
      <h1 class="hero-title gradient-text">{{ t('hero.dashboardTitle') }}</h1>
      <p class="hero-subtitle">{{ t('hero.dashboardSubtitle') }}</p>
    </div>

    <div class="dashboard-layout">
      <!-- Hardware Connection Panel -->
      <div class="hardware-panel card">
        <div class="card-header">
          <h3>{{ t('dashboard.hardwareConnection', 'Hardware Connection') }}</h3>
          <div class="connection-indicator" :class="connectionStatus">
            <span class="status-dot"></span>
            <span class="status-text">{{ connectionStatusText }}</span>
          </div>
        </div>
        
        <div class="hardware-info">
          <div v-if="deviceInfo.isSimulated" class="simulation-badge">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"></path>
            </svg>
            {{ t('dashboard.simulationMode', 'Simulation Mode') }}
          </div>
          <div class="info-row">
            <span class="info-label">{{ t('dashboard.device', 'Device') }}:</span>
            <span class="info-value">{{ deviceInfo.model || 'USB-1601' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">{{ t('dashboard.serialNumber', 'Serial') }}:</span>
            <span class="info-value">{{ deviceInfo.serialNumber || 'N/A' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">{{ t('dashboard.channels', 'Channels') }}:</span>
            <span class="info-value">{{ deviceInfo.channels || 16 }} AI</span>
          </div>
          <div class="info-row">
            <span class="info-label">{{ t('dashboard.maxSampleRate', 'Max Rate') }}:</span>
            <span class="info-value">{{ deviceInfo.maxSampleRate || 200000 }} S/s</span>
          </div>
        </div>
        
        <div class="connection-controls">
          <button 
            v-if="!isConnected" 
            @click="connectHardware" 
            class="btn-primary full-width"
            :disabled="isConnecting"
          >
            <span v-if="isConnecting" class="loading"></span>
            {{ isConnecting ? t('dashboard.connecting', 'Connecting...') : t('dashboard.connect') }}
          </button>
          
          <div v-else class="connected-controls">
            <button @click="startAcquisition" class="btn-success" :disabled="isAcquiring">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polygon points="5 3 19 12 5 21 5 3"></polygon>
              </svg>
              {{ t('dashboard.start', 'Start') }}
            </button>
            <button @click="stopAcquisition" class="btn-warning" :disabled="!isAcquiring">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="6" y="4" width="4" height="16"></rect>
                <rect x="14" y="4" width="4" height="16"></rect>
              </svg>
              {{ t('dashboard.stop', 'Stop') }}
            </button>
            <button @click="disconnectHardware" class="btn-secondary">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M18.36 6.64a9 9 0 1 1-12.73 0"></path>
                <line x1="12" y1="2" x2="12" y2="12"></line>
              </svg>
              {{ t('dashboard.disconnect') }}
            </button>
          </div>
        </div>
      </div>

      <!-- Waveform Display -->
      <div class="waveform-panel card">
        <div class="card-header">
          <h3>{{ t('dashboard.realtimeWaveform', 'Real-time Waveform') }}</h3>
          <div class="waveform-controls">
            <button @click="clearWaveform" class="btn-secondary small">
              {{ t('dashboard.clear') }}
            </button>
            <button @click="exportData" class="btn-secondary small">
              {{ t('dashboard.export', 'Export') }}
            </button>
          </div>
        </div>
        
        <WaveformDisplay 
          ref="waveformDisplay"
          :width="800"
          :height="400"
          :channels="activeChannels"
        />
      </div>

      <!-- Channel Configuration -->
      <div class="config-panel card">
        <div class="card-header">
          <h3>{{ t('dashboard.channelConfig', 'Channel Configuration') }}</h3>
        </div>
        
        <div class="channel-list">
          <div 
            v-for="channel in channels" 
            :key="channel.id"
            class="channel-item"
            :class="{ active: channel.enabled }"
          >
            <input 
              type="checkbox" 
              v-model="channel.enabled"
              @change="updateChannelConfig"
            />
            <div 
              class="channel-color" 
              :style="{ backgroundColor: channel.color }"
            ></div>
            <span class="channel-name">{{ channel.name }}</span>
            <select 
              v-model="channel.range" 
              class="channel-range"
              @change="updateChannelConfig"
            >
              <option :value="1">±1V</option>
              <option :value="2">±2V</option>
              <option :value="5">±5V</option>
              <option :value="10">±10V</option>
            </select>
            <span class="channel-value">{{ channelValues[channel.id]?.toFixed(3) || '0.000' }} V</span>
          </div>
        </div>
      </div>

      <!-- Acquisition Settings -->
      <div class="settings-panel card">
        <div class="card-header">
          <h3>{{ t('dashboard.acquisitionSettings', 'Acquisition Settings') }}</h3>
        </div>
        
        <div class="settings-grid">
          <div class="form-group">
            <label>{{ t('dashboard.sampleRate') }}</label>
            <select v-model="acquisitionSettings.sampleRate" :disabled="isAcquiring">
              <option :value="100">100 Hz</option>
              <option :value="1000">1 kHz</option>
              <option :value="10000">10 kHz</option>
              <option :value="50000">50 kHz</option>
              <option :value="100000">100 kHz</option>
              <option :value="200000">200 kHz</option>
            </select>
          </div>
          
          <div class="form-group">
            <label>{{ t('dashboard.bufferSize') }}</label>
            <input 
              type="number" 
              v-model="acquisitionSettings.bufferSize"
              min="100"
              max="10000"
              :disabled="isAcquiring"
            />
          </div>
          
          <div class="form-group">
            <label>{{ t('dashboard.triggerMode', 'Trigger Mode') }}</label>
            <select v-model="acquisitionSettings.triggerMode" :disabled="isAcquiring">
              <option value="none">{{ t('dashboard.none', 'None') }}</option>
              <option value="rising">{{ t('dashboard.risingEdge', 'Rising Edge') }}</option>
              <option value="falling">{{ t('dashboard.fallingEdge', 'Falling Edge') }}</option>
            </select>
          </div>
          
          <div class="form-group">
            <label>{{ t('dashboard.triggerLevel', 'Trigger Level') }}</label>
            <input 
              type="number" 
              v-model="acquisitionSettings.triggerLevel"
              min="-10"
              max="10"
              step="0.1"
              :disabled="isAcquiring || acquisitionSettings.triggerMode === 'none'"
            />
          </div>
        </div>
      </div>

      <!-- Statistics Panel -->
      <div class="stats-panel card">
        <div class="card-header">
          <h3>{{ t('dashboard.statistics') }}</h3>
        </div>
        
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-label">{{ t('dashboard.dataPoints') }}</div>
            <div class="stat-value">{{ statistics.dataPoints }}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">{{ t('dashboard.sampleRate') }}</div>
            <div class="stat-value">{{ statistics.actualRate }} Hz</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">{{ t('dashboard.runtime', 'Runtime') }}</div>
            <div class="stat-value">{{ runtime }}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">{{ t('dashboard.errors', 'Errors') }}</div>
            <div class="stat-value" :class="{ 'error': statistics.errors > 0 }">
              {{ statistics.errors }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Error Message -->
    <div v-if="errorMessage" class="error-message fade-in">
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"></circle>
        <line x1="12" y1="8" x2="12" y2="12"></line>
        <line x1="12" y1="16" x2="12.01" y2="16"></line>
      </svg>
      {{ errorMessage }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useI18n } from 'vue-i18n';
import WaveformDisplay from '../components/WaveformDisplay.vue';
import { usb1601Service, usb1601WebSocket } from '../services/usb1601Service';

const { t } = useI18n();

// Connection state
const isConnected = ref(false);
const isConnecting = ref(false);
const isAcquiring = ref(false);
const connectionStatus = computed(() => isConnected.value ? 'connected' : 'disconnected');
const connectionStatusText = computed(() => isConnected.value ? t('dashboard.connected') : t('dashboard.disconnected'));

// Device info
const deviceInfo = ref({
  model: 'USB-1601',
  serialNumber: '',
  channels: 16,
  maxSampleRate: 200000,
  resolution: 12,
  isSimulated: false
});

// Channels configuration
const channels = ref([
  { id: 0, name: 'Channel 0', color: '#00ff00', enabled: true, range: 10 },
  { id: 1, name: 'Channel 1', color: '#ff0000', enabled: false, range: 10 },
  { id: 2, name: 'Channel 2', color: '#0088ff', enabled: false, range: 10 },
  { id: 3, name: 'Channel 3', color: '#ffff00', enabled: false, range: 10 },
  { id: 4, name: 'Channel 4', color: '#ff00ff', enabled: false, range: 10 },
  { id: 5, name: 'Channel 5', color: '#00ffff', enabled: false, range: 10 },
  { id: 6, name: 'Channel 6', color: '#ff8800', enabled: false, range: 10 },
  { id: 7, name: 'Channel 7', color: '#8800ff', enabled: false, range: 10 }
]);

const activeChannels = computed(() => channels.value.filter(ch => ch.enabled));
const channelValues = ref<{ [key: number]: number }>({});

// Acquisition settings
const acquisitionSettings = ref({
  sampleRate: 1000,
  bufferSize: 1000,
  triggerMode: 'none',
  triggerLevel: 0
});

// Statistics
const statistics = ref({
  dataPoints: 0,
  actualRate: 0,
  errors: 0
});

// Runtime tracking
const runtime = ref('00:00:00');
let startTime: Date | null = null;
let runtimeTimer: number | null = null;

// Error handling
const errorMessage = ref('');

// Refs
const waveformDisplay = ref<InstanceType<typeof WaveformDisplay>>();

// Data acquisition timer
let dataTimer: number | null = null;

/**
 * Connect to hardware
 */
async function connectHardware() {
  isConnecting.value = true;
  errorMessage.value = '';
  
  try {
    // Test connection first
    const testResult = await usb1601Service.testConnection();
    
    // Check if in simulation mode
    if (testResult.isSimulated) {
      console.log('Running in simulation mode - no real hardware detected');
    }
    
    // Connect to device (real or simulated)
    const connectResult = await usb1601Service.connect();
    if (!connectResult.success) {
      throw new Error(connectResult.error || 'Failed to connect');
    }
    
    // Get device info
    const info = await usb1601Service.getDeviceInfo();
    if (info) {
      deviceInfo.value = { ...deviceInfo.value, ...info };
      
      // Update model name if simulated
      if (info.isSimulated) {
        deviceInfo.value.model = 'USB-1601 (Simulated)';
      }
    }
    
    // Connect WebSocket for real-time data (optional for simulated mode)
    try {
      usb1601WebSocket.connect();
      usb1601WebSocket.on('data', handleRealtimeData);
    } catch (wsError) {
      console.warn('WebSocket connection failed, using HTTP polling only:', wsError);
    }
    
    isConnected.value = true;
    
    // Show success message
    if (testResult.isSimulated) {
      console.log('Connected successfully in simulation mode');
    }
  } catch (error: any) {
    console.error('Failed to connect:', error);
    errorMessage.value = `Connection failed: ${error.message || 'Unknown error'}`;
    setTimeout(() => errorMessage.value = '', 5000);
  } finally {
    isConnecting.value = false;
  }
}

/**
 * Disconnect from hardware
 */
async function disconnectHardware() {
  try {
    if (isAcquiring.value) {
      await stopAcquisition();
    }
    
    await usb1601Service.disconnect();
    usb1601WebSocket.disconnect();
    
    isConnected.value = false;
    deviceInfo.value.serialNumber = '';
  } catch (error) {
    console.error('Failed to disconnect:', error);
    errorMessage.value = `Disconnect failed: ${error.message}`;
    setTimeout(() => errorMessage.value = '', 5000);
  }
}

/**
 * Start data acquisition
 */
async function startAcquisition() {
  if (!isConnected.value || isAcquiring.value) return;
  
  try {
    // Configure channels
    const enabledChannels = channels.value
      .filter(ch => ch.enabled)
      .map(ch => ch.id);
    
    if (enabledChannels.length === 0) {
      throw new Error('Please enable at least one channel');
    }
    
    // Configure hardware
    await usb1601Service.configureAI({
      channels: enabledChannels,
      sampleRate: acquisitionSettings.value.sampleRate,
      range: 10 // Use max range, scale in display
    });
    
    // Start continuous acquisition
    await usb1601Service.startContinuousAI({
      channels: enabledChannels,
      sampleRate: acquisitionSettings.value.sampleRate,
      bufferSize: acquisitionSettings.value.bufferSize
    });
    
    isAcquiring.value = true;
    startTime = new Date();
    statistics.value.dataPoints = 0;
    statistics.value.errors = 0;
    
    // Start runtime timer
    startRuntimeTimer();
    
    // Start data polling
    startDataPolling();
    
  } catch (error) {
    console.error('Failed to start acquisition:', error);
    errorMessage.value = `Failed to start: ${error.message}`;
    setTimeout(() => errorMessage.value = '', 5000);
  }
}

/**
 * Stop data acquisition
 */
async function stopAcquisition() {
  if (!isAcquiring.value) return;
  
  try {
    await usb1601Service.stopContinuousAI();
    isAcquiring.value = false;
    
    // Stop timers
    if (dataTimer) {
      clearInterval(dataTimer);
      dataTimer = null;
    }
    
    if (runtimeTimer) {
      clearInterval(runtimeTimer);
      runtimeTimer = null;
    }
    
  } catch (error) {
    console.error('Failed to stop acquisition:', error);
    errorMessage.value = `Failed to stop: ${error.message}`;
    setTimeout(() => errorMessage.value = '', 5000);
  }
}

/**
 * Start polling for data
 */
function startDataPolling() {
  if (dataTimer) clearInterval(dataTimer);
  
  const pollInterval = Math.max(50, 1000 / acquisitionSettings.value.sampleRate);
  
  dataTimer = window.setInterval(async () => {
    if (!isAcquiring.value) return;
    
    try {
      const result = await usb1601Service.readLatestData();
      if (result.data && result.data.length > 0) {
        processData(result.data);
      }
    } catch (error) {
      console.error('Data polling error:', error);
      statistics.value.errors++;
    }
  }, pollInterval);
}

/**
 * Handle real-time data from WebSocket
 */
function handleRealtimeData(data: any) {
  if (!isAcquiring.value) return;
  
  if (data.type === 'data' && data.values) {
    processData(data.values);
  }
}

/**
 * Process incoming data
 */
function processData(data: any) {
  // Update channel values
  if (Array.isArray(data)) {
    activeChannels.value.forEach((channel, index) => {
      if (index < data.length) {
        channelValues.value[channel.id] = data[index];
        
        // Add to waveform display
        if (waveformDisplay.value) {
          waveformDisplay.value.addData(channel.id, [data[index]]);
        }
      }
    });
    
    statistics.value.dataPoints += data.length;
  }
  
  // Update actual sample rate
  if (startTime) {
    const elapsed = (Date.now() - startTime.getTime()) / 1000;
    if (elapsed > 0) {
      statistics.value.actualRate = Math.round(statistics.value.dataPoints / elapsed);
    }
  }
}

/**
 * Clear waveform display
 */
function clearWaveform() {
  if (waveformDisplay.value) {
    waveformDisplay.value.clearData();
  }
  statistics.value.dataPoints = 0;
  channelValues.value = {};
}

/**
 * Export data to CSV
 */
function exportData() {
  // TODO: Implement data export
  console.log('Export data - to be implemented');
}

/**
 * Update channel configuration
 */
async function updateChannelConfig() {
  if (!isConnected.value || isAcquiring.value) return;
  
  // Configuration will be applied when starting acquisition
}

/**
 * Start runtime timer
 */
function startRuntimeTimer() {
  if (runtimeTimer) clearInterval(runtimeTimer);
  
  runtimeTimer = window.setInterval(() => {
    if (startTime) {
      const diff = Date.now() - startTime.getTime();
      const hours = Math.floor(diff / 3600000);
      const minutes = Math.floor((diff % 3600000) / 60000);
      const seconds = Math.floor((diff % 60000) / 1000);
      runtime.value = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }
  }, 1000);
}

// Cleanup on unmount
onUnmounted(() => {
  if (isAcquiring.value) {
    stopAcquisition();
  }
  if (isConnected.value) {
    disconnectHardware();
  }
});
</script>

<style scoped>
.dashboard-layout {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.hardware-panel {
  grid-column: span 1;
}

.waveform-panel {
  grid-column: span 2;
}

.config-panel {
  grid-column: span 1;
}

.settings-panel {
  grid-column: span 1;
}

.stats-panel {
  grid-column: span 1;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.card-header h3 {
  margin: 0;
}

.connection-indicator {
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

.connection-indicator.connected .status-dot {
  background: #10b981;
}

.connection-indicator.disconnected .status-dot {
  background: #ef4444;
  animation: none;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.hardware-info {
  margin-bottom: 1.5rem;
}

.simulation-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: linear-gradient(135deg, #fbbf24, #f59e0b);
  color: white;
  border-radius: 100px;
  font-weight: 600;
  font-size: 0.875rem;
  margin-bottom: 1rem;
  animation: pulse-glow 2s infinite;
}

@keyframes pulse-glow {
  0%, 100% { 
    box-shadow: 0 2px 10px rgba(245, 158, 11, 0.3);
  }
  50% { 
    box-shadow: 0 2px 20px rgba(245, 158, 11, 0.5);
  }
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--border-color);
}

.info-label {
  color: var(--light-text);
  font-weight: 500;
}

.info-value {
  font-family: monospace;
  font-weight: 600;
}

.connection-controls {
  margin-top: 1.5rem;
}

.connected-controls {
  display: flex;
  gap: 0.5rem;
}

.connected-controls button {
  flex: 1;
}

.channel-list {
  max-height: 400px;
  overflow-y: auto;
}

.channel-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  border-radius: 6px;
  margin-bottom: 0.5rem;
  background: var(--surface);
  transition: all 0.2s ease;
}

.channel-item.active {
  background: rgba(0, 102, 204, 0.1);
  border: 1px solid var(--primary-color);
}

.channel-color {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 2px solid white;
}

.channel-name {
  flex: 1;
  font-weight: 500;
}

.channel-range {
  padding: 0.25rem 0.5rem;
  border: 1px solid var(--border-color);
  border-radius: 4px;
  background: white;
}

.channel-value {
  font-family: monospace;
  font-weight: 600;
  min-width: 80px;
  text-align: right;
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

.stat-item {
  padding: 1rem;
  background: var(--surface);
  border-radius: 8px;
  text-align: center;
}

.stat-label {
  font-size: 0.875rem;
  color: var(--light-text);
  margin-bottom: 0.5rem;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--primary-color);
}

.stat-value.error {
  color: #dc2626;
}

.waveform-controls {
  display: flex;
  gap: 0.5rem;
}

.btn-secondary.small {
  padding: 0.25rem 0.75rem;
  font-size: 0.875rem;
}

.btn-success {
  background: #10b981;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s ease;
}

.btn-success:hover:not(:disabled) {
  background: #059669;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.25);
}

.btn-warning {
  background: #f59e0b;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s ease;
}

.btn-warning:hover:not(:disabled) {
  background: #d97706;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(245, 158, 11, 0.25);
}

.btn-secondary {
  background: white;
  color: var(--text-color);
  border: 2px solid var(--border-color);
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s ease;
}

.btn-secondary:hover:not(:disabled) {
  border-color: var(--primary-color);
  color: var(--primary-color);
}

button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.error-message {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  background: rgba(239, 68, 68, 0.9);
  color: white;
  border-radius: 8px;
  font-weight: 500;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 1000;
}

.loading {
  display: inline-block;
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.full-width {
  width: 100%;
}

@media (max-width: 1200px) {
  .waveform-panel {
    grid-column: span 1;
  }
}

@media (max-width: 768px) {
  .dashboard-layout {
    grid-template-columns: 1fr;
  }
  
  .hardware-panel,
  .waveform-panel,
  .config-panel,
  .settings-panel,
  .stats-panel {
    grid-column: span 1;
  }
}
</style>