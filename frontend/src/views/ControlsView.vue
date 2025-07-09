<template>
  <div class="controls-view">
    <div class="page-header">
      <h1 class="page-title">控制控件</h1>
      <p class="page-subtitle">专业控制组件，支持精密调节和用户交互操作</p>
    </div>

    <div class="controls-grid">
      <!-- 旋钮控件 -->
      <div class="control-card">
        <div class="card-header">
          <h3 class="card-title">精密旋钮</h3>
          <p class="card-description">高精度旋转控制，支持细调和粗调</p>
        </div>
        <div class="card-content">
          <div class="knob-demo">
            <div class="knob-container">
              <div class="knob" @mousedown="startKnobDrag" :style="{ transform: `rotate(${knobAngle}deg)` }">
                <div class="knob-indicator"></div>
              </div>
              <div class="knob-value">{{ knobValue.toFixed(2) }}</div>
              <div class="knob-unit">Hz</div>
            </div>
          </div>
        </div>
      </div>

      <!-- 滑块控件 -->
      <div class="control-card">
        <div class="card-header">
          <h3 class="card-title">范围滑块</h3>
          <p class="card-description">线性范围调节控件</p>
        </div>
        <div class="card-content">
          <div class="slider-demo">
            <div class="slider-group">
              <div class="slider-item">
                <label>频率</label>
                <div class="slider-container">
                  <input 
                    type="range" 
                    v-model="frequency" 
                    min="0" 
                    max="1000" 
                    class="slider"
                  />
                  <div class="slider-value">{{ frequency }} Hz</div>
                </div>
              </div>
              <div class="slider-item">
                <label>幅度</label>
                <div class="slider-container">
                  <input 
                    type="range" 
                    v-model="amplitude" 
                    min="0" 
                    max="10" 
                    step="0.1" 
                    class="slider"
                  />
                  <div class="slider-value">{{ amplitude }} V</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 按钮阵列 -->
      <div class="control-card">
        <div class="card-header">
          <h3 class="card-title">按钮阵列</h3>
          <p class="card-description">多功能按钮组合控制</p>
        </div>
        <div class="card-content">
          <div class="button-array-demo">
            <div class="button-grid">
              <button 
                v-for="(btn, index) in buttonStates" 
                :key="index"
                :class="['array-button', { active: btn.active }]"
                @click="toggleButton(index)"
              >
                {{ btn.label }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 开关控件 -->
      <div class="control-card">
        <div class="card-header">
          <h3 class="card-title">开关控件</h3>
          <p class="card-description">多种样式的开关和切换控件</p>
        </div>
        <div class="card-content">
          <div class="switch-demo">
            <div class="switch-group">
              <div class="switch-item">
                <label>电源开关</label>
                <div class="toggle-switch" :class="{ active: powerSwitch }" @click="powerSwitch = !powerSwitch">
                  <div class="switch-handle"></div>
                </div>
              </div>
              <div class="switch-item">
                <label>自动模式</label>
                <div class="toggle-switch" :class="{ active: autoMode }" @click="autoMode = !autoMode">
                  <div class="switch-handle"></div>
                </div>
              </div>
              <div class="switch-item">
                <label>报警使能</label>
                <div class="toggle-switch" :class="{ active: alarmEnable }" @click="alarmEnable = !alarmEnable">
                  <div class="switch-handle"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 数值输入控件 -->
      <div class="control-card">
        <div class="card-header">
          <h3 class="card-title">数值输入</h3>
          <p class="card-description">精密数值输入和调节控件</p>
        </div>
        <div class="card-content">
          <div class="numeric-demo">
            <div class="numeric-group">
              <div class="numeric-item">
                <label>设定值</label>
                <div class="numeric-input">
                  <button class="numeric-btn" @click="decreaseValue">-</button>
                  <input type="number" v-model="numericValue" step="0.01" />
                  <button class="numeric-btn" @click="increaseValue">+</button>
                  <span class="numeric-unit">V</span>
                </div>
              </div>
              <div class="numeric-item">
                <label>步进值</label>
                <div class="numeric-input">
                  <button class="numeric-btn" @click="decreaseStep">-</button>
                  <input type="number" v-model="stepValue" step="0.001" min="0.001" />
                  <button class="numeric-btn" @click="increaseStep">+</button>
                  <span class="numeric-unit">V</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 方向控制 -->
      <div class="control-card">
        <div class="card-header">
          <h3 class="card-title">方向控制</h3>
          <p class="card-description">多方向控制和导航控件</p>
        </div>
        <div class="card-content">
          <div class="direction-demo">
            <div class="direction-pad">
              <button class="dir-btn dir-up" @click="handleDirection('up')">
                <el-icon><ArrowUp /></el-icon>
              </button>
              <button class="dir-btn dir-left" @click="handleDirection('left')">
                <el-icon><ArrowLeft /></el-icon>
              </button>
              <button class="dir-btn dir-center" @click="handleDirection('center')">
                <el-icon><Aim /></el-icon>
              </button>
              <button class="dir-btn dir-right" @click="handleDirection('right')">
                <el-icon><ArrowRight /></el-icon>
              </button>
              <button class="dir-btn dir-down" @click="handleDirection('down')">
                <el-icon><ArrowDown /></el-icon>
              </button>
            </div>
            <div class="direction-status">
              当前方向: {{ currentDirection }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { ArrowUp, ArrowDown, ArrowLeft, ArrowRight, Aim } from '@element-plus/icons-vue'

// 响应式数据
const knobAngle = ref(45)
const knobValue = ref(125.5)
const frequency = ref(500)
const amplitude = ref(5.0)
const powerSwitch = ref(true)
const autoMode = ref(false)
const alarmEnable = ref(true)
const numericValue = ref(12.34)
const stepValue = ref(0.01)
const currentDirection = ref('center')

// 按钮状态
const buttonStates = ref([
  { label: 'CH1', active: true },
  { label: 'CH2', active: false },
  { label: 'CH3', active: true },
  { label: 'CH4', active: false },
  { label: 'TRIG', active: false },
  { label: 'AUTO', active: true },
  { label: 'SINGLE', active: false },
  { label: 'STOP', active: false }
])

// 方法
const startKnobDrag = (event: MouseEvent) => {
  // 旋钮拖拽逻辑（简化版）
  console.log('Knob drag started')
}

const toggleButton = (index: number) => {
  buttonStates.value[index].active = !buttonStates.value[index].active
}

const decreaseValue = () => {
  numericValue.value = Math.max(0, numericValue.value - stepValue.value)
}

const increaseValue = () => {
  numericValue.value = numericValue.value + stepValue.value
}

const decreaseStep = () => {
  stepValue.value = Math.max(0.001, stepValue.value - 0.001)
}

const increaseStep = () => {
  stepValue.value = stepValue.value + 0.001
}

const handleDirection = (direction: string) => {
  currentDirection.value = direction
}
</script>

<style lang="scss" scoped>
@import "@/styles/variables.scss";

.controls-view {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 48px;
  
  .page-title {
    font-size: 36px;
    font-weight: 700;
    color: var(--text-primary);
    margin-bottom: 12px;
  }
  
  .page-subtitle {
    font-size: 18px;
    color: var(--text-secondary);
    line-height: 1.6;
  }
}

.controls-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 24px;
}

.control-card {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 24px;
  transition: var(--transition-base);

  &:hover {
    box-shadow: var(--shadow-medium);
    transform: translateY(-2px);
  }

  .card-header {
    margin-bottom: 20px;

    .card-title {
      font-size: 18px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 8px;
    }

    .card-description {
      font-size: 14px;
      color: var(--text-secondary);
      line-height: 1.5;
    }
  }

  .card-content {
    min-height: 150px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

// 旋钮控件样式
.knob-demo {
  width: 100%;
  text-align: center;

  .knob-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 12px;
  }

  .knob {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    background: linear-gradient(145deg, #f0f0f0, #d0d0d0);
    border: 3px solid var(--border-color);
    position: relative;
    cursor: pointer;
    transition: transform 0.1s ease;

    &:hover {
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    }

    .knob-indicator {
      position: absolute;
      top: 8px;
      left: 50%;
      transform: translateX(-50%);
      width: 4px;
      height: 20px;
      background: var(--primary-color);
      border-radius: 2px;
    }
  }

  .knob-value {
    font-size: 18px;
    font-weight: 600;
    color: var(--text-primary);
  }

  .knob-unit {
    font-size: 14px;
    color: var(--text-secondary);
  }
}

// 滑块控件样式
.slider-demo {
  width: 100%;

  .slider-group {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .slider-item {
    label {
      display: block;
      font-size: 14px;
      color: var(--text-secondary);
      margin-bottom: 8px;
    }

    .slider-container {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .slider {
      flex: 1;
      height: 6px;
      border-radius: 3px;
      background: var(--border-color);
      outline: none;
      appearance: none;

      &::-webkit-slider-thumb {
        appearance: none;
        width: 20px;
        height: 20px;
        border-radius: 50%;
        background: var(--primary-color);
        cursor: pointer;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      }

      &::-moz-range-thumb {
        width: 20px;
        height: 20px;
        border-radius: 50%;
        background: var(--primary-color);
        cursor: pointer;
        border: none;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      }
    }

    .slider-value {
      font-size: 14px;
      font-weight: 500;
      color: var(--text-primary);
      min-width: 80px;
      text-align: right;
    }
  }
}

// 按钮阵列样式
.button-array-demo {
  width: 100%;

  .button-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 8px;
  }

  .array-button {
    padding: 12px 8px;
    border: 2px solid var(--border-color);
    border-radius: 6px;
    background: var(--surface-color);
    color: var(--text-secondary);
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;

    &:hover {
      border-color: var(--primary-color);
      color: var(--primary-color);
    }

    &.active {
      background: var(--primary-color);
      border-color: var(--primary-color);
      color: white;
      box-shadow: 0 2px 4px rgba(46, 134, 171, 0.3);
    }
  }
}

// 开关控件样式
.switch-demo {
  width: 100%;

  .switch-group {
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .switch-item {
    display: flex;
    justify-content: space-between;
    align-items: center;

    label {
      font-size: 14px;
      color: var(--text-secondary);
    }

    .toggle-switch {
      width: 50px;
      height: 26px;
      border-radius: 13px;
      background: var(--border-color);
      position: relative;
      cursor: pointer;
      transition: background 0.3s ease;

      &.active {
        background: var(--primary-color);

        .switch-handle {
          transform: translateX(24px);
        }
      }

      .switch-handle {
        position: absolute;
        top: 2px;
        left: 2px;
        width: 22px;
        height: 22px;
        border-radius: 50%;
        background: white;
        transition: transform 0.3s ease;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      }
    }
  }
}

// 数值输入样式
.numeric-demo {
  width: 100%;

  .numeric-group {
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .numeric-item {
    label {
      display: block;
      font-size: 14px;
      color: var(--text-secondary);
      margin-bottom: 8px;
    }

    .numeric-input {
      display: flex;
      align-items: center;
      border: 2px solid var(--border-color);
      border-radius: 6px;
      overflow: hidden;

      .numeric-btn {
        width: 32px;
        height: 32px;
        border: none;
        background: var(--surface-color);
        color: var(--text-secondary);
        cursor: pointer;
        transition: all 0.2s ease;

        &:hover {
          background: var(--primary-color);
          color: white;
        }
      }

      input {
        flex: 1;
        height: 32px;
        border: none;
        outline: none;
        text-align: center;
        font-size: 14px;
        color: var(--text-primary);
        background: transparent;
      }

      .numeric-unit {
        padding: 0 8px;
        font-size: 12px;
        color: var(--text-secondary);
        background: var(--background-color);
      }
    }
  }
}

// 方向控制样式
.direction-demo {
  width: 100%;
  text-align: center;

  .direction-pad {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    grid-template-rows: repeat(3, 1fr);
    gap: 4px;
    width: 120px;
    height: 120px;
    margin: 0 auto 16px;

    .dir-btn {
      border: 2px solid var(--border-color);
      border-radius: 6px;
      background: var(--surface-color);
      color: var(--text-secondary);
      cursor: pointer;
      transition: all 0.2s ease;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;

      &:hover {
        border-color: var(--primary-color);
        color: var(--primary-color);
        background: rgba(46, 134, 171, 0.1);
      }

      &:active {
        background: var(--primary-color);
        color: white;
      }

      &.dir-up { grid-column: 2; grid-row: 1; }
      &.dir-left { grid-column: 1; grid-row: 2; }
      &.dir-center { grid-column: 2; grid-row: 2; }
      &.dir-right { grid-column: 3; grid-row: 2; }
      &.dir-down { grid-column: 2; grid-row: 3; }
    }
  }

  .direction-status {
    font-size: 14px;
    color: var(--text-secondary);
  }
}

// 响应式设计
@media (max-width: 768px) {
  .controls-grid {
    grid-template-columns: 1fr;
  }

  .control-card {
    padding: 16px;
  }

  .page-header {
    .page-title {
      font-size: 28px;
    }

    .page-subtitle {
      font-size: 16px;
    }
  }

  .button-array-demo .button-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
