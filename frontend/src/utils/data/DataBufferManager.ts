/**
 * 高性能数据缓冲管理器
 * 专为高速数据采集和实时显示优化
 * 支持多级缓存、环形缓冲区和智能内存管理
 */

export interface BufferConfig {
  maxChannels: number
  bufferSize: number // 每个通道的缓冲区大小
  compressionThreshold: number // 压缩阈值
  maxMemoryUsage: number // 最大内存使用量 (MB)
}

export interface DataPoint {
  timestamp: number
  value: number
  quality: number // 数据质量 0-1
}

export interface TimeRange {
  start: number
  end: number
}

export interface BufferStatistics {
  totalPoints: number
  memoryUsage: number // MB
  compressionRatio: number
  oldestTimestamp: number
  newestTimestamp: number
}

/**
 * 高性能环形缓冲区
 * 使用TypedArray优化内存访问性能
 */
export class CircularBuffer {
  private timestamps: Float64Array
  private values: Float32Array
  private qualities: Uint8Array
  
  private head: number = 0
  private tail: number = 0
  private size: number = 0
  private capacity: number
  
  constructor(capacity: number) {
    this.capacity = capacity
    this.timestamps = new Float64Array(capacity)
    this.values = new Float32Array(capacity)
    this.qualities = new Uint8Array(capacity)
  }
  
  public push(timestamp: number, value: number, quality: number = 1): void {
    this.timestamps[this.tail] = timestamp
    this.values[this.tail] = value
    this.qualities[this.tail] = Math.round(quality * 255)
    
    this.tail = (this.tail + 1) % this.capacity
    
    if (this.size < this.capacity) {
      this.size++
    } else {
      // 缓冲区满，移动头指针
      this.head = (this.head + 1) % this.capacity
    }
  }
  
  public pushBatch(data: DataPoint[]): void {
    for (const point of data) {
      this.push(point.timestamp, point.value, point.quality)
    }
  }
  
  public getRange(startIndex: number, count: number): DataPoint[] {
    const result: DataPoint[] = []
    
    for (let i = 0; i < count && i < this.size; i++) {
      const index = (this.head + startIndex + i) % this.capacity
      result.push({
        timestamp: this.timestamps[index],
        value: this.values[index],
        quality: this.qualities[index] / 255
      })
    }
    
    return result
  }
  
  public getTimeRange(startTime: number, endTime: number): DataPoint[] {
    const result: DataPoint[] = []
    
    // 二分查找起始位置
    const startIndex = this.binarySearchTime(startTime)
    
    for (let i = startIndex; i < this.size; i++) {
      const index = (this.head + i) % this.capacity
      const timestamp = this.timestamps[index]
      
      if (timestamp > endTime) break
      
      if (timestamp >= startTime) {
        result.push({
          timestamp,
          value: this.values[index],
          quality: this.qualities[index] / 255
        })
      }
    }
    
    return result
  }
  
  private binarySearchTime(targetTime: number): number {
    let left = 0
    let right = this.size - 1
    
    while (left <= right) {
      const mid = Math.floor((left + right) / 2)
      const index = (this.head + mid) % this.capacity
      const timestamp = this.timestamps[index]
      
      if (timestamp < targetTime) {
        left = mid + 1
      } else {
        right = mid - 1
      }
    }
    
    return left
  }
  
  public getLatest(count: number): DataPoint[] {
    const startIndex = Math.max(0, this.size - count)
    return this.getRange(startIndex, count)
  }
  
  public clear(): void {
    this.head = 0
    this.tail = 0
    this.size = 0
  }
  
  public getSize(): number {
    return this.size
  }
  
  public getCapacity(): number {
    return this.capacity
  }
  
  public getOldestTimestamp(): number {
    if (this.size === 0) return 0
    return this.timestamps[this.head]
  }
  
  public getNewestTimestamp(): number {
    if (this.size === 0) return 0
    const lastIndex = (this.tail - 1 + this.capacity) % this.capacity
    return this.timestamps[lastIndex]
  }
  
  public getMemoryUsage(): number {
    // 计算内存使用量 (MB)
    const timestampBytes = this.timestamps.byteLength
    const valueBytes = this.values.byteLength
    const qualityBytes = this.qualities.byteLength
    
    return (timestampBytes + valueBytes + qualityBytes) / (1024 * 1024)
  }
}

/**
 * 多级缓存管理器
 * L1: 实时数据缓存 (最近数据)
 * L2: 压缩数据缓存 (中期数据)
 * L3: 历史数据缓存 (长期数据)
 */
export class MultiLevelCache {
  private l1Cache: Map<number, CircularBuffer> = new Map() // 通道ID -> 缓冲区
  private l2Cache: Map<number, CircularBuffer> = new Map()
  private l3Cache: Map<number, CircularBuffer> = new Map()
  
  private config: BufferConfig
  private compressionWorker: Worker | null = null
  
  constructor(config: BufferConfig) {
    this.config = config
    this.initializeWorker()
  }
  
  private initializeWorker(): void {
    // 创建Web Worker用于数据压缩
    const workerCode = `
      // LTTB压缩算法的Web Worker实现
      self.onmessage = function(e) {
        const { data, targetPoints, channelId } = e.data
        const compressed = lttbCompress(data, targetPoints)
        self.postMessage({ channelId, compressed })
      }
      
      function lttbCompress(data, targetPoints) {
        if (data.length <= targetPoints) return data
        
        const sampled = []
        const bucketSize = (data.length - 2) / (targetPoints - 2)
        
        sampled.push(data[0]) // 保留第一个点
        
        for (let i = 0; i < targetPoints - 2; i++) {
          const bucketStart = Math.floor(i * bucketSize) + 1
          const bucketEnd = Math.floor((i + 1) * bucketSize) + 1
          
          // 计算下一个桶的平均点
          const nextBucketStart = Math.floor((i + 1) * bucketSize) + 1
          const nextBucketEnd = Math.floor((i + 2) * bucketSize) + 1
          
          let avgTimestamp = 0, avgValue = 0, avgCount = 0
          for (let j = nextBucketStart; j < nextBucketEnd && j < data.length; j++) {
            avgTimestamp += data[j].timestamp
            avgValue += data[j].value
            avgCount++
          }
          
          if (avgCount > 0) {
            avgTimestamp /= avgCount
            avgValue /= avgCount
          }
          
          // 找到形成最大三角形面积的点
          let maxArea = -1
          let maxAreaIndex = bucketStart
          
          const prevPoint = sampled[sampled.length - 1]
          
          for (let j = bucketStart; j < bucketEnd && j < data.length; j++) {
            const area = Math.abs(
              (prevPoint.timestamp - avgTimestamp) * (data[j].value - prevPoint.value) -
              (prevPoint.timestamp - data[j].timestamp) * (avgValue - prevPoint.value)
            ) * 0.5
            
            if (area > maxArea) {
              maxArea = area
              maxAreaIndex = j
            }
          }
          
          sampled.push(data[maxAreaIndex])
        }
        
        sampled.push(data[data.length - 1]) // 保留最后一个点
        return sampled
      }
    `
    
    const blob = new Blob([workerCode], { type: 'application/javascript' })
    this.compressionWorker = new Worker(URL.createObjectURL(blob))
    
    this.compressionWorker.onmessage = (e) => {
      const { channelId, compressed } = e.data
      this.handleCompressedData(channelId, compressed)
    }
  }
  
  public addData(channelId: number, data: DataPoint[]): void {
    // 确保通道缓冲区存在
    this.ensureChannelBuffers(channelId)
    
    // 添加到L1缓存
    const l1Buffer = this.l1Cache.get(channelId)!
    l1Buffer.pushBatch(data)
    
    // 检查是否需要压缩和迁移数据
    this.checkCompressionNeeded(channelId)
    
    // 检查内存使用量
    this.checkMemoryUsage()
  }
  
  private ensureChannelBuffers(channelId: number): void {
    if (!this.l1Cache.has(channelId)) {
      this.l1Cache.set(channelId, new CircularBuffer(this.config.bufferSize))
      this.l2Cache.set(channelId, new CircularBuffer(this.config.bufferSize * 2))
      this.l3Cache.set(channelId, new CircularBuffer(this.config.bufferSize * 10))
    }
  }
  
  private checkCompressionNeeded(channelId: number): void {
    const l1Buffer = this.l1Cache.get(channelId)!
    
    if (l1Buffer.getSize() >= this.config.compressionThreshold) {
      // 获取需要压缩的数据
      const dataToCompress = l1Buffer.getRange(0, this.config.compressionThreshold)
      
      // 发送到Web Worker进行压缩
      if (this.compressionWorker) {
        this.compressionWorker.postMessage({
          channelId,
          data: dataToCompress,
          targetPoints: Math.floor(this.config.compressionThreshold / 4)
        })
      }
    }
  }
  
  private handleCompressedData(channelId: number, compressedData: DataPoint[]): void {
    const l2Buffer = this.l2Cache.get(channelId)
    if (l2Buffer) {
      l2Buffer.pushBatch(compressedData)
    }
    
    // 检查L2缓存是否需要进一步压缩到L3
    this.checkL2ToL3Migration(channelId)
  }
  
  private checkL2ToL3Migration(channelId: number): void {
    const l2Buffer = this.l2Cache.get(channelId)!
    
    if (l2Buffer.getSize() >= this.config.compressionThreshold * 2) {
      const dataToMigrate = l2Buffer.getRange(0, this.config.compressionThreshold)
      
      // 进一步压缩到L3
      if (this.compressionWorker) {
        this.compressionWorker.postMessage({
          channelId: channelId + 1000, // 使用不同ID标识L3压缩
          data: dataToMigrate,
          targetPoints: Math.floor(this.config.compressionThreshold / 10)
        })
      }
    }
  }
  
  private checkMemoryUsage(): void {
    const totalMemory = this.getTotalMemoryUsage()
    
    if (totalMemory > this.config.maxMemoryUsage) {
      // 清理最旧的L3数据
      this.cleanupOldData()
    }
  }
  
  private cleanupOldData(): void {
    for (const [channelId, buffer] of this.l3Cache) {
      if (buffer.getSize() > 0) {
        // 清理最旧的25%数据
        const pointsToRemove = Math.floor(buffer.getSize() * 0.25)
        const remainingData = buffer.getRange(pointsToRemove, buffer.getSize() - pointsToRemove)
        
        buffer.clear()
        buffer.pushBatch(remainingData)
      }
    }
  }
  
  public getData(channelId: number, timeRange: TimeRange, maxPoints?: number): DataPoint[] {
    const allData: DataPoint[] = []
    
    // 从所有缓存级别收集数据
    const l1Buffer = this.l1Cache.get(channelId)
    const l2Buffer = this.l2Cache.get(channelId)
    const l3Buffer = this.l3Cache.get(channelId)
    
    if (l3Buffer) {
      allData.push(...l3Buffer.getTimeRange(timeRange.start, timeRange.end))
    }
    
    if (l2Buffer) {
      allData.push(...l2Buffer.getTimeRange(timeRange.start, timeRange.end))
    }
    
    if (l1Buffer) {
      allData.push(...l1Buffer.getTimeRange(timeRange.start, timeRange.end))
    }
    
    // 按时间戳排序
    allData.sort((a, b) => a.timestamp - b.timestamp)
    
    // 去重（相同时间戳保留质量最高的）
    const deduplicatedData = this.deduplicateData(allData)
    
    // 如果指定了最大点数，进行采样
    if (maxPoints && deduplicatedData.length > maxPoints) {
      return this.sampleData(deduplicatedData, maxPoints)
    }
    
    return deduplicatedData
  }
  
  private deduplicateData(data: DataPoint[]): DataPoint[] {
    const result: DataPoint[] = []
    const timestampMap = new Map<number, DataPoint>()
    
    for (const point of data) {
      const existing = timestampMap.get(point.timestamp)
      if (!existing || point.quality > existing.quality) {
        timestampMap.set(point.timestamp, point)
      }
    }
    
    return Array.from(timestampMap.values()).sort((a, b) => a.timestamp - b.timestamp)
  }
  
  private sampleData(data: DataPoint[], targetPoints: number): DataPoint[] {
    if (data.length <= targetPoints) return data
    
    const step = data.length / targetPoints
    const sampled: DataPoint[] = []
    
    for (let i = 0; i < targetPoints; i++) {
      const index = Math.floor(i * step)
      sampled.push(data[index])
    }
    
    return sampled
  }
  
  public getLatestData(channelId: number, count: number): DataPoint[] {
    const l1Buffer = this.l1Cache.get(channelId)
    return l1Buffer ? l1Buffer.getLatest(count) : []
  }
  
  public clearChannel(channelId: number): void {
    this.l1Cache.get(channelId)?.clear()
    this.l2Cache.get(channelId)?.clear()
    this.l3Cache.get(channelId)?.clear()
  }
  
  public clearAllChannels(): void {
    for (const channelId of this.l1Cache.keys()) {
      this.clearChannel(channelId)
    }
  }
  
  public getStatistics(): BufferStatistics {
    let totalPoints = 0
    let oldestTimestamp = Infinity
    let newestTimestamp = -Infinity
    
    for (const buffer of this.l1Cache.values()) {
      totalPoints += buffer.getSize()
      if (buffer.getSize() > 0) {
        oldestTimestamp = Math.min(oldestTimestamp, buffer.getOldestTimestamp())
        newestTimestamp = Math.max(newestTimestamp, buffer.getNewestTimestamp())
      }
    }
    
    for (const buffer of this.l2Cache.values()) {
      totalPoints += buffer.getSize()
      if (buffer.getSize() > 0) {
        oldestTimestamp = Math.min(oldestTimestamp, buffer.getOldestTimestamp())
        newestTimestamp = Math.max(newestTimestamp, buffer.getNewestTimestamp())
      }
    }
    
    for (const buffer of this.l3Cache.values()) {
      totalPoints += buffer.getSize()
      if (buffer.getSize() > 0) {
        oldestTimestamp = Math.min(oldestTimestamp, buffer.getOldestTimestamp())
        newestTimestamp = Math.max(newestTimestamp, buffer.getNewestTimestamp())
      }
    }
    
    const memoryUsage = this.getTotalMemoryUsage()
    const compressionRatio = this.calculateCompressionRatio()
    
    return {
      totalPoints,
      memoryUsage,
      compressionRatio,
      oldestTimestamp: oldestTimestamp === Infinity ? 0 : oldestTimestamp,
      newestTimestamp: newestTimestamp === -Infinity ? 0 : newestTimestamp
    }
  }
  
  private getTotalMemoryUsage(): number {
    let totalMemory = 0
    
    for (const buffer of this.l1Cache.values()) {
      totalMemory += buffer.getMemoryUsage()
    }
    
    for (const buffer of this.l2Cache.values()) {
      totalMemory += buffer.getMemoryUsage()
    }
    
    for (const buffer of this.l3Cache.values()) {
      totalMemory += buffer.getMemoryUsage()
    }
    
    return totalMemory
  }
  
  private calculateCompressionRatio(): number {
    let originalSize = 0
    let compressedSize = 0
    
    for (const buffer of this.l1Cache.values()) {
      originalSize += buffer.getSize()
    }
    
    for (const buffer of this.l2Cache.values()) {
      compressedSize += buffer.getSize()
    }
    
    for (const buffer of this.l3Cache.values()) {
      compressedSize += buffer.getSize()
    }
    
    return originalSize > 0 ? compressedSize / originalSize : 1
  }
  
  public dispose(): void {
    if (this.compressionWorker) {
      this.compressionWorker.terminate()
      this.compressionWorker = null
    }
    
    this.l1Cache.clear()
    this.l2Cache.clear()
    this.l3Cache.clear()
  }
}

/**
 * 数据缓冲管理器主类
 * 统一管理多通道数据缓冲和压缩
 */
export class DataBufferManager {
  private cache: MultiLevelCache
  private config: BufferConfig
  private dataRate: number = 0
  private lastDataTime: number = 0
  private dataCount: number = 0
  
  constructor(config: BufferConfig) {
    this.config = config
    this.cache = new MultiLevelCache(config)
  }
  
  public addData(channelId: number, data: DataPoint[]): void {
    this.cache.addData(channelId, data)
    this.updateDataRate(data.length)
  }
  
  public addSinglePoint(channelId: number, timestamp: number, value: number, quality: number = 1): void {
    this.addData(channelId, [{ timestamp, value, quality }])
  }
  
  public getData(channelId: number, timeRange: TimeRange, maxPoints?: number): DataPoint[] {
    return this.cache.getData(channelId, timeRange, maxPoints)
  }
  
  public getLatestData(channelId: number, count: number): DataPoint[] {
    return this.cache.getLatestData(channelId, count)
  }
  
  public getRealtimeData(channelId: number, timeWindow: number): DataPoint[] {
    const now = Date.now()
    const timeRange: TimeRange = {
      start: now - timeWindow,
      end: now
    }
    return this.getData(channelId, timeRange)
  }
  
  private updateDataRate(pointCount: number): void {
    const now = performance.now()
    this.dataCount += pointCount
    
    if (now - this.lastDataTime >= 1000) {
      this.dataRate = (this.dataCount * 1000) / (now - this.lastDataTime)
      this.dataCount = 0
      this.lastDataTime = now
    }
  }
  
  public getDataRate(): number {
    return this.dataRate
  }
  
  public getStatistics(): BufferStatistics {
    return this.cache.getStatistics()
  }
  
  public clearChannel(channelId: number): void {
    this.cache.clearChannel(channelId)
  }
  
  public clearAllChannels(): void {
    this.cache.clearAllChannels()
    this.dataRate = 0
    this.dataCount = 0
  }
  
  public dispose(): void {
    this.cache.dispose()
  }
}
