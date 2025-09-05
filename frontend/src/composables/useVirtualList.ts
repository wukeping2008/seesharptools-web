import { ref, computed, onMounted, onUnmounted, Ref } from 'vue'

export interface VirtualListOptions {
  itemHeight: number | ((index: number) => number)
  overscan?: number
  scrollThreshold?: number
  bufferSize?: number
}

export interface VirtualListReturn<T> {
  containerRef: Ref<HTMLElement | undefined>
  wrapperRef: Ref<HTMLElement | undefined>
  visibleItems: Ref<T[]>
  visibleRange: Ref<{ start: number; end: number }>
  totalHeight: Ref<number>
  scrollToIndex: (index: number) => void
  scrollToTop: () => void
  scrollToBottom: () => void
  refresh: () => void
}

/**
 * 高性能虚拟列表Hook
 * 支持大数据量列表的流畅渲染
 */
export function useVirtualList<T>(
  items: Ref<T[]>,
  options: VirtualListOptions
): VirtualListReturn<T> {
  const {
    itemHeight,
    overscan = 5,
    scrollThreshold = 50,
    bufferSize = 3
  } = options

  // DOM引用
  const containerRef = ref<HTMLElement>()
  const wrapperRef = ref<HTMLElement>()

  // 状态
  const scrollTop = ref(0)
  const containerHeight = ref(0)
  const visibleRange = ref({ start: 0, end: 0 })
  const itemHeightCache = new Map<number, number>()

  // 计算项目高度
  const getItemHeight = (index: number): number => {
    if (typeof itemHeight === 'function') {
      if (!itemHeightCache.has(index)) {
        itemHeightCache.set(index, itemHeight(index))
      }
      return itemHeightCache.get(index)!
    }
    return itemHeight
  }

  // 计算总高度
  const totalHeight = computed(() => {
    let height = 0
    for (let i = 0; i < items.value.length; i++) {
      height += getItemHeight(i)
    }
    return height
  })

  // 计算可见项目范围
  const calculateVisibleRange = () => {
    if (!containerRef.value) return

    const scrollY = scrollTop.value
    const viewportHeight = containerHeight.value
    
    let accumulatedHeight = 0
    let startIndex = 0
    let endIndex = items.value.length - 1

    // 查找起始索引
    for (let i = 0; i < items.value.length; i++) {
      const height = getItemHeight(i)
      if (accumulatedHeight + height > scrollY) {
        startIndex = Math.max(0, i - overscan)
        break
      }
      accumulatedHeight += height
    }

    // 查找结束索引
    accumulatedHeight = 0
    for (let i = startIndex; i < items.value.length; i++) {
      if (accumulatedHeight > viewportHeight + scrollY) {
        endIndex = Math.min(items.value.length - 1, i + overscan)
        break
      }
      accumulatedHeight += getItemHeight(i)
    }

    visibleRange.value = { start: startIndex, end: endIndex }
  }

  // 可见项目
  const visibleItems = computed(() => {
    const { start, end } = visibleRange.value
    return items.value.slice(start, end + 1).map((item, index) => ({
      ...item,
      _virtualIndex: start + index,
      _virtualOffset: calculateItemOffset(start + index)
    }))
  })

  // 计算项目偏移
  const calculateItemOffset = (index: number): number => {
    let offset = 0
    for (let i = 0; i < index; i++) {
      offset += getItemHeight(i)
    }
    return offset
  }

  // 滚动处理
  let scrollTimer: number | null = null
  const handleScroll = () => {
    if (!containerRef.value) return

    scrollTop.value = containerRef.value.scrollTop

    // 防抖处理
    if (scrollTimer) {
      clearTimeout(scrollTimer)
    }

    scrollTimer = window.setTimeout(() => {
      calculateVisibleRange()
    }, 16) // 约60fps
  }

  // 窗口大小变化处理
  const handleResize = () => {
    if (!containerRef.value) return
    containerHeight.value = containerRef.value.clientHeight
    calculateVisibleRange()
  }

  // 滚动到指定索引
  const scrollToIndex = (index: number) => {
    if (!containerRef.value) return
    
    const offset = calculateItemOffset(index)
    containerRef.value.scrollTop = offset
  }

  // 滚动到顶部
  const scrollToTop = () => {
    if (!containerRef.value) return
    containerRef.value.scrollTop = 0
  }

  // 滚动到底部
  const scrollToBottom = () => {
    if (!containerRef.value) return
    containerRef.value.scrollTop = totalHeight.value
  }

  // 刷新
  const refresh = () => {
    itemHeightCache.clear()
    calculateVisibleRange()
  }

  // 生命周期
  onMounted(() => {
    if (containerRef.value) {
      containerRef.value.addEventListener('scroll', handleScroll, { passive: true })
      containerHeight.value = containerRef.value.clientHeight
      calculateVisibleRange()
    }
    window.addEventListener('resize', handleResize)
  })

  onUnmounted(() => {
    if (containerRef.value) {
      containerRef.value.removeEventListener('scroll', handleScroll)
    }
    window.removeEventListener('resize', handleResize)
    if (scrollTimer) {
      clearTimeout(scrollTimer)
    }
  })

  return {
    containerRef,
    wrapperRef,
    visibleItems,
    visibleRange,
    totalHeight,
    scrollToIndex,
    scrollToTop,
    scrollToBottom,
    refresh
  }
}

/**
 * 动态高度虚拟列表Hook
 * 支持不定高度项目的虚拟滚动
 */
export function useDynamicVirtualList<T>(
  items: Ref<T[]>,
  estimatedItemHeight: number = 50
) {
  const measuredHeights = new Map<number, number>()
  const estimatedTotalHeight = ref(items.value.length * estimatedItemHeight)

  // 测量项目高度
  const measureItem = (index: number, element: HTMLElement) => {
    const height = element.getBoundingClientRect().height
    measuredHeights.set(index, height)
    updateEstimatedTotalHeight()
  }

  // 更新预估总高度
  const updateEstimatedTotalHeight = () => {
    let totalHeight = 0
    for (let i = 0; i < items.value.length; i++) {
      totalHeight += measuredHeights.get(i) || estimatedItemHeight
    }
    estimatedTotalHeight.value = totalHeight
  }

  // 获取项目高度
  const getItemHeight = (index: number) => {
    return measuredHeights.get(index) || estimatedItemHeight
  }

  return useVirtualList(items, {
    itemHeight: getItemHeight,
    overscan: 3
  })
}