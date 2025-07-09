/**
 * 信号发生器相关类型定义
 * 支持任意波形生成、调制功能、扫频等专业功能
 */

// 波形类型枚举
export enum WaveformType {
  SINE = 'sine',           // 正弦波
  SQUARE = 'square',       // 方波
  TRIANGLE = 'triangle',   // 三角波
  SAWTOOTH = 'sawtooth',   // 锯齿波
  PULSE = 'pulse',         // 脉冲波
  NOISE = 'noise',         // 噪声
  DC = 'dc',               // 直流
  ARBITRARY = 'arbitrary'  // 任意波形
}

// 调制类型枚举
export enum ModulationType {
  NONE = 'none',    // 无调制
  AM = 'am',        // 幅度调制
  FM = 'fm',        // 频率调制
  PM = 'pm',        // 相位调制
  FSK = 'fsk',      // 频移键控
  ASK = 'ask',      // 幅移键控
  PSK = 'psk'       // 相移键控
}

// 扫频模式枚举
export enum SweepMode {
  NONE = 'none',        // 无扫频
  LINEAR = 'linear',    // 线性扫频
  LOG = 'log',          // 对数扫频
  STEP = 'step'         // 步进扫频
}

// 触发模式枚举
export enum TriggerMode {
  CONTINUOUS = 'continuous',  // 连续输出
  TRIGGERED = 'triggered',    // 触发输出
  GATED = 'gated',           // 门控输出
  BURST = 'burst'            // 突发输出
}

// 输出阻抗枚举
export enum OutputImpedance {
  HIGH_Z = 'high_z',    // 高阻抗
  FIFTY_OHM = '50_ohm'  // 50欧姆
}

// 波形参数接口
export interface WaveformParameters {
  type: WaveformType;
  frequency: number;        // 频率 (Hz)
  amplitude: number;        // 幅度 (V)
  offset: number;          // 直流偏置 (V)
  phase: number;           // 相位 (度)
  dutyCycle: number;       // 占空比 (%) - 仅方波和脉冲波
  symmetry: number;        // 对称性 (%) - 仅三角波
  riseTime: number;        // 上升时间 (ns) - 仅脉冲波
  fallTime: number;        // 下降时间 (ns) - 仅脉冲波
}

// 调制参数接口
export interface ModulationParameters {
  type: ModulationType;
  enabled: boolean;
  frequency: number;       // 调制频率 (Hz)
  depth: number;          // 调制深度 (%)
  deviation: number;      // 频偏 (Hz) - 仅FM
  phaseDeviation: number; // 相偏 (度) - 仅PM
  source: 'internal' | 'external'; // 调制源
}

// 扫频参数接口
export interface SweepParameters {
  mode: SweepMode;
  enabled: boolean;
  startFrequency: number;  // 起始频率 (Hz)
  stopFrequency: number;   // 终止频率 (Hz)
  sweepTime: number;       // 扫频时间 (s)
  returnTime: number;      // 返回时间 (s)
  direction: 'up' | 'down' | 'updown'; // 扫频方向
}

// 突发模式参数接口
export interface BurstParameters {
  enabled: boolean;
  cycles: number;          // 突发周期数
  period: number;          // 突发周期 (s)
  phase: number;           // 突发相位 (度)
  polarity: 'normal' | 'inverted'; // 极性
  gateSource: 'internal' | 'external'; // 门控源
}

// 输出配置接口
export interface OutputConfiguration {
  enabled: boolean;
  impedance: OutputImpedance;
  invert: boolean;         // 输出反相
  sync: boolean;           // 同步输出
  protection: boolean;     // 过载保护
}

// 任意波形数据接口
export interface ArbitraryWaveform {
  name: string;
  description: string;
  sampleRate: number;      // 采样率 (Sa/s)
  length: number;          // 波形长度
  data: number[];          // 波形数据 (-1.0 到 1.0)
  interpolation: 'linear' | 'cubic'; // 插值方式
}

// 信号发生器状态接口
export interface SignalGeneratorStatus {
  outputEnabled: boolean;
  frequency: number;
  amplitude: number;
  waveformType: WaveformType;
  modulationEnabled: boolean;
  sweepEnabled: boolean;
  burstEnabled: boolean;
  temperature: number;     // 设备温度 (°C)
  errorCode: number;       // 错误代码
  errorMessage: string;    // 错误信息
}

// 信号发生器配置接口
export interface SignalGeneratorConfig {
  // 基本波形参数
  waveform: WaveformParameters;
  
  // 调制参数
  modulation: ModulationParameters;
  
  // 扫频参数
  sweep: SweepParameters;
  
  // 突发模式参数
  burst: BurstParameters;
  
  // 输出配置
  output: OutputConfiguration;
  
  // 触发配置
  trigger: {
    mode: TriggerMode;
    source: 'internal' | 'external' | 'manual';
    edge: 'rising' | 'falling';
    level: number;         // 触发电平 (V)
  };
  
  // 时钟配置
  clock: {
    source: 'internal' | 'external';
    frequency: number;     // 时钟频率 (Hz)
    reference: '10MHz' | '100MHz'; // 参考时钟
  };
}

// 频率范围定义
export interface FrequencyRange {
  min: number;
  max: number;
  resolution: number;
  unit: string;
}

// 幅度范围定义
export interface AmplitudeRange {
  min: number;
  max: number;
  resolution: number;
  unit: string;
}

// 设备规格接口
export interface SignalGeneratorSpecs {
  model: string;
  channels: number;
  frequencyRange: FrequencyRange;
  amplitudeRange: AmplitudeRange;
  offsetRange: AmplitudeRange;
  sampleRate: number;      // 最大采样率 (Sa/s)
  resolution: number;      // 垂直分辨率 (bits)
  waveformMemory: number;  // 波形内存 (points)
  supportedWaveforms: WaveformType[];
  supportedModulations: ModulationType[];
  maxHarmonics: number;    // 最大谐波数
  spuriousFree: number;    // 无杂散动态范围 (dBc)
  phaseNoise: number;      // 相位噪声 (dBc/Hz)
}

// 预设波形库接口
export interface WaveformLibrary {
  standard: ArbitraryWaveform[];    // 标准波形
  user: ArbitraryWaveform[];        // 用户自定义波形
  builtin: ArbitraryWaveform[];     // 内置波形
}

// 测量结果接口
export interface MeasurementResult {
  frequency: number;
  amplitude: number;
  rms: number;
  peakToPeak: number;
  thd: number;             // 总谐波失真
  snr: number;             // 信噪比
  timestamp: Date;
}

// 校准数据接口
export interface CalibrationData {
  date: Date;
  temperature: number;
  frequencyCorrection: number[];
  amplitudeCorrection: number[];
  phaseCorrection: number[];
  valid: boolean;
}

// 事件类型定义
export type SignalGeneratorEvent = 
  | 'output-enabled'
  | 'output-disabled'
  | 'frequency-changed'
  | 'amplitude-changed'
  | 'waveform-changed'
  | 'modulation-changed'
  | 'sweep-started'
  | 'sweep-completed'
  | 'burst-triggered'
  | 'error-occurred'
  | 'calibration-required';

// 事件数据接口
export interface SignalGeneratorEventData {
  type: SignalGeneratorEvent;
  timestamp: Date;
  data: any;
  source: string;
}

// 导出的默认配置
export const DEFAULT_SIGNAL_GENERATOR_CONFIG: SignalGeneratorConfig = {
  waveform: {
    type: WaveformType.SINE,
    frequency: 1000,
    amplitude: 1.0,
    offset: 0.0,
    phase: 0.0,
    dutyCycle: 50.0,
    symmetry: 50.0,
    riseTime: 10,
    fallTime: 10
  },
  modulation: {
    type: ModulationType.NONE,
    enabled: false,
    frequency: 100,
    depth: 50,
    deviation: 1000,
    phaseDeviation: 90,
    source: 'internal'
  },
  sweep: {
    mode: SweepMode.NONE,
    enabled: false,
    startFrequency: 100,
    stopFrequency: 10000,
    sweepTime: 1.0,
    returnTime: 0.1,
    direction: 'up'
  },
  burst: {
    enabled: false,
    cycles: 10,
    period: 0.001,
    phase: 0,
    polarity: 'normal',
    gateSource: 'internal'
  },
  output: {
    enabled: false,
    impedance: OutputImpedance.FIFTY_OHM,
    invert: false,
    sync: false,
    protection: true
  },
  trigger: {
    mode: TriggerMode.CONTINUOUS,
    source: 'internal',
    edge: 'rising',
    level: 0.0
  },
  clock: {
    source: 'internal',
    frequency: 100000000,
    reference: '10MHz'
  }
};

// 标准频率预设
export const STANDARD_FREQUENCIES = [
  1, 10, 100, 1000, 10000, 100000, 1000000, 10000000
];

// 标准幅度预设
export const STANDARD_AMPLITUDES = [
  0.001, 0.01, 0.1, 1.0, 5.0, 10.0
];
