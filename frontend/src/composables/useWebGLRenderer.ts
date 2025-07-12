import { ref, onMounted, onUnmounted, nextTick } from 'vue'

export interface WebGLRendererOptions {
  width: number
  height: number
  antialias?: boolean
  alpha?: boolean
  preserveDrawingBuffer?: boolean
  powerPreference?: 'default' | 'high-performance' | 'low-power'
}

export interface RenderData {
  vertices: Float32Array
  colors?: Float32Array
  indices?: Uint16Array
  lineWidth?: number
  pointSize?: number
}

export function useWebGLRenderer(options: WebGLRendererOptions) {
  const canvasRef = ref<HTMLCanvasElement>()
  const gl = ref<WebGLRenderingContext | null>(null)
  const isInitialized = ref(false)
  const animationId = ref<number>()

  // 着色器程序
  let shaderProgram: WebGLProgram | null = null
  let vertexBuffer: WebGLBuffer | null = null
  let colorBuffer: WebGLBuffer | null = null
  let indexBuffer: WebGLBuffer | null = null

  // 顶点着色器源码
  const vertexShaderSource = `
    attribute vec2 a_position;
    attribute vec4 a_color;
    uniform vec2 u_resolution;
    uniform mat3 u_transform;
    varying vec4 v_color;
    
    void main() {
      vec3 position = u_transform * vec3(a_position, 1.0);
      vec2 clipSpace = ((position.xy / u_resolution) * 2.0) - 1.0;
      gl_Position = vec4(clipSpace * vec2(1, -1), 0, 1);
      gl_PointSize = 2.0;
      v_color = a_color;
    }
  `

  // 片段着色器源码
  const fragmentShaderSource = `
    precision mediump float;
    varying vec4 v_color;
    
    void main() {
      gl_FragColor = v_color;
    }
  `

  // 创建着色器
  const createShader = (type: number, source: string): WebGLShader | null => {
    if (!gl.value) return null
    
    const shader = gl.value.createShader(type)
    if (!shader) return null
    
    gl.value.shaderSource(shader, source)
    gl.value.compileShader(shader)
    
    if (!gl.value.getShaderParameter(shader, gl.value.COMPILE_STATUS)) {
      console.error('着色器编译错误:', gl.value.getShaderInfoLog(shader))
      gl.value.deleteShader(shader)
      return null
    }
    
    return shader
  }

  // 创建着色器程序
  const createShaderProgram = (): WebGLProgram | null => {
    if (!gl.value) return null
    
    const vertexShader = createShader(gl.value.VERTEX_SHADER, vertexShaderSource)
    const fragmentShader = createShader(gl.value.FRAGMENT_SHADER, fragmentShaderSource)
    
    if (!vertexShader || !fragmentShader) return null
    
    const program = gl.value.createProgram()
    if (!program) return null
    
    gl.value.attachShader(program, vertexShader)
    gl.value.attachShader(program, fragmentShader)
    gl.value.linkProgram(program)
    
    if (!gl.value.getProgramParameter(program, gl.value.LINK_STATUS)) {
      console.error('着色器程序链接错误:', gl.value.getProgramInfoLog(program))
      gl.value.deleteProgram(program)
      return null
    }
    
    return program
  }

  // 初始化WebGL
  const initWebGL = (): boolean => {
    if (!canvasRef.value) return false
    
    const context = canvasRef.value.getContext('webgl', {
      antialias: options.antialias ?? true,
      alpha: options.alpha ?? false,
      preserveDrawingBuffer: options.preserveDrawingBuffer ?? false,
      powerPreference: options.powerPreference ?? 'high-performance'
    })
    
    if (!context) {
      console.error('WebGL不支持')
      return false
    }
    
    gl.value = context
    
    // 设置视口
    gl.value.viewport(0, 0, options.width, options.height)
    
    // 创建着色器程序
    shaderProgram = createShaderProgram()
    if (!shaderProgram) return false
    
    // 创建缓冲区
    vertexBuffer = gl.value.createBuffer()
    colorBuffer = gl.value.createBuffer()
    indexBuffer = gl.value.createBuffer()
    
    // 启用混合
    gl.value.enable(gl.value.BLEND)
    gl.value.blendFunc(gl.value.SRC_ALPHA, gl.value.ONE_MINUS_SRC_ALPHA)
    
    isInitialized.value = true
    return true
  }

  // 清除画布
  const clear = (r = 0, g = 0, b = 0, a = 1) => {
    if (!gl.value) return
    
    gl.value.clearColor(r, g, b, a)
    gl.value.clear(gl.value.COLOR_BUFFER_BIT)
  }

  // 渲染线条
  const renderLines = (data: RenderData) => {
    if (!gl.value || !shaderProgram || !isInitialized.value) return
    
    gl.value.useProgram(shaderProgram)
    
    // 设置顶点数据
    if (vertexBuffer && data.vertices) {
      gl.value.bindBuffer(gl.value.ARRAY_BUFFER, vertexBuffer)
      gl.value.bufferData(gl.value.ARRAY_BUFFER, data.vertices, gl.value.DYNAMIC_DRAW)
      
      const positionLocation = gl.value.getAttribLocation(shaderProgram, 'a_position')
      gl.value.enableVertexAttribArray(positionLocation)
      gl.value.vertexAttribPointer(positionLocation, 2, gl.value.FLOAT, false, 0, 0)
    }
    
    // 设置颜色数据
    if (colorBuffer && data.colors) {
      gl.value.bindBuffer(gl.value.ARRAY_BUFFER, colorBuffer)
      gl.value.bufferData(gl.value.ARRAY_BUFFER, data.colors, gl.value.DYNAMIC_DRAW)
      
      const colorLocation = gl.value.getAttribLocation(shaderProgram, 'a_color')
      gl.value.enableVertexAttribArray(colorLocation)
      gl.value.vertexAttribPointer(colorLocation, 4, gl.value.FLOAT, false, 0, 0)
    }
    
    // 设置uniform变量
    const resolutionLocation = gl.value.getUniformLocation(shaderProgram, 'u_resolution')
    gl.value.uniform2f(resolutionLocation, options.width, options.height)
    
    const transformLocation = gl.value.getUniformLocation(shaderProgram, 'u_transform')
    const transform = new Float32Array([
      1, 0, 0,
      0, 1, 0,
      0, 0, 1
    ])
    gl.value.uniformMatrix3fv(transformLocation, false, transform)
    
    // 设置线宽
    if (data.lineWidth) {
      gl.value.lineWidth(data.lineWidth)
    }
    
    // 绘制
    if (data.indices && indexBuffer) {
      gl.value.bindBuffer(gl.value.ELEMENT_ARRAY_BUFFER, indexBuffer)
      gl.value.bufferData(gl.value.ELEMENT_ARRAY_BUFFER, data.indices, gl.value.DYNAMIC_DRAW)
      gl.value.drawElements(gl.value.LINES, data.indices.length, gl.value.UNSIGNED_SHORT, 0)
    } else {
      gl.value.drawArrays(gl.value.LINE_STRIP, 0, data.vertices.length / 2)
    }
  }

  // 渲染点
  const renderPoints = (data: RenderData) => {
    if (!gl.value || !shaderProgram || !isInitialized.value) return
    
    gl.value.useProgram(shaderProgram)
    
    // 设置顶点数据
    if (vertexBuffer && data.vertices) {
      gl.value.bindBuffer(gl.value.ARRAY_BUFFER, vertexBuffer)
      gl.value.bufferData(gl.value.ARRAY_BUFFER, data.vertices, gl.value.DYNAMIC_DRAW)
      
      const positionLocation = gl.value.getAttribLocation(shaderProgram, 'a_position')
      gl.value.enableVertexAttribArray(positionLocation)
      gl.value.vertexAttribPointer(positionLocation, 2, gl.value.FLOAT, false, 0, 0)
    }
    
    // 设置颜色数据
    if (colorBuffer && data.colors) {
      gl.value.bindBuffer(gl.value.ARRAY_BUFFER, colorBuffer)
      gl.value.bufferData(gl.value.ARRAY_BUFFER, data.colors, gl.value.DYNAMIC_DRAW)
      
      const colorLocation = gl.value.getAttribLocation(shaderProgram, 'a_color')
      gl.value.enableVertexAttribArray(colorLocation)
      gl.value.vertexAttribPointer(colorLocation, 4, gl.value.FLOAT, false, 0, 0)
    }
    
    // 设置uniform变量
    const resolutionLocation = gl.value.getUniformLocation(shaderProgram, 'u_resolution')
    gl.value.uniform2f(resolutionLocation, options.width, options.height)
    
    const transformLocation = gl.value.getUniformLocation(shaderProgram, 'u_transform')
    const transform = new Float32Array([
      1, 0, 0,
      0, 1, 0,
      0, 0, 1
    ])
    gl.value.uniformMatrix3fv(transformLocation, false, transform)
    
    // 绘制点
    gl.value.drawArrays(gl.value.POINTS, 0, data.vertices.length / 2)
  }

  // 开始动画循环
  const startAnimation = (renderCallback: () => void) => {
    const animate = () => {
      renderCallback()
      animationId.value = requestAnimationFrame(animate)
    }
    animate()
  }

  // 停止动画循环
  const stopAnimation = () => {
    if (animationId.value) {
      cancelAnimationFrame(animationId.value)
      animationId.value = undefined
    }
  }

  // 调整画布大小
  const resize = (width: number, height: number) => {
    if (!canvasRef.value || !gl.value) return
    
    options.width = width
    options.height = height
    
    canvasRef.value.width = width
    canvasRef.value.height = height
    
    gl.value.viewport(0, 0, width, height)
  }

  // 获取性能信息
  const getPerformanceInfo = () => {
    if (!gl.value) return null
    
    const debugInfo = gl.value.getExtension('WEBGL_debug_renderer_info')
    return {
      vendor: debugInfo ? gl.value.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL) : 'Unknown',
      renderer: debugInfo ? gl.value.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL) : 'Unknown',
      version: gl.value.getParameter(gl.value.VERSION),
      shadingLanguageVersion: gl.value.getParameter(gl.value.SHADING_LANGUAGE_VERSION),
      maxTextureSize: gl.value.getParameter(gl.value.MAX_TEXTURE_SIZE),
      maxVertexAttribs: gl.value.getParameter(gl.value.MAX_VERTEX_ATTRIBS),
      maxVaryingVectors: gl.value.getParameter(gl.value.MAX_VARYING_VECTORS),
      maxFragmentUniforms: gl.value.getParameter(gl.value.MAX_FRAGMENT_UNIFORM_VECTORS),
      maxVertexUniforms: gl.value.getParameter(gl.value.MAX_VERTEX_UNIFORM_VECTORS)
    }
  }

  // 清理资源
  const cleanup = () => {
    stopAnimation()
    
    if (gl.value) {
      if (shaderProgram) {
        gl.value.deleteProgram(shaderProgram)
        shaderProgram = null
      }
      
      if (vertexBuffer) {
        gl.value.deleteBuffer(vertexBuffer)
        vertexBuffer = null
      }
      
      if (colorBuffer) {
        gl.value.deleteBuffer(colorBuffer)
        colorBuffer = null
      }
      
      if (indexBuffer) {
        gl.value.deleteBuffer(indexBuffer)
        indexBuffer = null
      }
    }
    
    gl.value = null
    isInitialized.value = false
  }

  // 生命周期管理
  onMounted(() => {
    nextTick(() => {
      initWebGL()
    })
  })

  onUnmounted(() => {
    cleanup()
  })

  return {
    canvasRef,
    gl,
    isInitialized,
    initWebGL,
    clear,
    renderLines,
    renderPoints,
    startAnimation,
    stopAnimation,
    resize,
    getPerformanceInfo,
    cleanup
  }
}

// 数据转换工具
export function convertToWebGLData(
  points: Array<[number, number]>,
  color: [number, number, number, number] = [1, 1, 1, 1]
): RenderData {
  const vertices = new Float32Array(points.length * 2)
  const colors = new Float32Array(points.length * 4)
  
  for (let i = 0; i < points.length; i++) {
    vertices[i * 2] = points[i][0]
    vertices[i * 2 + 1] = points[i][1]
    
    colors[i * 4] = color[0]
    colors[i * 4 + 1] = color[1]
    colors[i * 4 + 2] = color[2]
    colors[i * 4 + 3] = color[3]
  }
  
  return {
    vertices,
    colors
  }
}

// 性能监控
export function useWebGLPerformanceMonitor() {
  const frameCount = ref(0)
  const fps = ref(0)
  const lastTime = ref(0)
  const frameTime = ref(0)
  
  const updatePerformance = () => {
    const now = performance.now()
    frameCount.value++
    
    if (lastTime.value === 0) {
      lastTime.value = now
      return
    }
    
    const deltaTime = now - lastTime.value
    frameTime.value = deltaTime
    
    if (deltaTime >= 1000) {
      fps.value = Math.round((frameCount.value * 1000) / deltaTime)
      frameCount.value = 0
      lastTime.value = now
    }
  }
  
  const reset = () => {
    frameCount.value = 0
    fps.value = 0
    lastTime.value = 0
    frameTime.value = 0
  }
  
  return {
    fps,
    frameTime,
    updatePerformance,
    reset
  }
}
