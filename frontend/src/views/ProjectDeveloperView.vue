<template>
  <div class="project-developer">
    <!-- 顶部工具栏 -->
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>测控项目开发器</h2>
        <div class="project-info">
          <span>项目: {{ currentProject.name }}</span>
          <button @click="saveProject" class="btn-save">保存项目</button>
          <button @click="loadProject" class="btn-load">加载项目</button>
          <button @click="newProject" class="btn-new">新建项目</button>
        </div>
      </div>
      <div class="toolbar-right">
        <button @click="toggleCodeEditor" class="btn-code">
          {{ showCodeEditor ? '隐藏代码' : '显示代码' }}
        </button>
        <button @click="runProject" class="btn-run" :disabled="isRunning">
          {{ isRunning ? '运行中...' : '运行项目' }}
        </button>
        <button @click="stopProject" class="btn-stop" :disabled="!isRunning">
          停止项目
        </button>
      </div>
    </div>

    <div class="main-content">
      <!-- 左侧控件面板 -->
      <div class="left-panel">
        <div class="panel-header">
          <h3>控件库</h3>
          <input 
            v-model="searchTerm" 
            placeholder="搜索控件..." 
            class="search-input"
          />
        </div>
        
        <div class="control-categories">
          <div 
            v-for="category in filteredCategories" 
            :key="category.name"
            class="category"
          >
            <div 
              class="category-header" 
              @click="toggleCategory(category.name)"
            >
              <span class="category-icon">{{ category.expanded ? '▼' : '▶' }}</span>
              <span>{{ category.name }}</span>
              <span class="count">({{ category.controls.length }})</span>
            </div>
            
            <div v-if="category.expanded" class="category-content">
              <div 
                v-for="control in category.controls"
                :key="control.id"
                class="control-item"
                draggable="true"
                @dragstart="onDragStart($event, control)"
                @dblclick="addControlToCenter(control)"
              >
                <div class="control-icon">
                  <span>{{ control.name.charAt(0) }}</span>
                </div>
                <div class="control-info">
                  <div class="control-name">{{ control.name }}</div>
                  <div class="control-desc">{{ control.description }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 中间设计区域 -->
      <div class="center-panel">
        <div class="design-area-header">
          <h3>设计区域</h3>
          <div class="design-tools">
            <button @click="clearDesign" class="btn-clear">清空</button>
            <button @click="toggleGrid" class="btn-grid">
              {{ showGrid ? '隐藏网格' : '显示网格' }}
            </button>
            <select v-model="zoomLevel" class="zoom-select">
              <option value="0.5">50%</option>
              <option value="0.75">75%</option>
              <option value="1">100%</option>
              <option value="1.25">125%</option>
              <option value="1.5">150%</option>
            </select>
          </div>
        </div>
        
        <div 
          class="design-canvas"
          :class="{ 'show-grid': showGrid }"
          :style="{ transform: `scale(${zoomLevel})` }"
          @drop="onDrop"
          @dragover.prevent
          @click="selectElement(null)"
        >
          <div 
            v-for="element in designElements"
            :key="element.id"
            class="design-element"
            :class="{ 'selected': selectedElement?.id === element.id }"
            :style="{
              left: element.x + 'px',
              top: element.y + 'px',
              width: element.width + 'px',
              height: element.height + 'px'
            }"
            @click.stop="selectElement(element)"
            @mousedown="startDrag($event, element)"
          >
            <component 
              :is="getComponent(element.component)" 
              v-bind="element.props"
              @update:value="updateElementValue(element.id, $event)"
            />
            <div v-if="selectedElement?.id === element.id" class="resize-handles">
              <div class="resize-handle nw" @mousedown.stop="startResize($event, element, 'nw')"></div>
              <div class="resize-handle ne" @mousedown.stop="startResize($event, element, 'ne')"></div>
              <div class="resize-handle sw" @mousedown.stop="startResize($event, element, 'sw')"></div>
              <div class="resize-handle se" @mousedown.stop="startResize($event, element, 'se')"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧属性面板 -->
      <div class="right-panel">
        <div class="panel-header">
          <h3>属性面板</h3>
        </div>
        
        <div v-if="selectedElement" class="properties-panel">
          <div class="property-group">
            <h4>基本属性</h4>
            <div class="property-item">
              <label>名称:</label>
              <input v-model="selectedElement.name" class="property-input" />
            </div>
            <div class="property-item">
              <label>X坐标:</label>
              <input v-model.number="selectedElement.x" type="number" class="property-input" />
            </div>
            <div class="property-item">
              <label>Y坐标:</label>
              <input v-model.number="selectedElement.y" type="number" class="property-input" />
            </div>
            <div class="property-item">
              <label>宽度:</label>
              <input v-model.number="selectedElement.width" type="number" class="property-input" />
            </div>
            <div class="property-item">
              <label>高度:</label>
              <input v-model.number="selectedElement.height" type="number" class="property-input" />
            </div>
          </div>

          <div class="property-group">
            <h4>控件属性</h4>
            <div 
              v-for="(value, key) in selectedElement.props"
              :key="key"
              class="property-item"
            >
              <label>{{ key }}:</label>
              <input 
                v-if="typeof value === 'string' || typeof value === 'number'"
                v-model="selectedElement.props[key]" 
                :type="typeof value === 'number' ? 'number' : 'text'"
                class="property-input" 
              />
              <input 
                v-else-if="typeof value === 'boolean'"
                v-model="selectedElement.props[key]" 
                type="checkbox"
                class="property-checkbox" 
              />
            </div>
          </div>

          <div class="property-group">
            <h4>硬件绑定</h4>
            <div class="property-item">
              <label>硬件设备:</label>
              <select v-model="selectedElement.hardwareBinding.device" class="property-select">
                <option value="">选择设备</option>
                <option v-for="device in availableDevices" :key="device.id" :value="device.id">
                  {{ device.name }}
                </option>
              </select>
            </div>
            <div class="property-item">
              <label>通道/端口:</label>
              <input v-model="selectedElement.hardwareBinding.channel" class="property-input" />
            </div>
            <div class="property-item">
              <label>数据类型:</label>
              <select v-model="selectedElement.hardwareBinding.dataType" class="property-select">
                <option value="analog">模拟量</option>
                <option value="digital">数字量</option>
                <option value="frequency">频率</option>
                <option value="temperature">温度</option>
              </select>
            </div>
          </div>

          <div class="property-group">
            <h4>事件处理</h4>
            <div class="property-item">
              <label>点击事件:</label>
              <textarea 
                v-model="selectedElement.events.onClick" 
                class="property-textarea"
                placeholder="// JavaScript代码"
              ></textarea>
            </div>
            <div class="property-item">
              <label>值变化事件:</label>
              <textarea 
                v-model="selectedElement.events.onValueChange" 
                class="property-textarea"
                placeholder="// JavaScript代码"
              ></textarea>
            </div>
          </div>

          <div class="property-actions">
            <button @click="deleteElement" class="btn-delete">删除控件</button>
            <button @click="duplicateElement" class="btn-duplicate">复制控件</button>
          </div>
        </div>

        <div v-else class="no-selection">
          <p>请选择一个控件来编辑属性</p>
        </div>
      </div>
    </div>

    <!-- 底部代码编辑器 -->
    <div v-if="showCodeEditor" class="code-editor-panel">
      <div class="code-editor-header">
        <h3>项目代码</h3>
        <div class="code-tabs">
          <button 
            v-for="tab in codeTabs"
            :key="tab.id"
            class="code-tab"
            :class="{ active: activeCodeTab === tab.id }"
            @click="activeCodeTab = tab.id"
          >
            {{ tab.name }}
          </button>
        </div>
      </div>
      <div class="code-editor-content">
        <textarea 
          v-model="currentCode"
          class="code-textarea"
          placeholder="在这里编写项目代码..."
        ></textarea>
      </div>
    </div>

    <!-- 项目保存/加载对话框 -->
    <div v-if="showProjectDialog" class="project-dialog-overlay" @click="showProjectDialog = false">
      <div class="project-dialog" @click.stop>
        <h3>{{ projectDialogMode === 'save' ? '保存项目' : '加载项目' }}</h3>
        <div v-if="projectDialogMode === 'save'">
          <input v-model="projectName" placeholder="项目名称" class="project-input" />
          <textarea v-model="projectDescription" placeholder="项目描述" class="project-textarea"></textarea>
        </div>
        <div v-else>
          <div class="project-list">
            <div 
              v-for="project in savedProjects"
              :key="project.id"
              class="project-item"
              @click="selectProject(project)"
            >
              <h4>{{ project.name }}</h4>
              <p>{{ project.description }}</p>
              <small>{{ project.lastModified }}</small>
            </div>
          </div>
        </div>
        <div class="dialog-actions">
          <button @click="showProjectDialog = false" class="btn-cancel">取消</button>
          <button 
            @click="confirmProjectAction" 
            class="btn-confirm"
            :disabled="projectDialogMode === 'save' && !projectName"
          >
            {{ projectDialogMode === 'save' ? '保存' : '加载' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue'
import { backendApi, type HardwareDevice } from '@/services/BackendApiService'

// 导入专业控件组件
import CircularGauge from '@/components/professional/gauges/CircularGauge.vue'
import LinearGauge from '@/components/professional/gauges/LinearGauge.vue'
import Thermometer from '@/components/professional/gauges/Thermometer.vue'
import DigitalDisplay from '@/components/professional/indicators/DigitalDisplay.vue'
import LEDIndicator from '@/components/professional/indicators/LEDIndicator.vue'
import Switch from '@/components/professional/controls/Switch.vue'
import ButtonArray from '@/components/professional/controls/ButtonArray.vue'
import Oscilloscope from '@/components/professional/instruments/Oscilloscope.vue'
import SignalGenerator from '@/components/professional/instruments/SignalGenerator.vue'
import EasyChart from '@/components/professional/charts/EasyChart.vue'
import EnhancedEasyChart from '@/components/professional/charts/EnhancedEasyChart.vue'
import DigitalMultimeter from '@/components/instruments/DigitalMultimeter.vue'
import TemperatureAcquisitionCard from '@/components/professional/instruments/TemperatureAcquisitionCard.vue'
import DIOCard from '@/components/professional/instruments/DIOCard.vue'

// 类型定义
interface ControlDefinition {
  id: string
  name: string
  description: string
  component: string
  icon?: any
  defaultProps: Record<string, any>
  defaultSize: { width: number; height: number }
}

interface DesignElement {
  id: string
  name: string
  component: string
  x: number
  y: number
  width: number
  height: number
  props: Record<string, any>
  hardwareBinding: {
    device: string
    channel: string
    dataType: string
    taskId?: number
  }
  events: {
    onClick: string
    onValueChange: string
  }
}

interface Project {
  id: string
  name: string
  description: string
  elements: DesignElement[]
  code: Record<string, string>
  lastModified: string
}

// 响应式数据
const searchTerm = ref('')
const showGrid = ref(true)
const zoomLevel = ref(1)
const selectedElement = ref<DesignElement | null>(null)
const designElements = ref<DesignElement[]>([])
const showCodeEditor = ref(false)
const activeCodeTab = ref('main')
const isRunning = ref(false)
const showProjectDialog = ref(false)
const projectDialogMode = ref<'save' | 'load'>('save')
const projectName = ref('')
const projectDescription = ref('')
const savedProjects = ref<Project[]>([])

// 当前项目
const currentProject = ref<Project>({
  id: 'new-project',
  name: '新项目',
  description: '',
  elements: [],
  code: {
    main: '// 主程序代码\n',
    hardware: '// 硬件控制代码\n',
    events: '// 事件处理代码\n'
  },
  lastModified: new Date().toISOString()
})

// 可用设备列表
const availableDevices = ref([
  { id: 'mock-device', name: '模拟设备 (MockDevice)' },
  { id: 'jy5510', name: 'JY5510 PXI数据采集卡' },
  { id: 'jy5511', name: 'JY5511 PXI数据采集卡' },
  { id: 'jy5515', name: 'JY5515 PXI数据采集卡' },
  { id: 'jy5516', name: 'JY5516 PXI数据采集卡' },
  { id: 'jyusb1601', name: 'JYUSB1601 USB数据采集卡' }
])

// 控件分类定义
const controlCategories = ref([
  {
    name: '仪表类',
    expanded: true,
    controls: [
      {
        id: 'circular-gauge',
        name: '圆形仪表',
        description: '显示数值的圆形仪表盘',
        component: 'CircularGauge',
        defaultProps: { value: 50, min: 0, max: 100, unit: '' },
        defaultSize: { width: 200, height: 200 }
      },
      {
        id: 'linear-gauge',
        name: '线性仪表',
        description: '线性刻度仪表',
        component: 'LinearGauge',
        defaultProps: { value: 30, min: 0, max: 100 },
        defaultSize: { width: 300, height: 80 }
      },
      {
        id: 'thermometer',
        name: '温度计',
        description: '温度显示控件',
        component: 'Thermometer',
        defaultProps: { value: 25, min: -50, max: 150 },
        defaultSize: { width: 80, height: 300 }
      }
    ]
  },
  {
    name: '指示器',
    expanded: true,
    controls: [
      {
        id: 'digital-display',
        name: '数字显示',
        description: '数字显示屏',
        component: 'DigitalDisplay',
        defaultProps: { value: 123.45, precision: 2 },
        defaultSize: { width: 200, height: 80 }
      },
      {
        id: 'led-indicator',
        name: 'LED指示灯',
        description: 'LED状态指示灯',
        component: 'LEDIndicator',
        defaultProps: { state: false, color: 'green' },
        defaultSize: { width: 50, height: 50 }
      }
    ]
  },
  {
    name: '控制器',
    expanded: true,
    controls: [
      {
        id: 'switch',
        name: '开关',
        description: '开关控件',
        component: 'Switch',
        defaultProps: { value: false },
        defaultSize: { width: 80, height: 40 }
      },
      {
        id: 'button-array',
        name: '按钮组',
        description: '按钮数组',
        component: 'ButtonArray',
        defaultProps: { buttons: ['按钮1', '按钮2', '按钮3'] },
        defaultSize: { width: 300, height: 60 }
      }
    ]
  },
  {
    name: '仪器',
    expanded: true,
    controls: [
      {
        id: 'oscilloscope',
        name: '示波器',
        description: '虚拟示波器',
        component: 'Oscilloscope',
        defaultProps: {},
        defaultSize: { width: 600, height: 400 }
      },
      {
        id: 'signal-generator',
        name: '信号发生器',
        description: '信号发生器控件',
        component: 'SignalGenerator',
        defaultProps: {},
        defaultSize: { width: 400, height: 300 }
      }
    ]
  },
  {
    name: '图表',
    expanded: true,
    controls: [
      {
        id: 'easy-chart',
        name: '基础波形图',
        description: '基础实时波形显示',
        component: 'EasyChart',
        defaultProps: {},
        defaultSize: { width: 500, height: 300 }
      },
      {
        id: 'enhanced-easy-chart',
        name: '增强型图表',
        description: '增强型实时波形显示',
        component: 'EnhancedEasyChart',
        defaultProps: {},
        defaultSize: { width: 600, height: 400 }
      }
    ]
  },
  {
    name: '专业仪器',
    expanded: true,
    controls: [
      {
        id: 'digital-multimeter',
        name: '数字万用表',
        description: '8种测量功能的专业万用表',
        component: 'DigitalMultimeter',
        defaultProps: {},
        defaultSize: { width: 400, height: 300 }
      },
      {
        id: 'temperature-acquisition-card',
        name: '温度采集卡',
        description: '8种热电偶类型的温度采集',
        component: 'TemperatureAcquisitionCard',
        defaultProps: {},
        defaultSize: { width: 500, height: 400 }
      },
      {
        id: 'dio-card',
        name: 'DIO控制卡',
        description: '数字输入输出控制卡',
        component: 'DIOCard',
        defaultProps: {},
        defaultSize: { width: 500, height: 400 }
      }
    ]
  }
])

// 代码标签页
const codeTabs = ref([
  { id: 'main', name: '主程序' },
  { id: 'hardware', name: '硬件控制' },
  { id: 'events', name: '事件处理' }
])

// 计算属性
const filteredCategories = computed(() => {
  if (!searchTerm.value) return controlCategories.value
  
  return controlCategories.value.map(category => ({
    ...category,
    controls: category.controls.filter(control =>
      control.name.toLowerCase().includes(searchTerm.value.toLowerCase()) ||
      control.description.toLowerCase().includes(searchTerm.value.toLowerCase())
    )
  })).filter(category => category.controls.length > 0)
})

const currentCode = computed({
  get: () => currentProject.value.code[activeCodeTab.value] || '',
  set: (value: string) => {
    currentProject.value.code[activeCodeTab.value] = value
  }
})

// 组件映射
const componentMap = {
  CircularGauge,
  LinearGauge,
  Thermometer,
  DigitalDisplay,
  LEDIndicator,
  Switch,
  ButtonArray,
  Oscilloscope,
  SignalGenerator,
  EasyChart,
  EnhancedEasyChart,
  DigitalMultimeter,
  TemperatureAcquisitionCard,
  DIOCard
}

// 方法
const getComponent = (componentName: string) => {
  return componentMap[componentName as keyof typeof componentMap] || 'div'
}

const toggleCategory = (categoryName: string) => {
  const category = controlCategories.value.find(c => c.name === categoryName)
  if (category) {
    category.expanded = !category.expanded
  }
}

const onDragStart = (event: DragEvent, control: ControlDefinition) => {
  if (event.dataTransfer) {
    event.dataTransfer.setData('application/json', JSON.stringify(control))
  }
}

const onDrop = (event: DragEvent) => {
  event.preventDefault()
  if (event.dataTransfer) {
    try {
      const controlData = JSON.parse(event.dataTransfer.getData('application/json'))
      const canvas = event.currentTarget as HTMLElement
      const rect = canvas.getBoundingClientRect()
      const x = Math.max(0, (event.clientX - rect.left) / zoomLevel.value)
      const y = Math.max(0, (event.clientY - rect.top) / zoomLevel.value)
      
      addElement(controlData, x, y)
    } catch (error) {
      console.error('拖拽数据解析失败:', error)
    }
  }
}

const addElement = (controlDef: ControlDefinition, x: number, y: number) => {
  const element: DesignElement = {
    id: `element-${Date.now()}`,
    name: controlDef.name,
    component: controlDef.component,
    x,
    y,
    width: controlDef.defaultSize.width,
    height: controlDef.defaultSize.height,
    props: { ...controlDef.defaultProps },
    hardwareBinding: {
      device: '',
      channel: '',
      dataType: 'analog'
    },
    events: {
      onClick: '',
      onValueChange: ''
    }
  }
  
  designElements.value.push(element)
  selectedElement.value = element
}

const addControlToCenter = (control: ControlDefinition) => {
  // 在设计区域中央添加控件
  const centerX = 200
  const centerY = 150
  console.log('双击添加控件:', control.name)
  console.log('控件定义:', control)
  addElement(control, centerX, centerY)
  console.log('当前设计元素数量:', designElements.value.length)
}

const selectElement = (element: DesignElement | null) => {
  selectedElement.value = element
}

const deleteElement = () => {
  if (selectedElement.value) {
    const index = designElements.value.findIndex(el => el.id === selectedElement.value!.id)
    if (index > -1) {
      designElements.value.splice(index, 1)
      selectedElement.value = null
    }
  }
}

const duplicateElement = () => {
  if (selectedElement.value) {
    const newElement = {
      ...selectedElement.value,
      id: `element-${Date.now()}`,
      x: selectedElement.value.x + 20,
      y: selectedElement.value.y + 20
    }
    designElements.value.push(newElement)
    selectedElement.value = newElement
  }
}

const clearDesign = () => {
  designElements.value = []
  selectedElement.value = null
}

const toggleGrid = () => {
  showGrid.value = !showGrid.value
}

const toggleCodeEditor = () => {
  showCodeEditor.value = !showCodeEditor.value
}

const updateElementValue = (elementId: string, value: any) => {
  const element = designElements.value.find(el => el.id === elementId)
  if (element) {
    element.props.value = value
    
    // 执行值变化事件
    if (element.events.onValueChange) {
      try {
        const func = new Function('value', 'element', element.events.onValueChange)
        func(value, element)
      } catch (error) {
        console.error('事件执行错误:', error)
      }
    }
  }
}

const saveProject = () => {
  projectDialogMode.value = 'save'
  projectName.value = currentProject.value.name
  projectDescription.value = currentProject.value.description
  showProjectDialog.value = true
}

const loadProject = () => {
  projectDialogMode.value = 'load'
  loadSavedProjects()
  showProjectDialog.value = true
}

const newProject = () => {
  currentProject.value = {
    id: 'new-project',
    name: '新项目',
    description: '',
    elements: [],
    code: {
      main: '// 主程序代码\n',
      hardware: '// 硬件控制代码\n',
      events: '// 事件处理代码\n'
    },
    lastModified: new Date().toISOString()
  }
  designElements.value = []
  selectedElement.value = null
}

const confirmProjectAction = () => {
  if (projectDialogMode.value === 'save') {
    const project: Project = {
      ...currentProject.value,
      name: projectName.value,
      description: projectDescription.value,
      elements: [...designElements.value],
      lastModified: new Date().toISOString()
    }
    
    // 保存到本地存储
    const projects = JSON.parse(localStorage.getItem('seesharp-projects') || '[]')
    const existingIndex = projects.findIndex((p: Project) => p.id === project.id)
    
    if (existingIndex > -1) {
      projects[existingIndex] = project
    } else {
      project.id = `project-${Date.now()}`
      projects.push(project)
    }
    
    localStorage.setItem('seesharp-projects', JSON.stringify(projects))
    currentProject.value = project
  }
  
  showProjectDialog.value = false
}

const loadSavedProjects = () => {
  const projects = JSON.parse(localStorage.getItem('seesharp-projects') || '[]')
  savedProjects.value = projects
}

const selectProject = (project: Project) => {
  currentProject.value = project
  designElements.value = [...project.elements]
  selectedElement.value = null
  showProjectDialog.value = false
}

const runProject = async () => {
  isRunning.value = true
  
  try {
    console.log('开始运行项目:', currentProject.value.name)
    
    // 1. 检查后端连接
    const healthCheck = await backendApi.checkConnection()
    if (!healthCheck) {
      throw new Error('后端服务连接失败')
    }
    console.log('✓ 后端服务连接正常')
    
    // 2. 获取可用设备
    const devices = await backendApi.getDevices()
    if (!devices || devices.length === 0) {
      throw new Error('未发现可用设备')
    }
    console.log('✓ 发现设备:', devices.length, '个')
    
    // 3. 查找项目中绑定的硬件设备
    const boundElements = designElements.value.filter(el => el.hardwareBinding.device)
    if (boundElements.length === 0) {
      throw new Error('项目中没有绑定硬件设备的控件')
    }
    console.log('✓ 找到硬件绑定控件:', boundElements.length, '个')
    
    // 4. 为每个绑定的设备启动数据采集任务
    for (const element of boundElements) {
      const deviceId = parseInt(element.hardwareBinding.device)
      const device = devices.find((d: HardwareDevice) => d.id === deviceId)
      
      if (device) {
        console.log(`启动设备 ${device.model} 的数据采集...`)
        
        // 创建任务配置
        const taskConfig = {
          taskName: `项目任务-${element.name}`,
          deviceId: device.id,
          taskType: 'AnalogInput',
          channels: [{
            channelId: parseInt(element.hardwareBinding.channel || '0'),
            rangeLow: -10.0,
            rangeHigh: 10.0,
            terminal: 'Default',
            coupling: 'DC',
            enableIEPE: false
          }],
          sampling: {
            sampleRate: 1000,
            samplesToAcquire: 1000,
            mode: 'Continuous',
            bufferSize: 10000
          },
          trigger: {
            type: 'Immediate',
            source: 'None',
            edge: 'Rising',
            level: 0.0,
            preTriggerSamples: 0
          }
        }
        
        try {
          // 创建并启动任务
          const taskId = await backendApi.createTask(taskConfig)
          await backendApi.startTask(taskId)
          
          console.log(`✓ 设备 ${device.model} 数据采集任务启动成功, 任务ID: ${taskId}`)
          
          // 将任务ID保存到元素中，用于后续停止
          element.hardwareBinding.taskId = taskId
        } catch (taskError) {
          console.warn(`⚠ 设备 ${device.model} 数据采集任务启动失败:`, taskError)
        }
      }
    }
    
    // 5. 启动实时数据监控（如果有图表控件）
    const chartElements = designElements.value.filter(el => 
      ['EasyChart', 'EnhancedEasyChart'].includes(el.component)
    )
    
    if (chartElements.length > 0) {
      console.log('✓ 启动实时数据监控，图表控件数量:', chartElements.length)
      
      // 这里可以启动SignalR连接来接收实时数据
      // 并更新图表控件的数据
    }
    
    console.log('🚀 项目运行成功！')
    
  } catch (error) {
    console.error('❌ 项目运行失败:', error)
    const errorMessage = error instanceof Error ? error.message : '未知错误'
    alert(`项目运行失败: ${errorMessage}`)
    isRunning.value = false
  }
}

const stopProject = () => {
  isRunning.value = false
  console.log('停止项目')
}

// 拖拽和调整大小相关方法
let isDragging = false
let isResizing = false
let dragStartX = 0
let dragStartY = 0
let resizeDirection = ''

const startDrag = (event: MouseEvent, element: DesignElement) => {
  event.preventDefault()
  event.stopPropagation()
  
  isDragging = true
  dragStartX = event.clientX - element.x * zoomLevel.value
  dragStartY = event.clientY - element.y * zoomLevel.value
  selectedElement.value = element
  
  const onMouseMove = (e: MouseEvent) => {
    if (isDragging) {
      element.x = Math.max(0, (e.clientX - dragStartX) / zoomLevel.value)
      element.y = Math.max(0, (e.clientY - dragStartY) / zoomLevel.value)
    }
  }
  
  const onMouseUp = () => {
    isDragging = false
    document.removeEventListener('mousemove', onMouseMove)
    document.removeEventListener('mouseup', onMouseUp)
  }
  
  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}

const startResize = (event: MouseEvent, element: DesignElement, direction: string) => {
  isResizing = true
  resizeDirection = direction
  dragStartX = event.clientX
  dragStartY = event.clientY
  
  const startWidth = element.width
  const startHeight = element.height
  const startX = element.x
  const startY = element.y
  
  const onMouseMove = (e: MouseEvent) => {
    if (isResizing) {
      const deltaX = e.clientX - dragStartX
      const deltaY = e.clientY - dragStartY
      
      switch (direction) {
        case 'se':
          element.width = Math.max(50, startWidth + deltaX)
          element.height = Math.max(50, startHeight + deltaY)
          break
        case 'sw':
          element.width = Math.max(50, startWidth - deltaX)
          element.height = Math.max(50, startHeight + deltaY)
          element.x = startX + deltaX
          break
        case 'ne':
          element.width = Math.max(50, startWidth + deltaX)
          element.height = Math.max(50, startHeight - deltaY)
          element.y = startY + deltaY
          break
        case 'nw':
          element.width = Math.max(50, startWidth - deltaX)
          element.height = Math.max(50, startHeight - deltaY)
          element.x = startX + deltaX
          element.y = startY + deltaY
          break
      }
    }
  }
  
  const onMouseUp = () => {
    isResizing = false
    document.removeEventListener('mousemove', onMouseMove)
    document.removeEventListener('mouseup', onMouseUp)
  }
  
  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}

onMounted(() => {
  loadSavedProjects()
})
</script>

<style scoped>
.project-developer {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background: #f5f5f5;
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 20px;
  background: white;
  border-bottom: 1px solid #ddd;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.toolbar-left {
  display: flex;
  align-items: center;
  gap: 20px;
}

.toolbar-left h2 {
  margin: 0;
  color: #333;
}

.project-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.toolbar-right {
  display: flex;
  gap: 10px;
}

.btn-save, .btn-load, .btn-new, .btn-code, .btn-run, .btn-stop {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.btn-save { background: #4CAF50; color: white; }
.btn-load { background: #2196F3; color: white; }
.btn-new { background: #FF9800; color: white; }
.btn-code { background: #9C27B0; color: white; }
.btn-run { background: #4CAF50; color: white; }
.btn-stop { background: #f44336; color: white; }

.btn-save:hover, .btn-load:hover, .btn-new:hover, 
.btn-code:hover, .btn-run:hover, .btn-stop:hover {
  opacity: 0.8;
}

.btn-run:disabled, .btn-stop:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.main-content {
  display: flex;
  flex: 1;
  overflow: hidden;
}

.left-panel {
  width: 300px;
  background: white;
  border-right: 1px solid #ddd;
  display: flex;
  flex-direction: column;
}

.panel-header {
  padding: 15px;
  border-bottom: 1px solid #eee;
}

.panel-header h3 {
  margin: 0 0 10px 0;
  color: #333;
}

.search-input {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.control-categories {
  flex: 1;
  overflow-y: auto;
  padding: 10px;
}

.category {
  margin-bottom: 10px;
}

.category-header {
  display: flex;
  align-items: center;
  padding: 8px;
  background: #f8f9fa;
  border-radius: 4px;
  cursor: pointer;
  user-select: none;
}

.category-header:hover {
  background: #e9ecef;
}

.category-icon {
  margin-right: 8px;
  font-size: 12px;
}

.count {
  margin-left: auto;
  font-size: 12px;
  color: #666;
}

.category-content {
  margin-top: 5px;
}

.control-item {
  display: flex;
  align-items: center;
  padding: 8px;
  margin: 2px 0;
  background: white;
  border: 1px solid #eee;
  border-radius: 4px;
  cursor: grab;
  transition: all 0.2s;
}

.control-item:hover {
  border-color: #007bff;
  box-shadow: 0 2px 4px rgba(0,123,255,0.1);
}

.control-item:active {
  cursor: grabbing;
}

.control-icon {
  width: 32px;
  height: 32px;
  background: #f0f0f0;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 10px;
  font-weight: bold;
  color: #666;
}

.control-info {
  flex: 1;
}

.control-name {
  font-weight: 500;
  color: #333;
  font-size: 14px;
}

.control-desc {
  font-size: 12px;
  color: #666;
  margin-top: 2px;
}

.center-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #f8f9fa;
}

.design-area-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  background: white;
  border-bottom: 1px solid #ddd;
}

.design-area-header h3 {
  margin: 0;
  color: #333;
}

.design-tools {
  display: flex;
  gap: 10px;
  align-items: center;
}

.btn-clear, .btn-grid {
  padding: 6px 12px;
  border: 1px solid #ddd;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
}

.btn-clear:hover, .btn-grid:hover {
  background: #f8f9fa;
}

.zoom-select {
  padding: 6px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 12px;
}

.design-canvas {
  flex: 1;
  position: relative;
  overflow: auto;
  background: white;
  margin: 10px;
  border-radius: 4px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  transform-origin: top left;
}

.design-canvas.show-grid {
  background-image: 
    linear-gradient(to right, #f0f0f0 1px, transparent 1px),
    linear-gradient(to bottom, #f0f0f0 1px, transparent 1px);
  background-size: 20px 20px;
}

.design-element {
  position: absolute;
  border: 2px solid transparent;
  cursor: move;
}

.design-element.selected {
  border-color: #007bff;
}

.resize-handles {
  position: absolute;
  top: -4px;
  left: -4px;
  right: -4px;
  bottom: -4px;
  pointer-events: none;
}

.resize-handle {
  position: absolute;
  width: 8px;
  height: 8px;
  background: #007bff;
  border: 1px solid white;
  border-radius: 50%;
  pointer-events: all;
  cursor: pointer;
}

.resize-handle.nw {
  top: 0;
  left: 0;
  cursor: nw-resize;
}

.resize-handle.ne {
  top: 0;
  right: 0;
  cursor: ne-resize;
}

.resize-handle.sw {
  bottom: 0;
  left: 0;
  cursor: sw-resize;
}

.resize-handle.se {
  bottom: 0;
  right: 0;
  cursor: se-resize;
}

.right-panel {
  width: 350px;
  background: white;
  border-left: 1px solid #ddd;
  display: flex;
  flex-direction: column;
}

.properties-panel {
  flex: 1;
  overflow-y: auto;
  padding: 15px;
}

.property-group {
  margin-bottom: 20px;
  border: 1px solid #eee;
  border-radius: 4px;
  padding: 15px;
}

.property-group h4 {
  margin: 0 0 15px 0;
  color: #333;
  font-size: 14px;
  font-weight: 600;
}

.property-item {
  margin-bottom: 10px;
}

.property-item label {
  display: block;
  margin-bottom: 5px;
  font-size: 12px;
  font-weight: 500;
  color: #555;
}

.property-input, .property-select {
  width: 100%;
  padding: 6px 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 12px;
}

.property-checkbox {
  margin: 0;
}

.property-textarea {
  width: 100%;
  min-height: 60px;
  padding: 6px 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 12px;
  font-family: 'Courier New', monospace;
  resize: vertical;
}

.property-actions {
  display: flex;
  gap: 10px;
  margin-top: 15px;
}

.btn-delete, .btn-duplicate {
  flex: 1;
  padding: 8px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
}

.btn-delete {
  background: #dc3545;
  color: white;
}

.btn-duplicate {
  background: #6c757d;
  color: white;
}

.btn-delete:hover, .btn-duplicate:hover {
  opacity: 0.8;
}

.no-selection {
  padding: 20px;
  text-align: center;
  color: #666;
}

.code-editor-panel {
  height: 300px;
  background: white;
  border-top: 1px solid #ddd;
  display: flex;
  flex-direction: column;
}

.code-editor-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 15px;
  border-bottom: 1px solid #eee;
}

.code-editor-header h3 {
  margin: 0;
  color: #333;
}

.code-tabs {
  display: flex;
  gap: 5px;
}

.code-tab {
  padding: 6px 12px;
  border: 1px solid #ddd;
  background: #f8f9fa;
  border-radius: 4px 4px 0 0;
  cursor: pointer;
  font-size: 12px;
}

.code-tab.active {
  background: white;
  border-bottom-color: white;
}

.code-editor-content {
  flex: 1;
  padding: 10px;
}

.code-textarea {
  width: 100%;
  height: 100%;
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 10px;
  font-family: 'Courier New', monospace;
  font-size: 12px;
  resize: none;
}

.project-dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.project-dialog {
  background: white;
  border-radius: 8px;
  padding: 20px;
  width: 500px;
  max-height: 80vh;
  overflow-y: auto;
}

.project-dialog h3 {
  margin: 0 0 20px 0;
  color: #333;
}

.project-input, .project-textarea {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  margin-bottom: 10px;
}

.project-textarea {
  min-height: 80px;
  resize: vertical;
}

.project-list {
  max-height: 300px;
  overflow-y: auto;
  border: 1px solid #eee;
  border-radius: 4px;
}

.project-item {
  padding: 15px;
  border-bottom: 1px solid #eee;
  cursor: pointer;
  transition: background 0.2s;
}

.project-item:hover {
  background: #f8f9fa;
}

.project-item:last-child {
  border-bottom: none;
}

.project-item h4 {
  margin: 0 0 5px 0;
  color: #333;
}

.project-item p {
  margin: 0 0 5px 0;
  color: #666;
  font-size: 14px;
}

.project-item small {
  color: #999;
  font-size: 12px;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 20px;
}

.btn-cancel, .btn-confirm {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.btn-cancel {
  background: #6c757d;
  color: white;
}

.btn-confirm {
  background: #007bff;
  color: white;
}

.btn-confirm:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.btn-cancel:hover, .btn-confirm:hover {
  opacity: 0.8;
}
</style>
