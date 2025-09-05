/**
 * 高性能WebGL图表渲染器
 * 用于大数据量波形图的实时渲染
 */
export class WebGLChartRenderer {
  private gl: WebGLRenderingContext | null = null
  private canvas: HTMLCanvasElement
  private program: WebGLProgram | null = null
  private buffers: Map<string, WebGLBuffer> = new Map()
  private uniforms: Map<string, WebGLUniformLocation> = new Map()
  private attributes: Map<string, number> = new Map()
  
  // 渲染配置
  private config = {
    lineWidth: 2.0,
    antialias: true,
    pixelRatio: window.devicePixelRatio || 1,
    backgroundColor: [0.98, 0.98, 0.98, 1.0],
    gridColor: [0.9, 0.9, 0.9, 1.0],
    lineColors: [
      [0.2, 0.4, 0.8, 1.0], // 蓝色
      [0.8, 0.2, 0.2, 1.0], // 红色
      [0.2, 0.8, 0.2, 1.0], // 绿色
      [0.8, 0.8, 0.2, 1.0]  // 黄色
    ]
  }

  // 数据缓存
  private dataCache: Map<string, Float32Array> = new Map()
  private maxDataPoints = 100000
  private currentDataOffset = 0

  constructor(canvas: HTMLCanvasElement) {
    this.canvas = canvas
    this.initWebGL()
    this.initShaders()
    this.initBuffers()
    this.setupRenderLoop()
  }

  /**
   * 初始化WebGL上下文
   */
  private initWebGL(): void {
    const gl = this.canvas.getContext('webgl2', {
      antialias: this.config.antialias,
      alpha: true,
      preserveDrawingBuffer: true,
      powerPreference: 'high-performance'
    }) || this.canvas.getContext('webgl', {
      antialias: this.config.antialias,
      alpha: true,
      preserveDrawingBuffer: true
    })

    if (!gl) {
      throw new Error('WebGL不可用')
    }

    this.gl = gl
    this.resizeCanvas()
    
    // 启用混合
    gl.enable(gl.BLEND)
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA)
    
    // 设置视口
    gl.viewport(0, 0, this.canvas.width, this.canvas.height)
  }

  /**
   * 初始化着色器
   */
  private initShaders(): void {
    if (!this.gl) return

    // 顶点着色器
    const vertexShaderSource = `
      attribute vec2 a_position;
      attribute float a_value;
      
      uniform mat4 u_transform;
      uniform vec2 u_resolution;
      uniform vec2 u_dataRange;
      
      varying float v_value;
      
      void main() {
        vec2 normalized = vec2(
          (a_position.x / u_resolution.x) * 2.0 - 1.0,
          (a_value - u_dataRange.x) / (u_dataRange.y - u_dataRange.x) * 2.0 - 1.0
        );
        
        gl_Position = u_transform * vec4(normalized, 0.0, 1.0);
        v_value = a_value;
        gl_PointSize = 2.0;
      }
    `

    // 片段着色器
    const fragmentShaderSource = `
      precision mediump float;
      
      uniform vec4 u_color;
      uniform float u_opacity;
      
      varying float v_value;
      
      void main() {
        gl_FragColor = vec4(u_color.rgb, u_color.a * u_opacity);
      }
    `

    const vertexShader = this.createShader(this.gl.VERTEX_SHADER, vertexShaderSource)
    const fragmentShader = this.createShader(this.gl.FRAGMENT_SHADER, fragmentShaderSource)

    if (!vertexShader || !fragmentShader) {
      throw new Error('着色器编译失败')
    }

    // 创建程序
    const program = this.gl.createProgram()
    if (!program) {
      throw new Error('无法创建WebGL程序')
    }

    this.gl.attachShader(program, vertexShader)
    this.gl.attachShader(program, fragmentShader)
    this.gl.linkProgram(program)

    if (!this.gl.getProgramParameter(program, this.gl.LINK_STATUS)) {
      console.error('程序链接失败:', this.gl.getProgramInfoLog(program))
      throw new Error('WebGL程序链接失败')
    }

    this.program = program
    this.gl.useProgram(program)

    // 获取属性和uniform位置
    this.attributes.set('position', this.gl.getAttribLocation(program, 'a_position'))
    this.attributes.set('value', this.gl.getAttribLocation(program, 'a_value'))
    
    const transformLoc = this.gl.getUniformLocation(program, 'u_transform')
    const resolutionLoc = this.gl.getUniformLocation(program, 'u_resolution')
    const dataRangeLoc = this.gl.getUniformLocation(program, 'u_dataRange')
    const colorLoc = this.gl.getUniformLocation(program, 'u_color')
    const opacityLoc = this.gl.getUniformLocation(program, 'u_opacity')
    
    if (transformLoc) this.uniforms.set('transform', transformLoc)
    if (resolutionLoc) this.uniforms.set('resolution', resolutionLoc)
    if (dataRangeLoc) this.uniforms.set('dataRange', dataRangeLoc)
    if (colorLoc) this.uniforms.set('color', colorLoc)
    if (opacityLoc) this.uniforms.set('opacity', opacityLoc)
  }

  /**
   * 创建着色器
   */
  private createShader(type: number, source: string): WebGLShader | null {
    if (!this.gl) return null

    const shader = this.gl.createShader(type)
    if (!shader) return null

    this.gl.shaderSource(shader, source)
    this.gl.compileShader(shader)

    if (!this.gl.getShaderParameter(shader, this.gl.COMPILE_STATUS)) {
      console.error('着色器编译失败:', this.gl.getShaderInfoLog(shader))
      this.gl.deleteShader(shader)
      return null
    }

    return shader
  }

  /**
   * 初始化缓冲区
   */
  private initBuffers(): void {
    if (!this.gl) return

    // 创建位置缓冲区
    const positionBuffer = this.gl.createBuffer()
    if (positionBuffer) {
      this.buffers.set('position', positionBuffer)
    }

    // 创建数值缓冲区
    const valueBuffer = this.gl.createBuffer()
    if (valueBuffer) {
      this.buffers.set('value', valueBuffer)
    }

    // 创建网格缓冲区
    const gridBuffer = this.gl.createBuffer()
    if (gridBuffer) {
      this.buffers.set('grid', gridBuffer)
    }
  }

  /**
   * 更新数据
   */
  public updateData(seriesId: string, data: number[]): void {
    // 转换为Float32Array以提高性能
    const floatData = new Float32Array(data)
    this.dataCache.set(seriesId, floatData)
    
    // 如果数据超过最大点数，使用环形缓冲
    if (data.length > this.maxDataPoints) {
      this.currentDataOffset = data.length - this.maxDataPoints
    }
    
    this.render()
  }

  /**
   * 批量更新数据
   */
  public batchUpdateData(updates: Map<string, number[]>): void {
    updates.forEach((data, seriesId) => {
      const floatData = new Float32Array(data)
      this.dataCache.set(seriesId, floatData)
    })
    this.render()
  }

  /**
   * 渲染
   */
  private render(): void {
    if (!this.gl || !this.program) return

    const gl = this.gl

    // 清空画布
    gl.clearColor(...this.config.backgroundColor)
    gl.clear(gl.COLOR_BUFFER_BIT)

    // 设置uniform
    gl.uniform2f(this.uniforms.get('resolution')!, this.canvas.width, this.canvas.height)

    // 渲染每个数据系列
    let colorIndex = 0
    this.dataCache.forEach((data, seriesId) => {
      this.renderSeries(data, this.config.lineColors[colorIndex % this.config.lineColors.length])
      colorIndex++
    })

    // 渲染网格
    this.renderGrid()
  }

  /**
   * 渲染单个数据系列
   */
  private renderSeries(data: Float32Array, color: number[]): void {
    if (!this.gl || !this.program) return

    const gl = this.gl
    const numPoints = Math.min(data.length, this.maxDataPoints)
    
    // 创建位置数据
    const positions = new Float32Array(numPoints * 2)
    for (let i = 0; i < numPoints; i++) {
      positions[i * 2] = i
      positions[i * 2 + 1] = 0
    }

    // 更新位置缓冲区
    const positionBuffer = this.buffers.get('position')
    if (positionBuffer) {
      gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer)
      gl.bufferData(gl.ARRAY_BUFFER, positions, gl.DYNAMIC_DRAW)
      
      const posAttr = this.attributes.get('position')
      if (posAttr !== undefined) {
        gl.enableVertexAttribArray(posAttr)
        gl.vertexAttribPointer(posAttr, 2, gl.FLOAT, false, 0, 0)
      }
    }

    // 更新数值缓冲区
    const valueBuffer = this.buffers.get('value')
    if (valueBuffer) {
      gl.bindBuffer(gl.ARRAY_BUFFER, valueBuffer)
      gl.bufferData(gl.ARRAY_BUFFER, data, gl.DYNAMIC_DRAW)
      
      const valAttr = this.attributes.get('value')
      if (valAttr !== undefined) {
        gl.enableVertexAttribArray(valAttr)
        gl.vertexAttribPointer(valAttr, 1, gl.FLOAT, false, 0, 0)
      }
    }

    // 计算数据范围
    let min = Infinity, max = -Infinity
    for (let i = 0; i < data.length; i++) {
      min = Math.min(min, data[i])
      max = Math.max(max, data[i])
    }
    
    // 设置uniform
    gl.uniform2f(this.uniforms.get('dataRange')!, min, max)
    gl.uniform4fv(this.uniforms.get('color')!, color)
    gl.uniform1f(this.uniforms.get('opacity')!, 1.0)
    
    // 设置变换矩阵
    const transform = new Float32Array([
      1, 0, 0, 0,
      0, 1, 0, 0,
      0, 0, 1, 0,
      0, 0, 0, 1
    ])
    gl.uniformMatrix4fv(this.uniforms.get('transform')!, false, transform)

    // 绘制线条
    gl.lineWidth(this.config.lineWidth)
    gl.drawArrays(gl.LINE_STRIP, 0, numPoints)
  }

  /**
   * 渲染网格
   */
  private renderGrid(): void {
    if (!this.gl) return
    // 网格渲染逻辑（简化版）
    // 实际应用中应该绘制更详细的网格线
  }

  /**
   * 调整画布大小
   */
  private resizeCanvas(): void {
    const displayWidth = this.canvas.clientWidth * this.config.pixelRatio
    const displayHeight = this.canvas.clientHeight * this.config.pixelRatio

    if (this.canvas.width !== displayWidth || this.canvas.height !== displayHeight) {
      this.canvas.width = displayWidth
      this.canvas.height = displayHeight
      
      if (this.gl) {
        this.gl.viewport(0, 0, displayWidth, displayHeight)
      }
    }
  }

  /**
   * 设置渲染循环
   */
  private setupRenderLoop(): void {
    let lastTime = 0
    const targetFPS = 60
    const targetFrameTime = 1000 / targetFPS

    const animate = (currentTime: number) => {
      const deltaTime = currentTime - lastTime
      
      if (deltaTime >= targetFrameTime) {
        this.render()
        lastTime = currentTime
      }
      
      requestAnimationFrame(animate)
    }

    requestAnimationFrame(animate)
  }

  /**
   * 清理资源
   */
  public dispose(): void {
    if (!this.gl) return

    // 删除缓冲区
    this.buffers.forEach(buffer => {
      this.gl!.deleteBuffer(buffer)
    })
    this.buffers.clear()

    // 删除程序
    if (this.program) {
      this.gl.deleteProgram(this.program)
    }

    // 清空数据缓存
    this.dataCache.clear()
    
    // 清空WebGL上下文
    const loseContext = this.gl.getExtension('WEBGL_lose_context')
    if (loseContext) {
      loseContext.loseContext()
    }
  }

  /**
   * 获取性能统计
   */
  public getStats(): {
    fps: number
    dataPoints: number
    memoryUsage: number
  } {
    const totalPoints = Array.from(this.dataCache.values()).reduce((sum, data) => sum + data.length, 0)
    const memoryUsage = totalPoints * 4 // Float32 = 4 bytes
    
    return {
      fps: 60, // 简化版，实际应该计算真实FPS
      dataPoints: totalPoints,
      memoryUsage: memoryUsage / (1024 * 1024) // MB
    }
  }

  /**
   * 设置渲染配置
   */
  public setConfig(config: Partial<typeof this.config>): void {
    Object.assign(this.config, config)
    this.render()
  }
}