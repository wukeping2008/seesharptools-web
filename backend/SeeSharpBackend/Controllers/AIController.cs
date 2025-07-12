using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeeSharpBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIController> _logger;

        public AIController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<AIController> logger)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var apiKey = _configuration["Claude:ApiKey"];
            var hasApiKey = !string.IsNullOrEmpty(apiKey);
            
            return Ok(new { hasApiKey });
        }

        [HttpPost("generate-control")]
        public async Task<IActionResult> GenerateControl([FromBody] GenerateControlRequest request)
        {
            try
            {
                // 获取Claude API配置
                var apiKey = _configuration["Claude:ApiKey"];
                var apiUrl = _configuration["Claude:ApiUrl"] ?? "https://api.anthropic.com/v1/messages";

                // 如果有API密钥，使用Claude API
                if (!string.IsNullOrEmpty(apiKey))
                {
                    return await GenerateWithClaudeAPI(request, apiKey, apiUrl);
                }

                // 否则使用预定义模板
                return GenerateWithTemplate(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成控件时发生错误");
                return StatusCode(500, new { success = false, error = "服务器内部错误" });
            }
        }

        private async Task<IActionResult> GenerateWithClaudeAPI(GenerateControlRequest request, string apiKey, string apiUrl)
        {
            // 构建系统提示词
            var systemPrompt = @"你是一个Vue 3组件生成专家。用户会描述他们需要的测试测量仪器控件，你需要生成完整的Vue 3组件代码。

要求：
1. 使用Vue 3 Composition API (<script setup lang=""ts"">)
2. 使用TypeScript
3. 包含完整的template、script和style部分
4. 生成专业的测试测量仪器风格的控件
5. 确保代码可以直接使用，不需要额外依赖
6. 使用内联SVG或CSS来创建视觉效果
7. 包含必要的props接口定义
8. 添加适当的动画和交互效果

只返回Vue组件代码，不要包含任何解释或其他内容。";

            // 构建请求体
            var requestBody = new
            {
                model = "claude-3-sonnet-20240229",
                max_tokens = 4000,
                temperature = 0.7,
                system = systemPrompt,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $"请生成一个Vue 3组件：{request.Description}"
                    }
                }
            };

            // 设置请求头
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            // 发送请求
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Claude API错误: {response.StatusCode} - {responseContent}");
                return BadRequest(new { success = false, error = $"AI服务错误: {response.StatusCode}" });
            }

            // 解析响应
            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent);
            var generatedCode = claudeResponse?.content?.FirstOrDefault()?.text;

            if (string.IsNullOrEmpty(generatedCode))
            {
                return BadRequest(new { success = false, error = "AI未能生成有效代码" });
            }

            // 清理代码（移除可能的markdown标记）
            generatedCode = CleanGeneratedCode(generatedCode);

            return Ok(new { success = true, code = generatedCode });
        }

        private IActionResult GenerateWithTemplate(GenerateControlRequest request)
        {
            // 根据描述关键词选择合适的模板
            var description = request.Description?.ToLower() ?? "";
            string templateCode;

            if (description.Contains("温度") || description.Contains("仪表") || description.Contains("表盘"))
            {
                templateCode = GetTemperatureGaugeTemplate();
            }
            else if (description.Contains("led") || description.Contains("指示灯") || description.Contains("状态灯"))
            {
                templateCode = GetLEDTemplate();
            }
            else if (description.Contains("按钮") || description.Contains("开关") || description.Contains("控制"))
            {
                templateCode = GetButtonTemplate();
            }
            else
            {
                // 默认返回仪表盘模板
                templateCode = GetTemperatureGaugeTemplate();
            }

            return Ok(new { success = true, code = templateCode });
        }

        private string GetTemperatureGaugeTemplate()
        {
            return @"<template>
  <div class=""temperature-gauge"">
    <div class=""gauge-container"">
      <svg width=""200"" height=""200"" viewBox=""0 0 200 200"">
        <circle cx=""100"" cy=""100"" r=""90"" fill=""none"" stroke=""#e0e0e0"" stroke-width=""4""/>
        <g v-for=""(tick, index) in ticks"" :key=""index"">
          <line 
            :x1=""tick.x1"" 
            :y1=""tick.y1"" 
            :x2=""tick.x2"" 
            :y2=""tick.y2"" 
            stroke=""#666"" 
            :stroke-width=""tick.major ? 2 : 1""
          />
          <text 
            v-if=""tick.major"" 
            :x=""tick.textX"" 
            :y=""tick.textY"" 
            text-anchor=""middle"" 
            dominant-baseline=""middle"" 
            font-size=""12"" 
            fill=""#333""
          >
            {{ tick.value }}
          </text>
        </g>
        <line 
          x1=""100"" 
          y1=""100"" 
          :x2=""needleX"" 
          :y2=""needleY"" 
          stroke=""#ff4444"" 
          stroke-width=""3"" 
          stroke-linecap=""round""
        />
        <circle cx=""100"" cy=""100"" r=""6"" fill=""#ff4444""/>
      </svg>
      <div class=""value-display"">
        <span class=""value"">{{ currentValue.toFixed(1) }}</span>
        <span class=""unit"">°C</span>
      </div>
    </div>
    <div class=""controls"">
      <button @click=""decreaseValue"" class=""control-btn"">-</button>
      <button @click=""increaseValue"" class=""control-btn"">+</button>
    </div>
  </div>
</template>

<script setup lang=""ts"">
import { ref, computed, onMounted } from 'vue'

interface Props {
  min?: number
  max?: number
  value?: number
  unit?: string
}

const props = withDefaults(defineProps<Props>(), {
  min: 0,
  max: 100,
  value: 25,
  unit: '°C'
})

const currentValue = ref(props.value)

const ticks = computed(() => {
  const tickArray = []
  const range = props.max - props.min
  const step = range / 10
  
  for (let i = 0; i <= 10; i++) {
    const value = props.min + i * step
    const angle = (i * 270 / 10) - 135
    const radian = (angle * Math.PI) / 180
    
    const major = i % 2 === 0
    const radius = major ? 75 : 80
    const textRadius = 65
    
    tickArray.push({
      value: Math.round(value),
      major,
      x1: 100 + Math.cos(radian) * 85,
      y1: 100 + Math.sin(radian) * 85,
      x2: 100 + Math.cos(radian) * radius,
      y2: 100 + Math.sin(radian) * radius,
      textX: 100 + Math.cos(radian) * textRadius,
      textY: 100 + Math.sin(radian) * textRadius
    })
  }
  
  return tickArray
})

const needleAngle = computed(() => {
  const percentage = (currentValue.value - props.min) / (props.max - props.min)
  return (percentage * 270) - 135
})

const needleX = computed(() => {
  const radian = (needleAngle.value * Math.PI) / 180
  return 100 + Math.cos(radian) * 70
})

const needleY = computed(() => {
  const radian = (needleAngle.value * Math.PI) / 180
  return 100 + Math.sin(radian) * 70
})

const increaseValue = () => {
  if (currentValue.value < props.max) {
    currentValue.value += 1
  }
}

const decreaseValue = () => {
  if (currentValue.value > props.min) {
    currentValue.value -= 1
  }
}

onMounted(() => {
  setInterval(() => {
    const variation = (Math.random() - 0.5) * 2
    const newValue = currentValue.value + variation
    if (newValue >= props.min && newValue <= props.max) {
      currentValue.value = newValue
    }
  }, 2000)
})
</script>

<style scoped>
.temperature-gauge {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 20px;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 15px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  max-width: 300px;
}

.gauge-container {
  position: relative;
  margin-bottom: 20px;
}

.value-display {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  text-align: center;
  margin-top: 20px;
}

.value {
  font-size: 24px;
  font-weight: bold;
  color: #333;
}

.unit {
  font-size: 14px;
  color: #666;
  margin-left: 4px;
}

.controls {
  display: flex;
  gap: 10px;
}

.control-btn {
  width: 40px;
  height: 40px;
  border: none;
  border-radius: 50%;
  background: #4CAF50;
  color: white;
  font-size: 18px;
  font-weight: bold;
  cursor: pointer;
  transition: all 0.3s ease;
}

.control-btn:hover {
  background: #45a049;
  transform: scale(1.1);
}

.control-btn:active {
  transform: scale(0.95);
}
</style>";
        }

        private string GetLEDTemplate()
        {
            return @"<template>
  <div class=""led-panel"">
    <h3 class=""panel-title"">状态指示灯</h3>
    <div class=""led-grid"">
      <div 
        v-for=""(led, index) in leds"" 
        :key=""index"" 
        class=""led-item""
        @click=""toggleLED(index)""
      >
        <div 
          class=""led-light"" 
          :class=""{ 'led-on': led.state, [`led-${led.color}`]: true }""
        >
          <div class=""led-glow""></div>
        </div>
        <span class=""led-label"">{{ led.label }}</span>
      </div>
    </div>
    
    <div class=""control-panel"">
      <button @click=""toggleAll"" class=""control-button"">
        {{ allOn ? '全部关闭' : '全部开启' }}
      </button>
      <button @click=""testSequence"" class=""control-button"">
        测试序列
      </button>
    </div>
  </div>
</template>

<script setup lang=""ts"">
import { ref, computed } from 'vue'

interface LED {
  label: string
  color: string
  state: boolean
}

const leds = ref<LED[]>([
  { label: '电源', color: 'green', state: true },
  { label: '运行', color: 'blue', state: false },
  { label: '警告', color: 'yellow', state: false },
  { label: '错误', color: 'red', state: false },
  { label: '网络', color: 'cyan', state: true },
  { label: '数据', color: 'purple', state: false }
])

const allOn = computed(() => leds.value.every(led => led.state))

const toggleLED = (index: number) => {
  leds.value[index].state = !leds.value[index].state
}

const toggleAll = () => {
  const newState = !allOn.value
  leds.value.forEach(led => {
    led.state = newState
  })
}

const testSequence = async () => {
  leds.value.forEach(led => led.state = false)
  
  for (let i = 0; i < leds.value.length; i++) {
    await new Promise(resolve => setTimeout(resolve, 300))
    leds.value[i].state = true
  }
  
  await new Promise(resolve => setTimeout(resolve, 1000))
  
  for (let blink = 0; blink < 3; blink++) {
    leds.value.forEach(led => led.state = false)
    await new Promise(resolve => setTimeout(resolve, 200))
    leds.value.forEach(led => led.state = true)
    await new Promise(resolve => setTimeout(resolve, 200))
  }
}
</script>

<style scoped>
.led-panel {
  background: #2c3e50;
  border-radius: 12px;
  padding: 24px;
  color: white;
  max-width: 400px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
}

.panel-title {
  text-align: center;
  margin-bottom: 20px;
  color: #ecf0f1;
  font-size: 18px;
}

.led-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin-bottom: 24px;
}

.led-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  cursor: pointer;
  transition: transform 0.2s ease;
}

.led-item:hover {
  transform: scale(1.05);
}

.led-light {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  position: relative;
  margin-bottom: 8px;
  border: 2px solid #34495e;
  transition: all 0.3s ease;
}

.led-glow {
  position: absolute;
  top: -4px;
  left: -4px;
  right: -4px;
  bottom: -4px;
  border-radius: 50%;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.led-on .led-glow {
  opacity: 0.6;
  animation: pulse 2s infinite;
}

.led-green { background: #27ae60; }
.led-green.led-on { background: #2ecc71; box-shadow: 0 0 20px #2ecc71; }
.led-green .led-glow { background: radial-gradient(circle, #2ecc71, transparent); }

.led-blue { background: #2980b9; }
.led-blue.led-on { background: #3498db; box-shadow: 0 0 20px #3498db; }
.led-blue .led-glow { background: radial-gradient(circle, #3498db, transparent); }

.led-yellow { background: #f39c12; }
.led-yellow.led-on { background: #f1c40f; box-shadow: 0 0 20px #f1c40f; }
.led-yellow .led-glow { background: radial-gradient(circle, #f1c40f, transparent); }

.led-red { background: #c0392b; }
.led-red.led-on { background: #e74c3c; box-shadow: 0 0 20px #e74c3c; }
.led-red .led-glow { background: radial-gradient(circle, #e74c3c, transparent); }

.led-cyan { background: #16a085; }
.led-cyan.led-on { background: #1abc9c; box-shadow: 0 0 20px #1abc9c; }
.led-cyan .led-glow { background: radial-gradient(circle, #1abc9c, transparent); }

.led-purple { background: #8e44ad; }
.led-purple.led-on { background: #9b59b6; box-shadow: 0 0 20px #9b59b6; }
.led-purple .led-glow { background: radial-gradient(circle, #9b59b6, transparent); }

.led-label {
  font-size: 12px;
  color: #bdc3c7;
  text-align: center;
}

.control-panel {
  display: flex;
  gap: 12px;
  justify-content: center;
}

.control-button {
  padding: 8px 16px;
  background: #34495e;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.3s ease;
}

.control-button:hover {
  background: #4a6741;
}

@keyframes pulse {
  0%, 100% { opacity: 0.6; }
  50% { opacity: 1; }
}
</style>";
        }

        private string GetButtonTemplate()
        {
            return @"<template>
  <div class=""control-button-panel"">
    <h3 class=""panel-title"">控制按钮</h3>
    
    <div class=""button-grid"">
      <div class=""button-group"">
        <h4>主要控制</h4>
        <button 
          class=""control-btn primary"" 
          :class=""{ active: states.power }"" 
          @click=""togglePower""
        >
          <span class=""btn-icon"">⚡</span>
          {{ states.power ? '关闭' : '开启' }}
        </button>
        
        <button 
          class=""control-btn success"" 
          :disabled=""!states.power""
          :class=""{ active: states.running }"" 
          @click=""toggleRunning""
        >
          <span class=""btn-icon"">▶</span>
          {{ states.running ? '停止' : '启动' }}
        </button>
      </div>
      
      <div class=""button-group"">
        <h4>功能控制</h4>
        <button 
          class=""control-btn info"" 
          @click=""resetSystem""
        >
          <span class=""btn-icon"">🔄</span>
          重置
        </button>
        
        <button 
          class=""control-btn warning"" 
          @click=""showSettings""
        >
          <span class=""btn-icon"">⚙️</span>
          设置
        </button>
        
        <button 
          class=""control-btn danger"" 
          @click=""emergencyStop""
        >
          <span class=""btn-icon"">🛑</span>
          急停
        </button>
      </div>
    </div>
    
    <div class=""status-bar"">
      <div class=""status-item"" :class=""{ active: states.power }"">
        <span class=""status-dot""></span>
        电源
      </div>
      <div class=""status-item"" :class=""{ active: states.running }"">
        <span class=""status-dot""></span>
        运行
      </div>
      <div class=""status-item"" :class=""{ active: states.connected }"">
        <span class=""status-dot""></span>
        连接
      </div>
    </div>
  </div>
</template>

<script setup lang=""ts"">
import { reactive } from 'vue'

const states = reactive({
  power: false,
  running: false,
  connected: true
})

const togglePower = () => {
  states.power = !states.power
  if (!states.power) {
    states.running = false
  }
}

const toggleRunning = () => {
  if (states.power) {
    states.running = !states.running
  }
}

const resetSystem = () => {
  states.power = false
  states.running = false
  console.log('系统已重置')
}

const showSettings = () => {
  console.log('打开设置面板')
}

const emergencyStop = () => {
  states.power = false
  states.running = false
  console.log('紧急停止！')
}
</script>

<style scoped>
.control-button-panel {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 16px;
  padding: 24px;
  color: white;
  max-width: 500px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
}

.panel-title {
  text-align: center;
  margin-bottom: 24px;
  font-size: 20px;
  font-weight: bold;
}

.button-grid {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-bottom: 20px;
}

.button-group h4 {
  margin-bottom: 12px;
  color: #e0e6ed;
  font-size: 14px;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.control-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 24px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  margin-bottom: 8px;
  min-width: 120px;
}

.btn-icon {
  font-size: 18px;
}

.control-btn.primary {
  background: #3498db;
  color: white;
}

.control-btn.primary:hover {
  background: #2980b9;
  transform: translateY(-2px);
}

.control-btn.primary.active {
  background: #27ae60;
}

.control-btn.success {
  background: #2ecc71;
  color: white;
}

.control-btn.success:hover:not(:disabled) {
  background: #27ae60;
  transform: translateY(-2px);
}

.control-btn.info {
  background: #9b59b6;
  color: white;
}

.control-btn.info:hover {
  background: #8e44ad;
  transform: translateY(-2px);
}

.control-btn.warning {
  background: #f39c12;
  color: white;
}

.control-btn.warning:hover {
  background: #e67e22;
  transform: translateY(-2px);
}

.control-btn.danger {
  background: #e74c3c;
  color: white;
}

.control-btn.danger:hover {
  background: #c0392b;
  transform: translateY(-2px);
}

.status-bar {
  display: flex;
  justify-content: space-around;
  padding: 16px;
  background: rgba(0, 0, 0, 0.2);
  border-radius: 8px;
}

.status-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
}

.status-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #7f8c8d;
  transition: background 0.3s ease;
}

.status-item.active .status-dot {
  background: #2ecc71;
  box-shadow: 0 0 10px #2ecc71;
}
</style>";
        }

        private string CleanGeneratedCode(string code)
        {
            // 移除可能的markdown代码块标记
            code = code.Trim();
            if (code.StartsWith("```vue") || code.StartsWith("```"))
            {
                code = code.Substring(code.IndexOf('\n') + 1);
            }
            if (code.EndsWith("```"))
            {
                code = code.Substring(0, code.LastIndexOf("```"));
            }
            return code.Trim();
        }
    }

    public class GenerateControlRequest
    {
        public string Description { get; set; } = string.Empty;
    }

    public class ClaudeResponse
    {
        public List<ClaudeContent>? content { get; set; }
    }

    public class ClaudeContent
    {
        public string? text { get; set; }
        public string? type { get; set; }
    }
}
