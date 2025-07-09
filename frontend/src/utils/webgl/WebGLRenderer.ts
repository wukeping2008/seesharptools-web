/**
 * WebGL高性能渲染引擎
 * 专为StripChart高速数据流显示优化
 * 支持1GS/s数据率，16-32通道同步显示
 */

export interface WebGLRendererOptions {
  canvas: HTMLCanvasElement
  maxPoints: number
  maxChannels: number
  enableAntiAliasing: boolean
  enableLOD: boolean // Level of Detail
}

export interface Viewport {
  x: number
  y: number
  width: number
  height: number
  timeRange: [number, number]
  valueRange: [number, number]
}

export interface ChannelData {
  id: number
  name: string
  color: [number, number, number, number] // RGBA
  data: Float32Array
  visible: boolean
  lineWidth: number
}

export class WebGLRenderer {
  private gl: WebGLRenderingContext
  private shaderProgram: WebGLProgram | null = null
  private vertexBuffer: WebGLBuffer | null = null
  private colorBuffer: WebGLBuffer | null = null
  private indexBuffer: WebGLBuffer | null = null
  
  private options: WebGLRendererOptions
  private viewport: Viewport
  private channels: ChannelData[] = []
  
  // 着色器属性和uniform位置
  private attribLocations: {
    position: number
    color: number
  } = { position: -1, color: -1 }
  
  private uniformLocations: {
    transform: WebGLUniformLocation | null
    viewport: WebGLUniformLocation | null
    lineWidth: WebGLUniformLocation | null
  } = { transform: null, viewport: null, lineWidth: null }
  
  // 性能监控
  private frameCount = 0
  private lastFpsTime = 0
  private currentFps = 0
  
  constructor(options: WebGLRendererOptions) {
    this.options = options
    
    const gl = options.canvas.getContext('webgl', {
      antialias: options.enableAntiAliasing,
      alpha: false,
      depth: false,
      stencil: false,
      preserveDrawingBuffer: false,
      powerPreference: 'high-performance'
    })
    
    if (!gl) {
      throw new Error('WebGL not supported')
    }
    
    this.gl = gl
    this.viewport = {
      x: 0,
      y: 0,
      width: options.canvas.width,
      height: options.canvas.height,
      timeRange: [0, 1],
      valueRange: [-1, 1]
    }
    
    this.initializeWebGL()
  }
  
  private initializeWebGL(): void {
    const gl = this.gl
    
    // 设置视口
    gl.viewport(0, 0, this.options.canvas.width, this.options.canvas.height)
    
    // 启用混合以支持抗锯齿
    gl.enable(gl.BLEND)
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA)
    
    // 设置清除颜色
    gl.clearColor(1.0, 1.0, 1.0, 1.0)
    
    // 初始化着色器
    this.initShaders()
    
    // 创建缓冲区
    this.createBuffers()
  }
  
  private initShaders(): void {
    const gl = this.gl
    
    // 顶点着色器源码
    const vertexShaderSource = `
      attribute vec2 a_position;
      attribute vec4 a_color;
      
      uniform mat3 u_transform;
      uniform vec4 u_viewport;
      uniform float u_lineWidth;
      
      varying vec4 v_color;
      varying float v_lineWidth;
      
      void main() {
        // 应用变换矩阵
        vec3 position = u_transform * vec3(a_position, 1.0);
        
        // 转换到NDC坐标系
        vec2 ndc = vec2(
          (position.x - u_viewport.x) / u_viewport.z * 2.0 - 1.0,
          (position.y - u_viewport.y) / u_viewport.w * 2.0 - 1.0
        );
        
        gl_Position = vec4(ndc, 0.0, 1.0);
        v_color = a_color;
        v_lineWidth = u_lineWidth;
      }
    `
    
    // 片段着色器源码
    const fragmentShaderSource = `
      precision mediump float;
      
      varying vec4 v_color;
      varying float v_lineWidth;
      
      void main() {
        // 抗锯齿处理
        float alpha = v_color.a;
        
        // 根据线宽调整透明度
        if (v_lineWidth > 1.0) {
          alpha *= smoothstep(0.0, 1.0, v_lineWidth);
        }
        
        gl_FragColor = vec4(v_color.rgb, alpha);
      }
    `
    
    // 编译着色器
    const vertexShader = this.compileShader(gl.VERTEX_SHADER, vertexShaderSource)
    const fragmentShader = this.compileShader(gl.FRAGMENT_SHADER, fragmentShaderSource)
    
    // 创建着色器程序
    this.shaderProgram = gl.createProgram()
    if (!this.shaderProgram) {
      throw new Error('Failed to create shader program')
    }
    
    gl.attachShader(this.shaderProgram, vertexShader)
    gl.attachShader(this.shaderProgram, fragmentShader)
    gl.linkProgram(this.shaderProgram)
    
    if (!gl.getProgramParameter(this.shaderProgram, gl.LINK_STATUS)) {
      const error = gl.getProgramInfoLog(this.shaderProgram)
      throw new Error(`Shader program linking failed: ${error}`)
    }
    
    // 获取属性和uniform位置
    this.attribLocations.position = gl.getAttribLocation(this.shaderProgram, 'a_position')
    this.attribLocations.color = gl.getAttribLocation(this.shaderProgram, 'a_color')
    
    this.uniformLocations.transform = gl.getUniformLocation(this.shaderProgram, 'u_transform')
    this.uniformLocations.viewport = gl.getUniformLocation(this.shaderProgram, 'u_viewport')
    this.uniformLocations.lineWidth = gl.getUniformLocation(this.shaderProgram, 'u_lineWidth')
    
    // 清理
    gl.deleteShader(vertexShader)
    gl.deleteShader(fragmentShader)
  }
  
  private compileShader(type: number, source: string): WebGLShader {
    const gl = this.gl
    const shader = gl.createShader(type)
    
    if (!shader) {
      throw new Error('Failed to create shader')
    }
    
    gl.shaderSource(shader, source)
    gl.compileShader(shader)
    
    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
      const error = gl.getShaderInfoLog(shader)
      gl.deleteShader(shader)
      throw new Error(`Shader compilation failed: ${error}`)
    }
    
    return shader
  }
  
  private createBuffers(): void {
    const gl = this.gl
    
    // 创建顶点缓冲区
    this.vertexBuffer = gl.createBuffer()
    if (!this.vertexBuffer) {
      throw new Error('Failed to create vertex buffer')
    }
    
    // 创建颜色缓冲区
    this.colorBuffer = gl.createBuffer()
    if (!this.colorBuffer) {
      throw new Error('Failed to create color buffer')
    }
    
    // 创建索引缓冲区
    this.indexBuffer = gl.createBuffer()
    if (!this.indexBuffer) {
      throw new Error('Failed to create index buffer')
    }
  }
  
  public setViewport(viewport: Viewport): void {
    this.viewport = viewport
    
    // 更新WebGL视口
    this.gl.viewport(
      viewport.x,
      viewport.y,
      viewport.width,
      viewport.height
    )
  }
  
  public setChannels(channels: ChannelData[]): void {
    this.channels = channels.filter(ch => ch.visible)
  }
  
  public render(): void {
    const gl = this.gl
    
    if (!this.shaderProgram) return
    
    // 清除画布
    gl.clear(gl.COLOR_BUFFER_BIT)
    
    // 使用着色器程序
    gl.useProgram(this.shaderProgram)
    
    // 设置uniform变量
    this.setUniforms()
    
    // 渲染每个通道
    this.channels.forEach(channel => {
      this.renderChannel(channel)
    })
    
    // 更新FPS计数
    this.updateFPS()
  }
  
  private setUniforms(): void {
    const gl = this.gl
    
    // 计算变换矩阵
    const transform = this.calculateTransformMatrix()
    gl.uniformMatrix3fv(this.uniformLocations.transform, false, transform)
    
    // 设置视口
    const viewportArray = new Float32Array([
      this.viewport.x,
      this.viewport.y,
      this.viewport.width,
      this.viewport.height
    ])
    gl.uniform4fv(this.uniformLocations.viewport, viewportArray)
  }
  
  private calculateTransformMatrix(): Float32Array {
    const { timeRange, valueRange } = this.viewport
    
    // 创建从数据坐标到屏幕坐标的变换矩阵
    const scaleX = this.viewport.width / (timeRange[1] - timeRange[0])
    const scaleY = this.viewport.height / (valueRange[1] - valueRange[0])
    const translateX = -timeRange[0] * scaleX
    const translateY = -valueRange[0] * scaleY
    
    return new Float32Array([
      scaleX, 0, translateX,
      0, scaleY, translateY,
      0, 0, 1
    ])
  }
  
  private renderChannel(channel: ChannelData): void {
    const gl = this.gl
    
    if (channel.data.length === 0) return
    
    // 应用LOD（细节层次）优化
    const processedData = this.options.enableLOD 
      ? this.applyLOD(channel.data)
      : channel.data
    
    // 准备顶点数据
    const vertices = this.prepareVertices(processedData)
    const colors = this.prepareColors(channel.color, vertices.length / 2)
    const indices = this.prepareIndices(vertices.length / 2)
    
    // 更新顶点缓冲区
    gl.bindBuffer(gl.ARRAY_BUFFER, this.vertexBuffer)
    gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.DYNAMIC_DRAW)
    gl.enableVertexAttribArray(this.attribLocations.position)
    gl.vertexAttribPointer(this.attribLocations.position, 2, gl.FLOAT, false, 0, 0)
    
    // 更新颜色缓冲区
    gl.bindBuffer(gl.ARRAY_BUFFER, this.colorBuffer)
    gl.bufferData(gl.ARRAY_BUFFER, colors, gl.DYNAMIC_DRAW)
    gl.enableVertexAttribArray(this.attribLocations.color)
    gl.vertexAttribPointer(this.attribLocations.color, 4, gl.FLOAT, false, 0, 0)
    
    // 更新索引缓冲区
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.indexBuffer)
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.DYNAMIC_DRAW)
    
    // 设置线宽
    gl.uniform1f(this.uniformLocations.lineWidth, channel.lineWidth)
    
    // 绘制线条
    gl.drawElements(gl.LINE_STRIP, indices.length, gl.UNSIGNED_SHORT, 0)
  }
  
  private prepareVertices(data: Float32Array): Float32Array {
    const vertices = new Float32Array(data.length * 2)
    const dt = (this.viewport.timeRange[1] - this.viewport.timeRange[0]) / data.length
    
    for (let i = 0; i < data.length; i++) {
      vertices[i * 2] = this.viewport.timeRange[0] + i * dt // x (time)
      vertices[i * 2 + 1] = data[i] // y (value)
    }
    
    return vertices
  }
  
  private prepareColors(color: [number, number, number, number], pointCount: number): Float32Array {
    const colors = new Float32Array(pointCount * 4)
    
    for (let i = 0; i < pointCount; i++) {
      colors[i * 4] = color[0]     // R
      colors[i * 4 + 1] = color[1] // G
      colors[i * 4 + 2] = color[2] // B
      colors[i * 4 + 3] = color[3] // A
    }
    
    return colors
  }
  
  private prepareIndices(pointCount: number): Uint16Array {
    const indices = new Uint16Array(pointCount)
    
    for (let i = 0; i < pointCount; i++) {
      indices[i] = i
    }
    
    return indices
  }
  
  private applyLOD(data: Float32Array): Float32Array {
    // 根据视口大小和数据量决定采样率
    const viewportPixels = this.viewport.width
    const dataPoints = data.length
    
    if (dataPoints <= viewportPixels * 2) {
      return data // 不需要降采样
    }
    
    // 使用LTTB算法进行智能采样
    return this.lttbDownsample(data, viewportPixels)
  }
  
  private lttbDownsample(data: Float32Array, targetPoints: number): Float32Array {
    if (data.length <= targetPoints) {
      return data
    }
    
    const sampled = new Float32Array(targetPoints)
    const bucketSize = (data.length - 2) / (targetPoints - 2)
    
    // 保留第一个点
    sampled[0] = data[0]
    
    for (let i = 0; i < targetPoints - 2; i++) {
      // 计算当前桶的范围
      const bucketStart = Math.floor(i * bucketSize) + 1
      const bucketEnd = Math.floor((i + 1) * bucketSize) + 1
      
      // 计算下一个桶的平均点
      const nextBucketStart = Math.floor((i + 1) * bucketSize) + 1
      const nextBucketEnd = Math.floor((i + 2) * bucketSize) + 1
      
      let avgY = 0
      let avgCount = 0
      for (let j = nextBucketStart; j < nextBucketEnd && j < data.length; j++) {
        avgY += data[j]
        avgCount++
      }
      
      if (avgCount > 0) {
        avgY /= avgCount
      }
      
      // 在当前桶中找到形成最大三角形面积的点
      let maxArea = -1
      let maxAreaIndex = bucketStart
      
      const prevValue = sampled[i]
      
      for (let j = bucketStart; j < bucketEnd && j < data.length; j++) {
        const area = Math.abs(
          (i - (i + 2)) * (data[j] - prevValue) -
          (i - j) * (avgY - prevValue)
        ) * 0.5
        
        if (area > maxArea) {
          maxArea = area
          maxAreaIndex = j
        }
      }
      
      sampled[i + 1] = data[maxAreaIndex]
    }
    
    // 保留最后一个点
    sampled[targetPoints - 1] = data[data.length - 1]
    
    return sampled
  }
  
  private updateFPS(): void {
    this.frameCount++
    const now = performance.now()
    
    if (now - this.lastFpsTime >= 1000) {
      this.currentFps = Math.round((this.frameCount * 1000) / (now - this.lastFpsTime))
      this.frameCount = 0
      this.lastFpsTime = now
    }
  }
  
  public getFPS(): number {
    return this.currentFps
  }
  
  public resize(width: number, height: number): void {
    const canvas = this.options.canvas
    canvas.width = width
    canvas.height = height
    
    this.viewport.width = width
    this.viewport.height = height
    
    this.gl.viewport(0, 0, width, height)
  }
  
  public dispose(): void {
    const gl = this.gl
    
    if (this.shaderProgram) {
      gl.deleteProgram(this.shaderProgram)
    }
    
    if (this.vertexBuffer) {
      gl.deleteBuffer(this.vertexBuffer)
    }
    
    if (this.colorBuffer) {
      gl.deleteBuffer(this.colorBuffer)
    }
    
    if (this.indexBuffer) {
      gl.deleteBuffer(this.indexBuffer)
    }
  }
}
