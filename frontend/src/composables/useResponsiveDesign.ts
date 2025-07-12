import { ref, computed, onMounted, onUnmounted } from 'vue'

export interface BreakpointConfig {
  xs: number  // 超小屏幕
  sm: number  // 小屏幕
  md: number  // 中等屏幕
  lg: number  // 大屏幕
  xl: number  // 超大屏幕
  xxl: number // 超超大屏幕
}

export interface DeviceInfo {
  isMobile: boolean
  isTablet: boolean
  isDesktop: boolean
  isTouch: boolean
  orientation: 'portrait' | 'landscape'
  pixelRatio: number
  platform: string
}

export interface ResponsiveConfig {
  columns: {
    xs: number
    sm: number
    md: number
    lg: number
    xl: number
    xxl: number
  }
  spacing: {
    xs: number
    sm: number
    md: number
    lg: number
    xl: number
    xxl: number
  }
  fontSize: {
    xs: number
    sm: number
    md: number
    lg: number
    xl: number
    xxl: number
  }
}

const DEFAULT_BREAKPOINTS: BreakpointConfig = {
  xs: 480,
  sm: 768,
  md: 1024,
  lg: 1280,
  xl: 1600,
  xxl: 1920
}

const DEFAULT_RESPONSIVE_CONFIG: ResponsiveConfig = {
  columns: {
    xs: 1,
    sm: 2,
    md: 3,
    lg: 4,
    xl: 5,
    xxl: 6
  },
  spacing: {
    xs: 8,
    sm: 12,
    md: 16,
    lg: 20,
    xl: 24,
    xxl: 32
  },
  fontSize: {
    xs: 12,
    sm: 14,
    md: 16,
    lg: 18,
    xl: 20,
    xxl: 24
  }
}

export function useResponsiveDesign(
  breakpoints: BreakpointConfig = DEFAULT_BREAKPOINTS,
  config: ResponsiveConfig = DEFAULT_RESPONSIVE_CONFIG
) {
  const windowWidth = ref(0)
  const windowHeight = ref(0)
  const deviceInfo = ref<DeviceInfo>({
    isMobile: false,
    isTablet: false,
    isDesktop: true,
    isTouch: false,
    orientation: 'landscape',
    pixelRatio: 1,
    platform: 'unknown'
  })

  // 当前断点
  const currentBreakpoint = computed(() => {
    const width = windowWidth.value
    if (width < breakpoints.xs) return 'xs'
    if (width < breakpoints.sm) return 'sm'
    if (width < breakpoints.md) return 'md'
    if (width < breakpoints.lg) return 'lg'
    if (width < breakpoints.xl) return 'xl'
    return 'xxl'
  })

  // 响应式值获取
  const getResponsiveValue = <T>(values: Record<string, T>): T => {
    const breakpoint = currentBreakpoint.value
    return values[breakpoint] || values.md || Object.values(values)[0]
  }

  // 当前列数
  const currentColumns = computed(() => getResponsiveValue(config.columns))

  // 当前间距
  const currentSpacing = computed(() => getResponsiveValue(config.spacing))

  // 当前字体大小
  const currentFontSize = computed(() => getResponsiveValue(config.fontSize))

  // 断点检查
  const isXs = computed(() => currentBreakpoint.value === 'xs')
  const isSm = computed(() => currentBreakpoint.value === 'sm')
  const isMd = computed(() => currentBreakpoint.value === 'md')
  const isLg = computed(() => currentBreakpoint.value === 'lg')
  const isXl = computed(() => currentBreakpoint.value === 'xl')
  const isXxl = computed(() => currentBreakpoint.value === 'xxl')

  // 设备类型检查
  const isMobile = computed(() => deviceInfo.value.isMobile)
  const isTablet = computed(() => deviceInfo.value.isTablet)
  const isDesktop = computed(() => deviceInfo.value.isDesktop)
  const isTouch = computed(() => deviceInfo.value.isTouch)

  // 方向检查
  const isPortrait = computed(() => deviceInfo.value.orientation === 'portrait')
  const isLandscape = computed(() => deviceInfo.value.orientation === 'landscape')

  // 更新窗口尺寸
  const updateWindowSize = () => {
    windowWidth.value = window.innerWidth
    windowHeight.value = window.innerHeight
  }

  // 检测设备信息
  const detectDeviceInfo = () => {
    const userAgent = navigator.userAgent.toLowerCase()
    const width = window.innerWidth
    const height = window.innerHeight
    
    // 检测移动设备
    const isMobileDevice = /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini/i.test(userAgent)
    
    // 检测平板
    const isTabletDevice = /ipad|android(?!.*mobile)/i.test(userAgent) || 
                          (isMobileDevice && Math.min(width, height) >= 768)
    
    // 检测触摸支持
    const hasTouchSupport = 'ontouchstart' in window || 
                           navigator.maxTouchPoints > 0 || 
                           (navigator as any).msMaxTouchPoints > 0

    // 检测方向
    const orientation = height > width ? 'portrait' : 'landscape'

    // 检测平台
    let platform = 'unknown'
    if (/windows/i.test(userAgent)) platform = 'windows'
    else if (/macintosh|mac os x/i.test(userAgent)) platform = 'macos'
    else if (/linux/i.test(userAgent)) platform = 'linux'
    else if (/android/i.test(userAgent)) platform = 'android'
    else if (/iphone|ipad|ipod/i.test(userAgent)) platform = 'ios'

    deviceInfo.value = {
      isMobile: isMobileDevice && !isTabletDevice,
      isTablet: isTabletDevice,
      isDesktop: !isMobileDevice && !isTabletDevice,
      isTouch: hasTouchSupport,
      orientation,
      pixelRatio: window.devicePixelRatio || 1,
      platform
    }
  }

  // 处理窗口大小变化
  const handleResize = () => {
    updateWindowSize()
    detectDeviceInfo()
  }

  // 处理方向变化
  const handleOrientationChange = () => {
    // 延迟执行，等待方向变化完成
    setTimeout(() => {
      updateWindowSize()
      detectDeviceInfo()
    }, 100)
  }

  // 生命周期管理
  onMounted(() => {
    updateWindowSize()
    detectDeviceInfo()
    
    window.addEventListener('resize', handleResize)
    window.addEventListener('orientationchange', handleOrientationChange)
  })

  onUnmounted(() => {
    window.removeEventListener('resize', handleResize)
    window.removeEventListener('orientationchange', handleOrientationChange)
  })

  return {
    // 窗口尺寸
    windowWidth,
    windowHeight,
    
    // 设备信息
    deviceInfo,
    
    // 断点信息
    currentBreakpoint,
    isXs,
    isSm,
    isMd,
    isLg,
    isXl,
    isXxl,
    
    // 设备类型
    isMobile,
    isTablet,
    isDesktop,
    isTouch,
    
    // 方向
    isPortrait,
    isLandscape,
    
    // 响应式值
    currentColumns,
    currentSpacing,
    currentFontSize,
    getResponsiveValue,
    
    // 方法
    updateWindowSize,
    detectDeviceInfo
  }
}

// 触摸手势支持
export function useTouchGestures() {
  const touchStart = ref<{ x: number; y: number; time: number } | null>(null)
  const touchEnd = ref<{ x: number; y: number; time: number } | null>(null)
  const isSwipe = ref(false)
  const swipeDirection = ref<'left' | 'right' | 'up' | 'down' | null>(null)
  const swipeDistance = ref(0)
  const swipeVelocity = ref(0)

  // 触摸开始
  const handleTouchStart = (event: TouchEvent) => {
    const touch = event.touches[0]
    touchStart.value = {
      x: touch.clientX,
      y: touch.clientY,
      time: Date.now()
    }
    touchEnd.value = null
    isSwipe.value = false
    swipeDirection.value = null
  }

  // 触摸结束
  const handleTouchEnd = (event: TouchEvent) => {
    if (!touchStart.value) return

    const touch = event.changedTouches[0]
    touchEnd.value = {
      x: touch.clientX,
      y: touch.clientY,
      time: Date.now()
    }

    analyzeSwipe()
  }

  // 分析滑动手势
  const analyzeSwipe = () => {
    if (!touchStart.value || !touchEnd.value) return

    const deltaX = touchEnd.value.x - touchStart.value.x
    const deltaY = touchEnd.value.y - touchStart.value.y
    const deltaTime = touchEnd.value.time - touchStart.value.time
    
    const distance = Math.sqrt(deltaX * deltaX + deltaY * deltaY)
    const velocity = distance / deltaTime

    // 最小滑动距离和速度阈值
    const minDistance = 50
    const minVelocity = 0.1

    if (distance >= minDistance && velocity >= minVelocity) {
      isSwipe.value = true
      swipeDistance.value = distance
      swipeVelocity.value = velocity

      // 确定滑动方向
      if (Math.abs(deltaX) > Math.abs(deltaY)) {
        swipeDirection.value = deltaX > 0 ? 'right' : 'left'
      } else {
        swipeDirection.value = deltaY > 0 ? 'down' : 'up'
      }
    }
  }

  // 重置手势状态
  const resetGesture = () => {
    touchStart.value = null
    touchEnd.value = null
    isSwipe.value = false
    swipeDirection.value = null
    swipeDistance.value = 0
    swipeVelocity.value = 0
  }

  return {
    touchStart,
    touchEnd,
    isSwipe,
    swipeDirection,
    swipeDistance,
    swipeVelocity,
    handleTouchStart,
    handleTouchEnd,
    resetGesture
  }
}

// 移动端优化的虚拟键盘处理
export function useVirtualKeyboard() {
  const isKeyboardVisible = ref(false)
  const keyboardHeight = ref(0)
  const originalViewportHeight = ref(0)

  const detectKeyboard = () => {
    const currentHeight = window.visualViewport?.height || window.innerHeight
    
    if (originalViewportHeight.value === 0) {
      originalViewportHeight.value = currentHeight
    }

    const heightDifference = originalViewportHeight.value - currentHeight
    const threshold = 150 // 键盘高度阈值

    if (heightDifference > threshold) {
      isKeyboardVisible.value = true
      keyboardHeight.value = heightDifference
    } else {
      isKeyboardVisible.value = false
      keyboardHeight.value = 0
    }
  }

  onMounted(() => {
    if (window.visualViewport) {
      window.visualViewport.addEventListener('resize', detectKeyboard)
    } else {
      window.addEventListener('resize', detectKeyboard)
    }
    
    // 初始化
    detectKeyboard()
  })

  onUnmounted(() => {
    if (window.visualViewport) {
      window.visualViewport.removeEventListener('resize', detectKeyboard)
    } else {
      window.removeEventListener('resize', detectKeyboard)
    }
  })

  return {
    isKeyboardVisible,
    keyboardHeight,
    originalViewportHeight
  }
}

// 移动端性能优化
export function useMobileOptimization() {
  const isLowEndDevice = ref(false)
  const connectionType = ref<string>('unknown')
  const isSlowConnection = ref(false)

  // 检测设备性能
  const detectDevicePerformance = () => {
    // 检测硬件并发数
    const hardwareConcurrency = navigator.hardwareConcurrency || 1
    
    // 检测内存信息（如果可用）
    const memory = (navigator as any).deviceMemory || 4
    
    // 检测连接信息
    const connection = (navigator as any).connection || (navigator as any).mozConnection || (navigator as any).webkitConnection
    
    if (connection) {
      connectionType.value = connection.effectiveType || 'unknown'
      isSlowConnection.value = ['slow-2g', '2g', '3g'].includes(connection.effectiveType)
    }

    // 判断是否为低端设备
    isLowEndDevice.value = hardwareConcurrency <= 2 || memory <= 2
  }

  // 获取优化建议
  const getOptimizationSettings = () => {
    return {
      // 根据设备性能调整渲染质量
      renderQuality: isLowEndDevice.value ? 'low' : 'high',
      
      // 根据连接速度调整数据加载策略
      dataLoadingStrategy: isSlowConnection.value ? 'lazy' : 'eager',
      
      // 动画设置
      enableAnimations: !isLowEndDevice.value,
      
      // 图片质量
      imageQuality: isSlowConnection.value ? 'low' : 'high',
      
      // 更新频率
      updateFrequency: isLowEndDevice.value ? 30 : 60, // FPS
      
      // 缓存策略
      cacheStrategy: isSlowConnection.value ? 'aggressive' : 'normal'
    }
  }

  onMounted(() => {
    detectDevicePerformance()
  })

  return {
    isLowEndDevice,
    connectionType,
    isSlowConnection,
    getOptimizationSettings
  }
}
