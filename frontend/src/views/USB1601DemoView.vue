<template>
  <div class="usb1601-demo-view">
    <!-- 页面标题 -->
    <div class="page-header">
      <div class="header-content">
        <div class="title-section">
          <h1 class="page-title">
            <el-icon class="title-icon"><Cpu /></el-icon>
            简仪科技 USB-1601 数据采集系统
          </h1>
          <p class="page-subtitle">
            专业级USB数据采集卡演示 - 支持AI/AO/DI/DO多功能测量与控制
          </p>
        </div>
        <div class="action-section">
          <el-button-group>
            <el-button type="primary" size="large" @click="scrollToDemo">
              <el-icon><VideoPlay /></el-icon>
              开始演示
            </el-button>
            <el-button size="large" @click="showSpecs = true">
              <el-icon><Document /></el-icon>
              技术规格
            </el-button>
            <el-button size="large" @click="showDocumentation">
              <el-icon><Reading /></el-icon>
              使用文档
            </el-button>
          </el-button-group>
        </div>
      </div>
    </div>

    <!-- 产品特性介绍 -->
    <div class="features-section">
      <div class="container">
        <h2 class="section-title">产品特性</h2>
        <el-row :gutter="24">
          <el-col :span="6">
            <div class="feature-card">
              <div class="feature-icon">
                <el-icon><DataAnalysis /></el-icon>
              </div>
              <h3>AI - 模拟输入</h3>
              <p>16路差分/32路单端模拟输入<br/>16位分辨率，1MS/s采样率</p>
              <ul>
                <li>± 10V, ± 5V, ± 2V, ± 1V量程</li>
                <li>高精度低噪声设计</li>
                <li>软件可编程增益</li>
              </ul>
            </div>
          </el-col>
          <el-col :span="6">
            <div class="feature-card">
              <div class="feature-icon">
                <el-icon><Connection /></el-icon>
              </div>
              <h3>AO - 模拟输出</h3>
              <p>2路模拟输出通道<br/>16位分辨率，1MS/s更新率</p>
              <ul>
                <li>± 10V输出范围</li>
                <li>低失真输出设计</li>
                <li>任意波形生成</li>
              </ul>
            </div>
          </el-col>
          <el-col :span="6">
            <div class="feature-card">
              <div class="feature-icon">
                <el-icon><SwitchButton /></el-icon>
              </div>
              <h3>DI/DO - 数字IO</h3>
              <p>8路数字输入/输出<br/>TTL/CMOS兼容电平</p>
              <ul>
                <li>5V TTL/CMOS兼容</li>
                <li>可配置输入输出方向</li>
                <li>高速数字信号处理</li>
              </ul>
            </div>
          </el-col>
          <el-col :span="6">
            <div class="feature-card">
              <div class="feature-icon">
                <el-icon><Monitor /></el-icon>
              </div>
              <h3>Web集成</h3>
              <p>现代化Web界面<br/>实时数据可视化</p>
              <ul>
                <li>实时波形显示</li>
                <li>远程监控控制</li>
                <li>数据分析导出</li>
              </ul>
            </div>
          </el-col>
        </el-row>
      </div>
    </div>

    <!-- 演示区域 -->
    <div class="demo-section" ref="demoSectionRef">
      <div class="container">
        <h2 class="section-title">实时演示</h2>
        <!-- 演示说明 -->
        <div class="demo-description">
          <el-alert
            title="USB-1601 数据采集演示"
            description="本演示展示了简仪科技USB-1601数据采集卡的完整功能，支持AI模拟输入、AO模拟输出、数字IO控制和C# Runner代码执行。"
            type="success"
            :closable="false"
            show-icon
            style="margin-bottom: 24px"
          />
        </div>
        <div class="demo-container">
          <USB1601Demo />
        </div>
      </div>
    </div>

    <!-- 应用场景 -->
    <div class="applications-section">
      <div class="container">
        <h2 class="section-title">应用场景</h2>
        <el-row :gutter="24">
          <el-col :span="8">
            <div class="application-card">
              <div class="app-icon">
                <el-icon><DataBoard /></el-icon>
              </div>
              <h3>工业自动化</h3>
              <p>传感器信号采集、过程控制、设备监测、质量检测等工业自动化应用场景。</p>
            </div>
          </el-col>
          <el-col :span="8">
            <div class="application-card">
              <div class="app-icon">
                <el-icon><Cpu /></el-icon>
              </div>
              <h3>科研测试</h3>
              <p>实验数据采集、信号分析、科学测量、教学演示等科研教育应用。</p>
            </div>
          </el-col>
          <el-col :span="8">
            <div class="application-card">
              <div class="app-icon">
                <el-icon><Tools /></el-icon>
              </div>
              <h3>设备测试</h3>
              <p>产品测试、质量检验、性能评估、故障诊断等设备测试应用。</p>
            </div>
          </el-col>
        </el-row>
      </div>
    </div>

    <!-- 技术支持 -->
    <div class="support-section">
      <div class="container">
        <h2 class="section-title">技术支持</h2>
        <el-row :gutter="24">
          <el-col :span="8">
            <div class="support-card">
              <el-icon class="support-icon"><Document /></el-icon>
              <h3>完整文档</h3>
              <p>提供详细的用户手册、API文档、示例代码和最佳实践指南。</p>
              <el-button type="primary" text @click="showDocumentation">
                查看文档 <el-icon><ArrowRight /></el-icon>
              </el-button>
            </div>
          </el-col>
          <el-col :span="8">
            <div class="support-card">
              <el-icon class="support-icon"><Code /></el-icon>
              <h3>示例代码</h3>
              <p>提供C#、Python、LabVIEW等多种语言的示例代码和开发包。</p>
              <el-button type="primary" text @click="viewSampleCode">
                示例代码 <el-icon><ArrowRight /></el-icon>
              </el-button>
            </div>
          </el-col>
          <el-col :span="8">
            <div class="support-card">
              <el-icon class="support-icon"><Service /></el-icon>
              <h3>技术服务</h3>
              <p>专业技术团队提供技术咨询、定制开发和售后支持服务。</p>
              <el-button type="primary" text @click="contactSupport">
                联系我们 <el-icon><ArrowRight /></el-icon>
              </el-button>
            </div>
          </el-col>
        </el-row>
      </div>
    </div>

    <!-- 技术规格对话框 -->
    <el-dialog 
      v-model="showSpecs" 
      title="USB-1601 技术规格"
      width="70%"
      :before-close="handleSpecsClose"
    >
      <div class="specs-content">
        <el-row :gutter="24">
          <el-col :span="12">
            <h4>模拟输入 (AI)</h4>
            <el-table :data="aiSpecs" border>
              <el-table-column prop="parameter" label="参数" width="120" />
              <el-table-column prop="specification" label="规格" />
            </el-table>
          </el-col>
          <el-col :span="12">
            <h4>模拟输出 (AO)</h4>
            <el-table :data="aoSpecs" border>
              <el-table-column prop="parameter" label="参数" width="120" />
              <el-table-column prop="specification" label="规格" />
            </el-table>
          </el-col>
        </el-row>
        <el-row :gutter="24" style="margin-top: 20px;">
          <el-col :span="12">
            <h4>数字IO</h4>
            <el-table :data="dioSpecs" border>
              <el-table-column prop="parameter" label="参数" width="120" />
              <el-table-column prop="specification" label="规格" />
            </el-table>
          </el-col>
          <el-col :span="12">
            <h4>系统规格</h4>
            <el-table :data="systemSpecs" border>
              <el-table-column prop="parameter" label="参数" width="120" />
              <el-table-column prop="specification" label="规格" />
            </el-table>
          </el-col>
        </el-row>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import {
  Cpu,
  VideoPlay,
  Document,
  Reading,
  DataAnalysis,
  Connection,
  SwitchButton,
  Monitor,
  DataBoard,
  Tools,
  Service,
  ArrowRight
} from '@element-plus/icons-vue'
import USB1601Demo from '@/components/instruments/USB1601Demo.vue'

// 响应式数据
const showSpecs = ref(false)
const demoSectionRef = ref()

// 技术规格数据
const aiSpecs = [
  { parameter: '通道数', specification: '16路差分/32路单端' },
  { parameter: '分辨率', specification: '16位' },
  { parameter: '采样率', specification: '最高1MS/s' },
  { parameter: '输入量程', specification: '±10V, ±5V, ±2V, ±1V' },
  { parameter: '输入阻抗', specification: '>1GΩ' },
  { parameter: '精度', specification: '±0.02% of FSR' }
]

const aoSpecs = [
  { parameter: '通道数', specification: '2路' },
  { parameter: '分辨率', specification: '16位' },
  { parameter: '更新率', specification: '最高1MS/s' },
  { parameter: '输出范围', specification: '±10V' },
  { parameter: '精度', specification: '±0.02% of FSR' },
  { parameter: '建立时间', specification: '<10μs' }
]

const dioSpecs = [
  { parameter: '通道数', specification: '8路可配置' },
  { parameter: '电平标准', specification: 'TTL/CMOS' },
  { parameter: '输入电压', specification: '0-5V' },
  { parameter: '输出电压', specification: '0-5V' },
  { parameter: '驱动能力', specification: '24mA' },
  { parameter: '更新率', specification: '10MHz' }
]

const systemSpecs = [
  { parameter: '接口', specification: 'USB 2.0/3.0' },
  { parameter: '供电', specification: 'USB总线供电' },
  { parameter: '工作温度', specification: '0°C ~ 50°C' },
  { parameter: '存储温度', specification: '-20°C ~ 70°C' },
  { parameter: '湿度', specification: '5% ~ 95% RH' },
  { parameter: '尺寸', specification: '120×80×25mm' }
]

// 页面方法
function scrollToDemo() {
  if (demoSectionRef.value) {
    demoSectionRef.value.scrollIntoView({ 
      behavior: 'smooth',
      block: 'start'
    })
  }
}

function handleSpecsClose(done: () => void) {
  done()
}

function showDocumentation() {
  ElMessage.info('正在跳转到文档页面...')
  // 这里可以跳转到实际的文档页面
  window.open('/documentation/usb1601', '_blank')
}

function viewSampleCode() {
  ElMessage.info('正在打开代码示例...')
  // 这里可以跳转到代码示例页面
  window.open('/samples/usb1601', '_blank')
}

function contactSupport() {
  ElMessage.info('正在跳转到联系页面...')
  // 这里可以跳转到联系支持页面
  window.open('mailto:support@jytek.com', '_blank')
}

onMounted(() => {
  // 页面加载完成后的初始化操作
})
</script>

<style scoped>
.usb1601-demo-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.page-header {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
  padding: 60px 0;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
}

.title-section h1.page-title {
  display: flex;
  align-items: center;
  gap: 12px;
  margin: 0 0 8px 0;
  font-size: 32px;
  font-weight: 700;
  color: #1a1a1a;
  line-height: 1.2;
}

.title-icon {
  color: #667eea;
  font-size: 36px;
}

.page-subtitle {
  margin: 0;
  font-size: 16px;
  color: #666;
  line-height: 1.6;
}

.action-section {
  flex-shrink: 0;
}

/* 特性部分 */
.features-section {
  padding: 80px 0;
  background: rgba(255, 255, 255, 0.95);
}

.section-title {
  text-align: center;
  font-size: 28px;
  font-weight: 600;
  color: #333;
  margin: 0 0 40px 0;
}

.feature-card {
  background: white;
  border-radius: 12px;
  padding: 30px 20px;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  height: 100%;
}

.feature-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.15);
}

.feature-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 50%;
  margin-bottom: 20px;
}

.feature-icon .el-icon {
  font-size: 32px;
  color: white;
}

.feature-card h3 {
  margin: 0 0 12px 0;
  font-size: 18px;
  font-weight: 600;
  color: #333;
}

.feature-card p {
  margin: 0 0 16px 0;
  color: #666;
  line-height: 1.5;
}

.feature-card ul {
  list-style: none;
  padding: 0;
  margin: 0;
  text-align: left;
}

.feature-card ul li {
  position: relative;
  padding-left: 20px;
  margin-bottom: 6px;
  color: #777;
  font-size: 14px;
}

.feature-card ul li::before {
  content: '•';
  position: absolute;
  left: 0;
  color: #667eea;
  font-weight: bold;
}

/* 演示部分 */
.demo-section {
  padding: 80px 0;
  background: rgba(255, 255, 255, 0.9);
}

.demo-mode-selector {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-bottom: 24px;
}

.demo-container {
  max-width: none;
  margin: 0 -24px;
}

/* 应用场景 */
.applications-section {
  padding: 80px 0;
  background: rgba(255, 255, 255, 0.95);
}

.application-card {
  background: white;
  border-radius: 12px;
  padding: 30px;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  height: 100%;
}

.application-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.12);
}

.app-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  background: linear-gradient(45deg, #667eea, #764ba2);
  border-radius: 50%;
  margin-bottom: 20px;
}

.app-icon .el-icon {
  font-size: 28px;
  color: white;
}

.application-card h3 {
  margin: 0 0 12px 0;
  font-size: 18px;
  font-weight: 600;
  color: #333;
}

.application-card p {
  margin: 0;
  color: #666;
  line-height: 1.6;
}

/* 技术支持 */
.support-section {
  padding: 80px 0;
  background: rgba(255, 255, 255, 0.9);
}

.support-card {
  background: white;
  border-radius: 12px;
  padding: 30px;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  height: 100%;
}

.support-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.12);
}

.support-icon {
  font-size: 48px;
  color: #667eea;
  margin-bottom: 20px;
}

.support-card h3 {
  margin: 0 0 12px 0;
  font-size: 18px;
  font-weight: 600;
  color: #333;
}

.support-card p {
  margin: 0 0 20px 0;
  color: #666;
  line-height: 1.6;
}

/* 规格对话框 */
.specs-content h4 {
  margin: 0 0 16px 0;
  font-size: 16px;
  font-weight: 600;
  color: #333;
}

.specs-content .el-table {
  margin-bottom: 0;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .header-content {
    flex-direction: column;
    gap: 24px;
    text-align: center;
  }
  
  .demo-container {
    margin: 0;
  }
}

@media (max-width: 768px) {
  .container {
    padding: 0 16px;
  }
  
  .page-header {
    padding: 40px 0;
  }
  
  .title-section h1.page-title {
    font-size: 24px;
  }
  
  .features-section,
  .demo-section,
  .applications-section,
  .support-section {
    padding: 60px 0;
  }
  
  .section-title {
    font-size: 24px;
    margin-bottom: 30px;
  }
  
  .feature-card,
  .application-card,
  .support-card {
    padding: 24px;
    margin-bottom: 16px;
  }
}

@media (max-width: 480px) {
  .header-content {
    padding: 0 16px;
  }
  
  .action-section .el-button-group {
    flex-direction: column;
    width: 100%;
  }
  
  .action-section .el-button {
    width: 100%;
    margin: 2px 0 !important;
  }
}
</style>
