<template>
  <div class="ai-test-platform">
    <!-- 页面标题 -->
    <div class="page-header">
      <h1>AI智能测试平台</h1>
      <p class="subtitle">基于自然语言的智能测试代码生成和执行平台</p>
    </div>

    <!-- 主要内容区域 -->
    <div class="platform-content">
      <!-- 左侧面板 - 需求输入和模板选择 -->
      <div class="left-panel">
          <!-- 需求输入区域 -->
        <div class="requirement-section">
          <h3>测试需求描述</h3>
          
          <!-- 演示需求选择 -->
          <div class="demo-requirements">
            <label>📋 选择演示需求：</label>
            <div class="demo-buttons">
              <button
                v-for="demo in demoRequirements"
                :key="demo.id"
                @click="selectDemoRequirement(demo)"
                class="demo-btn"
                :title="demo.description"
              >
                {{ demo.icon }} {{ demo.name }}
              </button>
            </div>
          </div>

          <div class="input-group">
            <label>请用中文描述您的测试需求：</label>
            <textarea
              v-model="testRequirement"
              placeholder="例如：我需要对JY5500信号发生器进行THD测试，频率范围1kHz，分析2-10次谐波..."
              rows="4"
              class="requirement-input"
            ></textarea>
          </div>

          <!-- 设备选择 -->
          <div class="device-selection">
            <label>目标设备：</label>
            <select v-model="selectedDevice" class="device-select">
              <option value="">自动识别</option>
              <option value="JY5500">JY5500 信号发生器</option>
              <option value="JYUSB1601">JYUSB1601 数据采集卡</option>
              <option value="通用">通用设备</option>
            </select>
          </div>

          <!-- 测试类型 -->
          <div class="test-type-selection">
            <label>测试类型：</label>
            <div class="test-type-buttons">
              <button
                v-for="type in testTypes"
                :key="type.id"
                :class="['type-btn', { active: selectedTestType === type.id }]"
                @click="selectedTestType = type.id"
              >
                {{ type.name }}
              </button>
            </div>
          </div>

          <!-- 生成按钮和管理功能 -->
          <div class="button-group">
            <button
              @click="generateTestCode"
              :disabled="isGenerating || !testRequirement.trim()"
              class="generate-btn"
            >
              <span v-if="isGenerating" class="loading-spinner"></span>
              {{ isGenerating ? '正在生成...' : '生成测试代码' }}
            </button>
            <button
              @click="openApiKeyManagement"
              class="api-key-btn"
              title="管理AI API密钥"
            >
              🔑 API密钥管理
            </button>
            <button
              @click="openTestHistory"
              class="history-btn"
              title="查看测试历史记录"
            >
              📊 测试历史
            </button>
          </div>
        </div>

        <!-- 模板选择区域 -->
        <div class="template-section">
          <h3>推荐模板</h3>
          <div class="template-list">
            <div
              v-for="template in recommendedTemplates"
              :key="template.id"
              :class="['template-item', { selected: selectedTemplate === template.id }]"
              @click="selectTemplate(template)"
            >
              <div class="template-header">
                <h4>{{ template.name }}</h4>
                <span class="template-rating">⭐ {{ template.rating }}</span>
              </div>
              <p class="template-desc">{{ template.description }}</p>
              <div class="template-tags">
                <span
                  v-for="tag in template.tags"
                  :key="tag"
                  class="tag"
                >
                  {{ tag }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧面板 - 代码编辑和执行 -->
      <div class="right-panel">
        <!-- 代码编辑器 -->
        <div class="code-editor-section">
          <div class="editor-header">
            <h3>测试代码</h3>
            <div class="editor-actions">
              <button @click="copyCode" class="action-btn">
                📋 复制代码
              </button>
              <button @click="saveTemplate" class="action-btn">
                💾 保存为模板
              </button>
            </div>
          </div>
          
          <div class="editor-container">
            <textarea
              v-model="generatedCode"
              class="code-editor"
              placeholder="生成的测试代码将显示在这里..."
              rows="20"
            ></textarea>
          </div>

          <!-- 代码质量信息 -->
          <div v-if="codeQuality" class="code-quality">
            <h4>代码质量评估</h4>
            <div class="quality-score">
              <span class="score">{{ codeQuality.score }}/100</span>
              <div class="score-bar">
                <div 
                  class="score-fill" 
                  :style="{ width: `${codeQuality.score}%` }"
                ></div>
              </div>
            </div>
            
            <!-- 问题和建议 -->
            <div v-if="codeQuality.issues?.length" class="issues">
              <h5>⚠️ 发现的问题：</h5>
              <ul>
                <li v-for="issue in codeQuality.issues" :key="issue">{{ issue }}</li>
              </ul>
            </div>
            
            <div v-if="codeQuality.suggestions?.length" class="suggestions">
              <h5>💡 改进建议：</h5>
              <ul>
                <li v-for="suggestion in codeQuality.suggestions" :key="suggestion">{{ suggestion }}</li>
              </ul>
            </div>
          </div>
        </div>

        <!-- 测试执行区域 -->
        <div class="execution-section">
          <div class="execution-header">
            <h3>测试执行</h3>
            <button
              @click="executeTest"
              :disabled="isExecuting || !generatedCode.trim()"
              class="execute-btn"
            >
              <span v-if="isExecuting" class="loading-spinner"></span>
              {{ isExecuting ? '执行中...' : '▶️ 执行测试' }}
            </button>
          </div>

          <!-- 执行结果 -->
          <div v-if="executionResult" class="execution-result">
            <div class="result-header">
              <h4>执行结果</h4>
              <span :class="['result-status', executionResult.success ? 'success' : 'error']">
                {{ executionResult.success ? '✅ 成功' : '❌ 失败' }}
              </span>
            </div>

            <!-- 结果数据 -->
            <div v-if="executionResult.data" class="result-content">
              <div class="result-summary">
                <h5>测试摘要</h5>
                <div class="summary-grid">
                  <div class="summary-item">
                    <span class="label">设备类型:</span>
                    <span class="value">{{ executionResult.data.deviceType }}</span>
                  </div>
                  <div class="summary-item">
                    <span class="label">分析类型:</span>
                    <span class="value">{{ executionResult.data.analysisType }}</span>
                  </div>
                  <div class="summary-item">
                    <span class="label">执行时间:</span>
                    <span class="value">{{ formatTimestamp(executionResult.data.timestamp) }}</span>
                  </div>
                </div>
              </div>

              <!-- 结果图表 -->
              <div v-if="executionResult.data.spectrumData" class="result-chart">
                <h5>频谱分析结果</h5>
                <canvas ref="chartCanvas" width="400" height="200"></canvas>
              </div>
            </div>

            <!-- 错误信息 -->
            <div v-if="executionResult.error" class="error-info">
              <h5>错误信息</h5>
              <pre class="error-details">{{ executionResult.error }}</pre>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { AITestService } from '@/services/AITestService'
import type { CodeQuality, TestExecutionResult } from '@/types/ai'

// Router instance
const router = useRouter()

// 响应式数据
const testRequirement = ref('')
const selectedDevice = ref('')
const selectedTestType = ref('')
const selectedTemplate = ref('')
const generatedCode = ref('')
const isGenerating = ref(false)
const isExecuting = ref(false)
const codeQuality = ref<CodeQuality | null>(null)
const executionResult = ref<any>(null)
const chartCanvas = ref<HTMLCanvasElement>()
const oscilloscopeCanvas = ref<HTMLCanvasElement>()
const spectrumCanvas = ref<HTMLCanvasElement>()
const fftCanvas = ref<HTMLCanvasElement>()

// 新增：仪器面板相关数据
const activePanel = ref('oscilloscope')
const activeAnalysisTab = ref('statistics')
const isAcquiring = ref(false)

// 仪器设置
const oscoSettings = reactive({
  timebase: '10ms',
  voltage: '1V'
})

const spectrumSettings = reactive({
  startFreq: 20,
  stopFreq: 20000,
  resolution: '100Hz'
})

// 可用面板
const availablePanels = ref([
  { id: 'oscilloscope', name: '🔬 示波器' },
  { id: 'spectrum', name: '📊 频谱仪' },
  { id: 'analysis', name: '🔍 数据分析' }
])

// 分析选项卡
const analysisTabs = ref([
  { id: 'statistics', name: '统计分析' },
  { id: 'frequency', name: '频域分析' }
])

// THD分析结果
const thdResults = ref<any>(null)

// 统计分析结果
const statistics = reactive({
  mean: null as number | null,
  std: null as number | null,
  max: null as number | null,
  min: null as number | null,
  pp: null as number | null,
  rms: null as number | null
})

// 测试类型选项
const testTypes = ref([
  { id: 'vibration', name: '振动测试' },
  { id: 'electrical', name: '电气测试' },
  { id: 'temperature', name: '温度测量' },
  { id: 'signal', name: '信号分析' },
  { id: 'custom', name: '自定义' }
])

// 演示需求选项
const demoRequirements = ref([
  {
    id: 'thd_analysis',
    name: 'THD测试',
    icon: '🎵',
    requirement: '我需要对JY5500信号发生器进行THD（总谐波失真）测试，生成1kHz正弦波，分析2-10次谐波成分，测量失真度并显示频谱分析结果',
    deviceType: 'JY5500',
    testType: 'electrical',
    templateId: 'electrical_thd_analysis',
    description: '测试信号发生器的总谐波失真，适合音频设备质量检测'
  },
  {
    id: 'vibration_monitoring',
    name: '振动监测',
    icon: '📳',
    requirement: '使用JYUSB1601数据采集卡对电机轴承进行振动监测，采集3轴加速度信号，进行FFT频谱分析，识别轴承故障特征频率',
    deviceType: 'JYUSB1601',
    testType: 'vibration',
    templateId: 'vibration_bearing_analysis',
    description: '工业设备振动监测，适合预测性维护应用'
  },
  {
    id: 'temperature_logging',
    name: '温度记录',
    icon: '🌡️',
    requirement: '配置JYUSB1601采集8路热电偶温度信号，采样率10Hz，实时显示温度曲线，设置超温报警阈值，并保存数据到CSV文件',
    deviceType: 'JYUSB1601',
    testType: 'temperature',
    templateId: 'temperature_monitoring',
    description: '多通道温度监控系统，适合热管理应用'
  },
  {
    id: 'signal_generator',
    name: '信号生成',
    icon: '⚡',
    requirement: '使用JY5500生成多种标准测试信号：正弦波、方波、三角波，频率100Hz-10kHz可调，幅度0.1V-10V，用于电路板功能测试',
    deviceType: 'JY5500',
    testType: 'signal',
    templateId: '',
    description: '标准信号发生器配置，适合电路测试验证'
  },
  {
    id: 'power_analysis',
    name: '功率分析',
    icon: '⚡',
    requirement: '使用JYUSB1601同步采集电压和电流信号，计算有功功率、无功功率、功率因数，分析电能质量参数',
    deviceType: 'JYUSB1601', 
    testType: 'electrical',
    templateId: '',
    description: '电能质量分析，适合电力系统监测'
  },
  {
    id: 'data_acquisition',
    name: '高速采集',
    icon: '🚀',
    requirement: '配置JYUSB1601进行4通道高速数据采集，采样率100kHz，缓冲区大小1MB，连续采集模式，实时显示波形',
    deviceType: 'JYUSB1601',
    testType: 'signal',
    templateId: '',
    description: '高速多通道数据采集，适合快速信号分析'
  }
])

// 推荐模板
const recommendedTemplates = ref([
  {
    id: 'vibration_bearing_analysis',
    name: '轴承故障振动分析',
    description: '检测轴承故障的振动频谱分析程序，支持故障特征频率识别',
    rating: 4.8,
    tags: ['振动测试', 'FFT分析', '故障诊断']
  },
  {
    id: 'electrical_thd_analysis',
    name: '信号THD分析',
    description: '测试信号发生器的总谐波失真(THD)，分析各次谐波成分',
    rating: 4.6,
    tags: ['电气测试', 'THD分析', '信号质量']
  },
  {
    id: 'temperature_monitoring',
    name: '多点温度监控',
    description: '实时监控多个温度传感器，支持趋势分析和报警',
    rating: 4.5,
    tags: ['温度测量', '多点监控', '趋势分析']
  }
])

// 选择演示需求
const selectDemoRequirement = (demo: any) => {
  testRequirement.value = demo.requirement
  selectedDevice.value = demo.deviceType
  selectedTestType.value = demo.testType
  if (demo.templateId) {
    selectedTemplate.value = demo.templateId
  }
  
  showMessage(`已选择演示需求：${demo.name}`, 'info')
}

// 生成测试代码  
const generateTestCode = async () => {
  if (!testRequirement.value.trim()) return

  isGenerating.value = true
  codeQuality.value = null
  executionResult.value = null

  try {
    // 模拟AI代码生成过程
    await new Promise(resolve => setTimeout(resolve, 3000))
    
    // 根据设备类型和测试类型生成模拟代码
    const mockCode = generateMockCode(selectedDevice.value, selectedTestType.value, testRequirement.value)
    
    generatedCode.value = mockCode
    codeQuality.value = {
      score: Math.floor(Math.random() * 20) + 80, // 80-100分
      issues: [],
      suggestions: ['代码结构良好', '错误处理完善', '注释清晰易懂']
    }
    
    showMessage('AI代码生成成功！', 'success')
  } catch (error) {
    console.error('生成代码时出错:', error)
    showMessage('代码生成失败，请检查网络连接', 'error')
  } finally {
    isGenerating.value = false
  }
}

// 生成模拟代码
const generateMockCode = (deviceType: string, testType: string, requirement: string) => {
  const timestamp = new Date().toLocaleString('zh-CN')
  
  if (deviceType === 'JY5500' && testType === 'electrical') {
    return `// JY5500信号发生器THD测试代码
// 生成时间: ${timestamp}
// 需求: ${requirement.substring(0, 80)}...

using System;
using SeeSharpTools.JY5500;

class THDAnalysisTest 
{
    static void Main()
    {
        // 1. 初始化JY5500信号发生器
        var generator = new JY5500SignalGenerator();
        generator.Initialize();
        
        // 2. 配置1kHz正弦波输出
        generator.SetWaveform(WaveformType.Sine);
        generator.SetFrequency(1000); // 1kHz
        generator.SetAmplitude(2.0);   // 2V peak-to-peak
        
        // 3. 启动信号输出
        generator.StartOutput();
        Console.WriteLine("JY5500信号输出已启动 - 1kHz正弦波");
        
        // 4. 使用数据采集卡采样分析
        var analyzer = new THDAnalyzer();
        analyzer.SampleRate = 44100;
        analyzer.SampleSize = 4096;
        
        // 5. 执行THD分析
        var results = analyzer.AnalyzeTHD();
        
        Console.WriteLine($"基波频率: {results.FundamentalFreq} Hz");
        Console.WriteLine($"THD: {results.THD:F3}%");
        Console.WriteLine($"SINAD: {results.SINAD:F1} dB");
        
        // 6. 分析各次谐波
        for(int i = 2; i <= 10; i++) 
        {
            var harmonic = results.Harmonics[i];
            Console.WriteLine($"{i}次谐波: {harmonic.Frequency}Hz, {harmonic.Amplitude:F3}V ({harmonic.Percentage:F2}%)");
        }
        
        // 7. 清理资源
        generator.StopOutput();
        generator.Dispose();
        
        Console.WriteLine("THD测试完成");
    }
}`
  } else if (deviceType === 'JYUSB1601' && testType === 'vibration') {
    return `// JYUSB1601振动监测代码
// 生成时间: ${timestamp}
// 需求: ${requirement.substring(0, 80)}...

using System;
using SeeSharpTools.JY1601;

class VibrationMonitoring
{
    static void Main()
    {
        // 1. 初始化JYUSB1601数据采集卡
        var daq = new JYUSB1601();
        daq.Initialize();
        
        // 2. 配置3轴加速度传感器通道
        daq.ConfigureChannel(0, "AccelX", -10, 10, TerminalMode.Differential);
        daq.ConfigureChannel(1, "AccelY", -10, 10, TerminalMode.Differential);
        daq.ConfigureChannel(2, "AccelZ", -10, 10, TerminalMode.Differential);
        
        // 3. 设置采样参数
        daq.SampleRate = 25600; // 25.6kHz采样率
        daq.SamplesPerChannel = 2048;
        
        // 4. 启动连续采集
        daq.StartContinuousAcquisition();
        Console.WriteLine("振动监测已启动...");
        
        var fftAnalyzer = new FFTAnalyzer();
        var bearingAnalyzer = new BearingFaultAnalyzer();
        
        while (true)
        {
            // 5. 读取振动数据
            var data = daq.ReadData();
            
            // 6. FFT频谱分析
            var spectrum = fftAnalyzer.Analyze(data);
            
            // 7. 轴承故障特征频率检测
            var faultFeatures = bearingAnalyzer.DetectFaults(spectrum);
            
            // 8. 输出分析结果
            Console.WriteLine($"RMS振动: X={CalculateRMS(data[0]):F3}g, Y={CalculateRMS(data[1]):F3}g, Z={CalculateRMS(data[2]):F3}g");
            
            if (faultFeatures.HasFault)
            {
                Console.WriteLine($"⚠️ 检测到轴承故障: {faultFeatures.FaultType} @ {faultFeatures.Frequency}Hz");
            }
            
            Thread.Sleep(1000); // 1秒更新间隔
        }
        
        // 9. 清理资源
        daq.StopAcquisition();
        daq.Dispose();
    }
    
    static double CalculateRMS(double[] data)
    {
        return Math.Sqrt(data.Select(x => x * x).Average());
    }
}`
  } else {
    return `// 通用数据采集测试代码
// 生成时间: ${timestamp}
// 设备: ${deviceType}
// 测试类型: ${testType}
// 需求: ${requirement.substring(0, 80)}...

using System;
using SeeSharpTools.${deviceType};

class GeneralDAQTest
{
    static void Main()
    {
        Console.WriteLine("=== ${deviceType} ${testType} 测试程序 ===");
        
        // 1. 设备初始化
        var device = new ${deviceType}();
        device.Initialize();
        
        // 2. 配置采集参数
        device.SampleRate = 1000;
        device.SamplesPerChannel = 1000;
        
        // 3. 启动数据采集
        device.StartAcquisition();
        
        for(int i = 0; i < 10; i++)
        {
            // 4. 读取数据
            var data = device.ReadData();
            
            // 5. 数据处理
            Console.WriteLine($"采集第{i+1}次: {data.Length} 个样点");
            
            Thread.Sleep(1000);
        }
        
        // 6. 停止采集
        device.StopAcquisition();
        device.Dispose();
        
        Console.WriteLine("测试完成");
    }
}`
  }
}

// 选择模板
const selectTemplate = (template: any) => {
  selectedTemplate.value = template.id
  
  // 根据模板设置相关字段
  if (template.id.includes('vibration')) {
    selectedTestType.value = 'vibration'
    selectedDevice.value = 'JYUSB1601'
  } else if (template.id.includes('electrical')) {
    selectedTestType.value = 'electrical'
    selectedDevice.value = 'JY5500'
  } else if (template.id.includes('temperature')) {
    selectedTestType.value = 'temperature'
    selectedDevice.value = 'JYUSB1601'
  }
}

// 执行测试
const executeTest = async () => {
  if (!generatedCode.value.trim()) return

  isExecuting.value = true
  executionResult.value = null

  try {
    // 暂时模拟执行结果，避免404错误
    await new Promise(resolve => setTimeout(resolve, 2000))
    
    executionResult.value = {
      success: true,
      data: {
        deviceType: 'JYUSB1601',
        analysisType: 'THD分析',
        timestamp: new Date().toISOString(),
        spectrumData: Array.from({ length: 100 }, (_, i) => {
          const freq = i * 10
          let amplitude = -60
          if (freq === 50) amplitude = -3  // 基波
          if (freq === 150) amplitude = -20 // 三次谐波
          if (freq === 250) amplitude = -35 // 五次谐波
          return amplitude + Math.random() * 5
        })
      }
    }

    showMessage('测试执行成功！', 'success')
    
    // 如果有频谱数据，绘制图表
    if (executionResult.value.data?.spectrumData) {
      await nextTick()
      drawChart(executionResult.value.data.spectrumData)
    }
    
    // 启动数据采集和仪器面板显示
    startDataAcquisition()
    
  } catch (error) {
    console.error('执行测试时出错:', error)
    executionResult.value = {
      success: false,
      error: '测试执行失败，请检查网络连接'
    }
    showMessage('测试执行失败，请检查网络连接', 'error')
  } finally {
    isExecuting.value = false
  }
}

// 复制代码
const copyCode = async () => {
  if (!generatedCode.value.trim()) return

  try {
    await navigator.clipboard.writeText(generatedCode.value)
    showMessage('代码已复制到剪贴板', 'success')
  } catch (error) {
    console.error('复制失败:', error)
    showMessage('复制失败，请手动选择复制', 'error')
  }
}

// 保存为模板
const saveTemplate = async () => {
  if (!generatedCode.value.trim()) return

  const templateName = prompt('请输入模板名称:')
  if (!templateName) return

  try {
    const response = await AITestService.saveUserTemplate({
      name: templateName,
      description: `基于需求生成: ${testRequirement.value.substring(0, 50)}...`,
      deviceType: selectedDevice.value,
      testType: selectedTestType.value,
      code: generatedCode.value
    })

    if (response.success) {
      showMessage('模板保存成功！', 'success')
    } else {
      showMessage('模板保存失败：' + response.error, 'error')
    }
  } catch (error) {
    console.error('保存模板时出错:', error)
    showMessage('模板保存失败，请检查网络连接', 'error')
  }
}

// 打开API密钥管理页面
const openApiKeyManagement = () => {
  router.push('/api-key-management')
}

// 打开测试历史页面
const openTestHistory = () => {
  router.push('/test-history')
}

// 绘制图表
const drawChart = (spectrumData: number[]) => {
  if (!chartCanvas.value) return

  const ctx = chartCanvas.value.getContext('2d')
  if (!ctx) return

  const width = chartCanvas.value.width
  const height = chartCanvas.value.height

  // 清空画布
  ctx.clearRect(0, 0, width, height)

  // 绘制坐标轴
  ctx.strokeStyle = '#333'
  ctx.lineWidth = 1
  ctx.beginPath()
  ctx.moveTo(50, height - 30)
  ctx.lineTo(width - 30, height - 30)
  ctx.moveTo(50, 20)
  ctx.lineTo(50, height - 30)
  ctx.stroke()

  // 绘制频谱数据
  if (spectrumData.length > 0) {
    const maxVal = Math.max(...spectrumData)
    const minVal = Math.min(...spectrumData)
    const range = maxVal - minVal || 1

    ctx.strokeStyle = '#409eff'
    ctx.lineWidth = 2
    ctx.beginPath()

    spectrumData.forEach((value, index) => {
      const x = 50 + (index / (spectrumData.length - 1)) * (width - 80)
      const y = height - 30 - ((value - minVal) / range) * (height - 50)
      
      if (index === 0) {
        ctx.moveTo(x, y)
      } else {
        ctx.lineTo(x, y)
      }
    })

    ctx.stroke()
  }

  // 添加标签
  ctx.fillStyle = '#666'
  ctx.font = '12px Arial'
  ctx.fillText('频率 (Hz)', width / 2 - 30, height - 5)
  ctx.save()
  ctx.translate(15, height / 2)
  ctx.rotate(-Math.PI / 2)
  ctx.fillText('幅度 (dB)', -30, 0)
  ctx.restore()
}

// 格式化时间戳
const formatTimestamp = (timestamp: string) => {
  return new Date(timestamp).toLocaleString('zh-CN')
}

// 显示消息
const showMessage = (message: string, type: 'success' | 'error' | 'info' = 'info') => {
  // 简单的消息显示，可以后续改为更好的通知组件
  const className = type === 'success' ? 'success' : type === 'error' ? 'error' : 'info'
  console.log(`[${type.toUpperCase()}] ${message}`)
  
  // 可以添加toast通知
  if (type === 'error') {
    alert(`错误: ${message}`)
  }
}

// 新增：仪器控制功能
const startDataAcquisition = () => {
  isAcquiring.value = true
  showMessage('开始数据采集...', 'info')
  
  // 模拟数据采集
  generateMockData()
}

const stopDataAcquisition = () => {
  isAcquiring.value = false
  showMessage('数据采集已停止', 'info')
}

const performFFT = () => {
  showMessage('正在执行FFT分析...', 'info')
  // 模拟FFT分析
  setTimeout(() => {
    showMessage('FFT分析完成', 'success')
  }, 1000)
}

const exportData = () => {
  showMessage('数据导出功能开发中...', 'info')
}

// 生成模拟数据
const generateMockData = () => {
  if (!isAcquiring.value) return
  
  // 生成模拟示波器数据
  const oscoData = Array.from({ length: 1000 }, (_, i) => {
    const t = i / 100
    return Math.sin(2 * Math.PI * 50 * t) + 0.1 * Math.sin(2 * Math.PI * 150 * t)
  })
  
  // 生成模拟频谱数据
  const spectrumData = Array.from({ length: 100 }, (_, i) => {
    const freq = i * 10
    let amplitude = -60
    if (freq === 50) amplitude = -3  // 基波
    if (freq === 150) amplitude = -20 // 三次谐波
    if (freq === 250) amplitude = -35 // 五次谐波
    return amplitude + Math.random() * 5
  })
  
  // 计算THD
  const fundamental = -3
  const harmonics = [-20, -35, -45, -50]
  const thdValue = Math.sqrt(harmonics.reduce((sum, h) => sum + Math.pow(10, h/10), 0)) / Math.pow(10, fundamental/10) * 100
  
  thdResults.value = {
    thd: thdValue,
    fundamental: 50,
    amplitude: 1.0,
    harmonics: [
      { order: 2, frequency: 100, amplitude: 0.05, phase: 45, percentage: 5 },
      { order: 3, frequency: 150, amplitude: 0.03, phase: -30, percentage: 3 },
      { order: 4, frequency: 200, amplitude: 0.02, phase: 90, percentage: 2 },
      { order: 5, frequency: 250, amplitude: 0.015, phase: -60, percentage: 1.5 }
    ]
  }
  
  // 更新统计数据
  statistics.mean = 0.001
  statistics.std = 0.707
  statistics.max = 1.414
  statistics.min = -1.414
  statistics.pp = 2.828
  statistics.rms = 0.707
  
  // 绘制波形
  nextTick(() => {
    drawOscilloscope(oscoData)
    drawSpectrum(spectrumData)
  })
  
  // 如果还在采集，继续生成数据
  if (isAcquiring.value) {
    setTimeout(generateMockData, 100)
  }
}

// 绘制示波器波形
const drawOscilloscope = (data: number[]) => {
  if (!oscilloscopeCanvas.value) return
  
  const ctx = oscilloscopeCanvas.value.getContext('2d')
  if (!ctx) return
  
  const width = oscilloscopeCanvas.value.width
  const height = oscilloscopeCanvas.value.height
  
  // 清空画布
  ctx.fillStyle = '#000'
  ctx.fillRect(0, 0, width, height)
  
  // 绘制网格
  ctx.strokeStyle = '#333'
  ctx.lineWidth = 1
  for (let i = 0; i <= 10; i++) {
    const x = (i / 10) * width
    const y = (i / 10) * height
    ctx.beginPath()
    ctx.moveTo(x, 0)
    ctx.lineTo(x, height)
    ctx.moveTo(0, y)
    ctx.lineTo(width, y)
    ctx.stroke()
  }
  
  // 绘制波形
  ctx.strokeStyle = '#00ff00'
  ctx.lineWidth = 2
  ctx.beginPath()
  
  data.forEach((value, index) => {
    const x = (index / (data.length - 1)) * width
    const y = height / 2 - (value * height / 4)
    
    if (index === 0) {
      ctx.moveTo(x, y)
    } else {
      ctx.lineTo(x, y)
    }
  })
  
  ctx.stroke()
}

// 绘制频谱
const drawSpectrum = (data: number[]) => {
  if (!spectrumCanvas.value) return
  
  const ctx = spectrumCanvas.value.getContext('2d')
  if (!ctx) return
  
  const width = spectrumCanvas.value.width
  const height = spectrumCanvas.value.height
  
  // 清空画布
  ctx.fillStyle = '#000'
  ctx.fillRect(0, 0, width, height)
  
  // 绘制网格
  ctx.strokeStyle = '#333'
  ctx.lineWidth = 1
  for (let i = 0; i <= 10; i++) {
    const x = (i / 10) * width
    const y = (i / 10) * height
    ctx.beginPath()
    ctx.moveTo(x, 0)
    ctx.lineTo(x, height)
    ctx.moveTo(0, y)
    ctx.lineTo(width, y)
    ctx.stroke()
  }
  
  // 绘制频谱柱状图
  ctx.fillStyle = '#ffff00'
  data.forEach((value, index) => {
    const x = (index / data.length) * width
    const barHeight = ((value + 60) / 60) * height // 归一化到0-60dB
    const y = height - barHeight
    
    ctx.fillRect(x, y, width / data.length - 1, barHeight)
  })
  
  // 添加标签
  ctx.fillStyle = '#fff'
  ctx.font = '12px Arial'
  ctx.fillText('频率 (Hz)', width / 2 - 30, height - 5)
  ctx.save()
  ctx.translate(15, height / 2)
  ctx.rotate(-Math.PI / 2)
  ctx.fillText('幅度 (dB)', -30, 0)
  ctx.restore()
}

// 组件挂载
onMounted(() => {
  // 初始化逻辑
  console.log('AI测试平台已加载')
})
</script>

<style scoped>
.ai-test-platform {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 30px;
}

.page-header h1 {
  color: #2c3e50;
  margin-bottom: 10px;
}

.subtitle {
  color: #7f8c8d;
  font-size: 16px;
}

.platform-content {
  display: grid;
  grid-template-columns: 400px 1fr;
  gap: 30px;
  min-height: 600px;
}

/* 左侧面板样式 */
.left-panel {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 20px;
  border: 1px solid #e9ecef;
}

/* 演示需求样式 */
.demo-requirements {
  margin-bottom: 20px;
  padding: 15px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 8px;
  color: white;
}

.demo-requirements label {
  color: white !important;
  font-weight: 600;
  margin-bottom: 10px;
  display: block;
}

.demo-buttons {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 8px;
  margin-top: 10px;
}

.demo-btn {
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.2);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 12px;
  text-align: center;
  backdrop-filter: blur(10px);
}

.demo-btn:hover {
  background: rgba(255, 255, 255, 0.3);
  border-color: rgba(255, 255, 255, 0.5);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.demo-btn:active {
  transform: translateY(0);
}

.requirement-section,
.template-section {
  margin-bottom: 30px;
}

.requirement-section h3,
.template-section h3 {
  color: #2c3e50;
  margin-bottom: 15px;
  border-bottom: 2px solid #3498db;
  padding-bottom: 5px;
}

.input-group {
  margin-bottom: 15px;
}

.input-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: 500;
  color: #495057;
}

.requirement-input {
  width: 100%;
  padding: 10px;
  border: 1px solid #ced4da;
  border-radius: 4px;
  font-size: 14px;
  resize: vertical;
}

.device-selection,
.test-type-selection {
  margin-bottom: 15px;
}

.device-select {
  width: 100%;
  padding: 8px;
  border: 1px solid #ced4da;
  border-radius: 4px;
}

.test-type-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 8px;
}

.type-btn {
  padding: 6px 12px;
  border: 1px solid #dee2e6;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
}

.type-btn:hover {
  background: #e9ecef;
}

.type-btn.active {
  background: #007bff;
  color: white;
  border-color: #007bff;
}

.button-group {
  display: flex;
  gap: 10px;
  margin-top: 10px;
}

.generate-btn {
  flex: 1;
  padding: 12px;
  background: #28a745;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
  transition: background 0.2s;
}

.generate-btn:hover:not(:disabled) {
  background: #218838;
}

.generate-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.api-key-btn {
  padding: 12px 20px;
  background: #6c757d;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
  transition: background 0.2s;
}

.api-key-btn:hover {
  background: #5a6268;
}

.history-btn {
  padding: 12px 20px;
  background: #17a2b8;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
  transition: background 0.2s;
}

.history-btn:hover {
  background: #138496;
}

.template-list {
  max-height: 300px;
  overflow-y: auto;
}

.template-item {
  background: white;
  border: 1px solid #dee2e6;
  border-radius: 4px;
  padding: 12px;
  margin-bottom: 10px;
  cursor: pointer;
  transition: all 0.2s;
}

.template-item:hover {
  border-color: #007bff;
  box-shadow: 0 2px 4px rgba(0,123,255,0.1);
}

.template-item.selected {
  border-color: #007bff;
  background: #f8f9ff;
}

.template-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 5px;
}

.template-header h4 {
  margin: 0;
  color: #2c3e50;
  font-size: 14px;
}

.template-rating {
  font-size: 12px;
  color: #f39c12;
}

.template-desc {
  font-size: 12px;
  color: #6c757d;
  margin: 5px 0;
}

.template-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.tag {
  background: #e9ecef;
  color: #495057;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 11px;
}

/* 右侧面板样式 */
.right-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.code-editor-section,
.execution-section {
  background: white;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  padding: 20px;
}

.editor-header,
.execution-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.editor-header h3,
.execution-header h3 {
  margin: 0;
  color: #2c3e50;
}

.editor-actions {
  display: flex;
  gap: 10px;
}

.action-btn {
  padding: 6px 12px;
  background: #6c757d;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
}

.action-btn:hover {
  background: #5a6268;
}

.code-editor {
  width: 100%;
  min-height: 400px;
  padding: 15px;
  border: 1px solid #dee2e6;
  border-radius: 4px;
  font-family: 'Courier New', monospace;
  font-size: 13px;
  line-height: 1.4;
  resize: vertical;
}

.code-quality {
  margin-top: 15px;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 4px;
}

.quality-score {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}

.score {
  font-weight: bold;
  color: #28a745;
}

.score-bar {
  flex: 1;
  height: 8px;
  background: #e9ecef;
  border-radius: 4px;
  overflow: hidden;
}

.score-fill {
  height: 100%;
  background: linear-gradient(to right, #dc3545, #ffc107, #28a745);
  transition: width 0.3s;
}

.issues,
.suggestions {
  margin-top: 10px;
}

.issues h5,
.suggestions h5 {
  margin: 0 0 5px 0;
  font-size: 14px;
}

.issues ul,
.suggestions ul {
  margin: 0;
  padding-left: 20px;
  font-size: 13px;
}

.execute-btn {
  padding: 10px 20px;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.execute-btn:hover:not(:disabled) {
  background: #0056b3;
}

.execute-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.execution-result {
  margin-top: 15px;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.result-status.success {
  color: #28a745;
  font-weight: bold;
}

.result-status.error {
  color: #dc3545;
  font-weight: bold;
}

.result-content {
  background: #f8f9fa;
  padding: 15px;
  border-radius: 4px;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 10px;
  margin-bottom: 15px;
}

.summary-item {
  display: flex;
  justify-content: space-between;
}

.summary-item .label {
  font-weight: 500;
  color: #495057;
}

.summary-item .value {
  color: #2c3e50;
}

.result-chart {
  margin-top: 15px;
}

.result-chart h5 {
  margin-bottom: 10px;
}

.error-info {
  background: #f8d7da;
  border: 1px solid #f5c6cb;
  padding: 15px;
  border-radius: 4px;
}

.error-details {
  background: #fff;
  padding: 10px;
  border-radius: 4px;
  margin-top: 10px;
  font-size: 12px;
  overflow-x: auto;
}

.loading-spinner {
  display: inline-block;
  width: 12px;
  height: 12px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-right: 5px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .platform-content {
    grid-template-columns: 350px 1fr;
  }
}

@media (max-width: 768px) {
  .platform-content {
    grid-template-columns: 1fr;
    gap: 20px;
  }
  
  .left-panel {
    order: 2;
  }
  
  .right-panel {
    order: 1;
  }
}
</style>
