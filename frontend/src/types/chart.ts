// 图表数据类型
export interface ChartData {
  series: number[][] | number[];
  xStart?: number;
  xInterval?: number;
  labels?: string[];
  chartType?: 'line' | 'bar' | 'scatter' | 'area' | 'histogram';
  timestamps?: number[];
  sampleRate?: number;
  channels?: string[];
}

// 图表配置选项
export interface ChartOptions {
  autoScale: boolean;
  logarithmic: boolean;
  splitView: boolean;
  legendVisible: boolean;
  cursorMode: 'zoom' | 'cursor' | 'disabled';
  gridEnabled: boolean;
  minorGridEnabled: boolean;
  backgroundColor?: string;
  theme?: 'light' | 'dark' | 'scientific';
  realTime?: boolean;
  bufferSize?: number;
  triggerMode?: 'auto' | 'normal' | 'single';
  triggerLevel?: number;
  triggerChannel?: number;
  dualYAxis?: boolean; // 启用双Y轴模式
  yAxes?: AxisConfig[]; // Y轴配置数组，支持多个Y轴
}

// 坐标轴配置
export interface AxisConfig {
  min?: number;
  max?: number;
  autoScale: boolean;
  logarithmic: boolean;
  label: string;
  unit?: string;
  format?: string;
  division?: number;
  offset?: number;
}

// 系列配置
export interface SeriesConfig {
  name: string;
  color: string;
  lineWidth: number;
  lineType: 'solid' | 'dashed' | 'dotted';
  markerType: 'none' | 'circle' | 'square' | 'triangle' | 'diamond';
  markerSize: number;
  visible: boolean;
  chartType?: 'line' | 'bar' | 'scatter' | 'area' | 'histogram';
  fillOpacity?: number;
  barWidth?: number;
  stack?: string;
  yOffset?: number;
  yScale?: number;
  coupling?: 'DC' | 'AC' | 'GND';
  bandwidth?: number;
  probe?: number;
  yAxisIndex?: number; // 指定使用哪个Y轴 (0=左轴, 1=右轴)
}

// 游标配置
export interface CursorConfig {
  enabled: boolean;
  mode: 'zoom' | 'cursor' | 'disabled';
  color: string;
  lineWidth: number;
  value?: number;
  x1?: number;
  x2?: number;
  y1?: number;
  y2?: number;
  deltaX?: number;
  deltaY?: number;
}

// 触发配置
export interface TriggerConfig {
  enabled: boolean;
  mode: 'auto' | 'normal' | 'single';
  source: number;
  level: number;
  slope: 'rising' | 'falling' | 'both';
  coupling: 'DC' | 'AC' | 'HF' | 'LF';
  holdoff: number;
}

// 测量配置
export interface MeasurementConfig {
  enabled: boolean;
  type: 'frequency' | 'period' | 'amplitude' | 'rms' | 'peak' | 'mean' | 'min' | 'max';
  channel: number;
  unit: string;
  precision: number;
}

// 图表事件
export interface ChartEvents {
  onDataUpdate?: (data: ChartData) => void;
  onZoom?: (range: { xMin: number; xMax: number; yMin: number; yMax: number }) => void;
  onCursorMove?: (position: { x: number; y: number }) => void;
  onSeriesToggle?: (seriesIndex: number, visible: boolean) => void;
  onTrigger?: (triggerInfo: { time: number; level: number; channel: number }) => void;
  onMeasurement?: (measurement: { type: string; value: number; unit: string }) => void;
}

// 导出选项
export interface ExportOptions {
  format: 'png' | 'jpg' | 'svg' | 'csv' | 'mat';
  width?: number;
  height?: number;
  quality?: number;
  filename?: string;
  includeSettings?: boolean;
}

// 实时数据配置
export interface RealTimeConfig {
  enabled: boolean;
  bufferSize: number;
  updateInterval: number;
  scrollMode: 'append' | 'replace' | 'trigger';
  maxDataPoints?: number;
  compressionEnabled?: boolean;
}

// 数据缓冲区配置
export interface DataBufferConfig {
  maxSize: number;
  autoResize: boolean;
  compressionEnabled: boolean;
  compressionRatio: number;
  retentionTime?: number; // 数据保留时间（毫秒）
  memoryLimit?: number;
}

// 数据标记和注释
export interface DataAnnotation {
  id: string;
  x: number;
  y?: number;
  text: string;
  color?: string;
  backgroundColor?: string;
  fontSize?: number;
  position?: 'top' | 'bottom' | 'left' | 'right';
  visible: boolean;
  type?: 'marker' | 'region' | 'line';
}

// 图表模板
export interface ChartTemplate {
  id: string;
  name: string;
  description?: string;
  chartOptions: ChartOptions;
  seriesConfigs: SeriesConfig[];
  annotations?: DataAnnotation[];
  createdAt: Date;
  updatedAt: Date;
}

// 频谱分析配置
export interface SpectrumConfig {
  enabled: boolean;
  fftSize: number;
  windowType: 'hanning' | 'hamming' | 'blackman' | 'rectangular' | 'kaiser';
  overlap: number;
  averages: number;
  peakHold: boolean;
  referenceLevel: number;
  dynamicRange: number;
}

// 示波器配置
export interface OscilloscopeConfig {
  timebase: {
    scale: number; // 时间/格
    position: number; // 时间偏移
    mode: 'normal' | 'roll' | 'xy';
  };
  channels: {
    scale: number; // 电压/格
    position: number; // 垂直偏移
    coupling: 'DC' | 'AC' | 'GND';
    bandwidth: number;
    probe: number;
    invert: boolean;
  }[];
  trigger: TriggerConfig;
  acquisition: {
    mode: 'sample' | 'average' | 'envelope';
    depth: number;
    sampleRate: number;
  };
  display: {
    persistence: boolean;
    intensity: number;
    focus: number;
  };
}

// 数据生成器配置（用于测试）
export interface SignalGeneratorConfig {
  type: 'sine' | 'square' | 'triangle' | 'sawtooth' | 'noise' | 'pulse';
  frequency: number;
  amplitude: number;
  offset: number;
  phase: number;
  dutyCycle?: number; // 方波占空比
  noiseLevel?: number;
  modulation?: {
    enabled: boolean;
    type: 'AM' | 'FM' | 'PM';
    frequency: number;
    depth: number;
  };
}

// 波形数据类型（向后兼容）
export interface WaveformData extends ChartData {}

// 波形配置选项（向后兼容）
export interface WaveformOptions extends ChartOptions {}

// 波形系列配置（向后兼容）
export interface WaveformSeriesConfig extends SeriesConfig {}

// 波形事件（向后兼容）
export interface WaveformEvents extends ChartEvents {}
