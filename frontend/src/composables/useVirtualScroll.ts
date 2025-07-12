import { ref, computed, onMounted, onUnmounted, nextTick, type Ref } from 'vue'

export interface VirtualScrollOptions {
  itemHeight: number
  containerHeight: number
  buffer?: number
  threshold?: number
}

export interface VirtualScrollItem {
  id: string | number
  height?: number
  [key: string]: any
}

export function useVirtualScroll<T extends VirtualScrollItem>(
  items: Ref<T[]>,
  options: VirtualScrollOptions
) {
  const containerRef = ref<HTMLElement>()
  const scrollTop = ref(0)
  const buffer = options.buffer || 5
  const threshold = options.threshold || 100

  // 计算可见区域
  const visibleRange = computed(() => {
    const { itemHeight, containerHeight } = options
    const start = Math.floor(scrollTop.value / itemHeight)
    const visibleCount = Math.ceil(containerHeight / itemHeight)
    const end = start + visibleCount

    return {
      start: Math.max(0, start - buffer),
      end: Math.min(items.value.length, end + buffer),
      visibleStart: start,
      visibleEnd: end
    }
  })

  // 可见项目
  const visibleItems = computed(() => {
    const { start, end } = visibleRange.value
    return items.value.slice(start, end).map((item: T, index: number) => ({
      ...item,
      index: start + index,
      top: (start + index) * options.itemHeight
    }))
  })

  // 总高度
  const totalHeight = computed(() => {
    return items.value.length * options.itemHeight
  })

  // 偏移量
  const offsetY = computed(() => {
    return visibleRange.value.start * options.itemHeight
  })

  // 滚动处理
  const handleScroll = (event: Event) => {
    const target = event.target as HTMLElement
    scrollTop.value = target.scrollTop
  }

  // 滚动到指定项目
  const scrollToItem = (index: number) => {
    if (containerRef.value) {
      const targetScrollTop = index * options.itemHeight
      containerRef.value.scrollTop = targetScrollTop
      scrollTop.value = targetScrollTop
    }
  }

  // 滚动到顶部
  const scrollToTop = () => {
    scrollToItem(0)
  }

  // 滚动到底部
  const scrollToBottom = () => {
    scrollToItem(items.value.length - 1)
  }

  // 获取滚动状态
  const scrollState = computed(() => {
    const { visibleStart, visibleEnd } = visibleRange.value
    return {
      isAtTop: visibleStart === 0,
      isAtBottom: visibleEnd >= items.value.length,
      scrollPercentage: items.value.length > 0 
        ? (scrollTop.value / (totalHeight.value - options.containerHeight)) * 100 
        : 0
    }
  })

  return {
    containerRef,
    visibleItems,
    totalHeight,
    offsetY,
    scrollTop,
    visibleRange,
    scrollState,
    handleScroll,
    scrollToItem,
    scrollToTop,
    scrollToBottom
  }
}

// 内存优化的数据管理
export function useMemoryOptimizedData<T>(
  maxItems: number = 10000,
  cleanupThreshold: number = 15000
) {
  const data = ref<T[]>([])
  const isCleaningUp = ref(false)

  // 添加数据
  const addData = (newItems: T | T[]) => {
    const items = Array.isArray(newItems) ? newItems : [newItems]
    items.forEach(item => (data.value as T[]).push(item))

    // 检查是否需要清理
    if (data.value.length > cleanupThreshold) {
      cleanup()
    }
  }

  // 清理旧数据
  const cleanup = async () => {
    if (isCleaningUp.value) return

    isCleaningUp.value = true
    
    await nextTick()
    
    // 保留最新的数据
    const keepCount = maxItems
    if (data.value.length > keepCount) {
      data.value.splice(0, data.value.length - keepCount)
    }

    isCleaningUp.value = false
  }

  // 清空所有数据
  const clearData = () => {
    data.value.length = 0
  }

  // 获取内存使用情况
  const getMemoryUsage = () => {
    return {
      itemCount: data.value.length,
      memoryUsage: (JSON.stringify(data.value).length / 1024 / 1024).toFixed(2) + ' MB',
      isNearLimit: data.value.length > maxItems * 0.8
    }
  }

  return {
    data,
    isCleaningUp,
    addData,
    cleanup,
    clearData,
    getMemoryUsage
  }
}
