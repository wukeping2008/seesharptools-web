# 🚀 SeeSharpTools Web - Professional Test & Measurement Instrument Controls

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Vue 3](https://img.shields.io/badge/Vue-3.x-4FC08D.svg)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6.svg)](https://www.typescriptlang.org/)
[![.NET Core](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/)

> **World's First Professional Web-Based Test & Measurement Platform**  
> Bringing desktop-grade performance to the browser with AI-powered control generation

## 🌟 Overview

SeeSharpTools Web is a revolutionary web-based platform that brings professional test and measurement capabilities to modern browsers. Built with Vue 3 + TypeScript frontend and .NET Core backend, it delivers desktop-grade performance with cloud-native scalability.

### 🎯 Key Features

- **🧠 AI-Powered Control Generation** - Generate custom controls using natural language descriptions
- **📊 Real-Time Data Visualization** - Handle 1GS/s data streams with 32-channel synchronization
- **🔧 Hardware Integration** - Native support for PXI and USB data acquisition devices
- **🎨 Visual Project Designer** - Drag-and-drop interface for creating measurement applications
- **⚡ High-Performance Rendering** - WebGL-accelerated graphics with virtual scrolling
- **📱 Responsive Design** - Optimized for desktop, tablet, and mobile devices

## 🏗️ Architecture

### Frontend Stack
- **Framework**: Vue 3 + TypeScript + Composition API
- **UI Library**: Element Plus + Custom Professional Controls
- **Charts**: ECharts + Canvas + WebGL
- **State Management**: Pinia
- **Build Tool**: Vite
- **Styling**: SCSS + CSS Variables

### Backend Stack
- **API Service**: ASP.NET Core 8.0 Web API
- **Real-time Communication**: SignalR (WebSocket)
- **Hardware Interface**: MISD Standardized Interface Layer
- **Database**: InfluxDB (Time Series) + PostgreSQL (Metadata)
- **Caching**: Redis
- **Message Queue**: RabbitMQ

### AI Integration
- **Large Language Model**: DeepSeek API for control generation
- **Natural Language Processing**: Control requirement understanding
- **Template Engine**: Dynamic control generation system

## 🚀 Quick Start

### Prerequisites

- **Node.js** 18+ and npm
- **.NET 8.0 SDK**
- **Git**

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/wukeping2008/seesharptools-web.git
   cd seesharptools-web
   ```

2. **Setup Frontend**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

3. **Setup Backend**
   ```bash
   cd backend/SeeSharpBackend
   dotnet restore
   dotnet run
   ```

4. **Access the Application**
   - Frontend: http://localhost:5176
   - Backend API: http://localhost:5001

## 📦 Professional Control Library

### 📊 Chart Controls
- **StripChart** - High-speed data streaming with 1GS/s capability
- **EasyChart** - Scientific data visualization with FFT analysis
- **DualAxisChart** - Multi-axis data display
- **ProfessionalChart** - Advanced measurement tools

### 🔬 Instrument Controls
- **Digital Oscilloscope** - 4-channel synchronized display
- **Signal Generator** - Arbitrary waveform generation
- **Digital Multimeter** - 8 measurement functions
- **Temperature Acquisition** - Multi-channel thermocouple support
- **DIO Control Card** - Digital I/O management

### 📈 Indicator Controls
- **Circular Gauge** - Analog-style meters
- **Linear Gauge** - Bar-style indicators
- **LED Indicators** - Status displays
- **Digital Displays** - Numeric readouts

### 🎛️ Control Elements
- **Professional Buttons** - Enhanced button controls
- **Knobs & Sliders** - Analog input controls
- **Switch Arrays** - Multi-state controls

## 🧠 AI Control Generator

Transform natural language descriptions into professional Vue 3 components:

```
Input: "Create a circular gauge showing temperature from 0 to 100°C"
Output: Complete Vue 3 component with TypeScript, styling, and animations
```

### Features
- **Natural Language Processing** - Understand control requirements
- **Smart Template Matching** - 6 professional control templates
- **Quality Assurance** - Ensure generated code completeness
- **Intelligent Fallback** - Local template generation when API unavailable

## 🔧 Hardware Integration

### Supported Devices
- **JY5500 PXI Modules** - High-performance PXI-based instruments
- **JYUSB1601 USB DAQ** - USB data acquisition cards
- **Mock Devices** - Simulation for development and testing

### Driver Architecture
- **Universal Driver Interface** - Standardized hardware abstraction
- **Hot-Plug Support** - Dynamic device discovery
- **Multi-Language Support** - C# DLL with future C++/Python support
- **Configuration Management** - JSON-based driver configuration

## 🎨 Visual Project Designer

Create measurement applications without coding:

1. **Drag & Drop Interface** - Visual control placement
2. **Hardware Binding** - Connect controls to real devices
3. **Real-time Preview** - Live data visualization
4. **Code Generation** - Export complete Vue 3 projects
5. **Project Management** - Save, load, and share projects

## 📊 Performance Features

### High-Speed Data Processing
- **1GS/s Data Streams** - Real-time processing capability
- **32-Channel Synchronization** - Multi-channel data handling
- **WebGL Rendering** - Hardware-accelerated graphics
- **Virtual Scrolling** - Handle TB-scale datasets
- **LTTB Compression** - Intelligent data reduction

### Optimization Technologies
- **Multi-level Caching** - L1/L2/L3 cache system
- **Web Workers** - Background processing
- **Memory Management** - Intelligent garbage collection
- **Mobile Optimization** - Touch gestures and responsive design

## 🔬 Scientific Analysis Tools

### FFT Spectrum Analysis
- **Real-time FFT** - Cooley-Tukey algorithm implementation
- **6 Window Functions** - Rectangular, Hanning, Hamming, Blackman, Kaiser, Flat-top
- **Power Spectral Density** - Multiple display modes
- **Peak Detection** - Intelligent peak identification
- **THD Analysis** - Total Harmonic Distortion calculation

### Mathematical Functions
- **Statistical Analysis** - Mean, variance, RMS, skewness, kurtosis
- **Data Fitting** - Linear and polynomial regression
- **Digital Filters** - Moving average, median, Gaussian, low/high-pass
- **Numerical Computation** - Matrix operations, differentiation, integration

### Measurement Tools
- **Cursor Measurements** - Dual cursor system for precise measurements
- **Automatic Measurements** - Comprehensive signal parameter analysis
- **Frequency Analysis** - Harmonic detection and SNR calculation

## 🚀 Deployment

### Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose up -d
```

### Cloud Deployment
- **Kubernetes** - Container orchestration
- **Monitoring** - Prometheus + Grafana
- **Logging** - ELK Stack
- **CI/CD** - Automated deployment pipelines

## 📚 Documentation

- **[Development Plan](docs/DEVELOPMENT_PLAN.md)** - Comprehensive development roadmap
- **[API Documentation](docs/API_SPECIFICATION.md)** - Complete API reference
- **[Architecture Guide](docs/ARCHITECTURE.md)** - System architecture details
- **[Deployment Guide](DEPLOYMENT_GUIDE.md)** - Production deployment instructions

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏢 About

SeeSharpTools Web is developed by [Jingyi Technology](https://www.jingyi.com.cn), a leading provider of test and measurement solutions. This project represents our commitment to bringing professional measurement capabilities to modern web platforms.

### Contact
- **Website**: [https://www.jingyi.com.cn](https://www.jingyi.com.cn)
- **Email**: support@jingyi.com.cn
- **GitHub**: [https://github.com/wukeping2008/seesharptools-web](https://github.com/wukeping2008/seesharptools-web)

## 🌟 Acknowledgments

- Vue.js team for the excellent framework
- ECharts team for powerful charting capabilities
- Microsoft for .NET Core and SignalR
- All contributors and users of this project

---

**Made with ❤️ by Jingyi Technology**
