# SeeSharpTools Web

🚀 **World's First Web-based Professional Test & Measurement Hardware Driver Management Platform**

SeeSharpTools Web is a revolutionary web platform designed for JYTek's professional test and measurement hardware devices, providing complete web-based driver management and control interfaces.

## 🎯 Project Overview

This project completely web-enables traditional desktop test and measurement software, achieving:
- **Universal Hardware Driver Management**: Supports all JYTek .dll hardware drivers
- **Real-time Data Visualization**: Professional-grade charts and dashboard components
- **Modern User Interface**: Responsive design based on Vue 3 + TypeScript
- **High-performance Backend Services**: ASP.NET Core 9.0 + SignalR real-time communication

## 🏗️ Project Architecture

```
SeeSharpTools-Web/
├── frontend/           # Vue 3 + TypeScript frontend application
├── backend/           # ASP.NET Core 9.0 backend services
├── docs/             # Project documentation
└── examples/         # Examples and demonstrations
```

## ✨ Core Features

### 🔧 Backend Services (ASP.NET Core 9.0)
- **MISD Standardized Interface Layer**: Unified device management interface
- **Universal Driver Management Architecture**: Supports runtime hot-swappable drivers
- **SignalR Real-time Communication**: High-performance data streaming
- **RESTful API + Swagger**: Complete API documentation

### 🎨 Frontend Application (Vue 3 + TypeScript)
- **Professional Test & Measurement Component Library**: Dashboards, charts, controllers
- **Real-time Data Visualization**: WebGL-accelerated high-performance charts
- **Responsive Design**: Supports desktop and mobile devices
- **AI-assisted Development**: Intelligent control generator

### 📊 Professional Components
- **Chart Components**: EasyChart, StripChart, SpectrumChart
- **Gauge Components**: CircularGauge, LinearGauge, Thermometer
- **Indicators**: LED indicators, digital displays
- **Controllers**: Switches, button arrays, sliders

## 🚀 Quick Start

### Frontend Development
```bash
cd frontend
npm install --registry https://registry.npmmirror.com
npm run dev
```

### Backend Development
```bash
cd backend/SeeSharpBackend
dotnet restore
dotnet run
```

## 📋 Development Roadmap

For detailed development plans, see: [DEVELOPMENT_PLAN.md](docs/DEVELOPMENT_PLAN.md)

## 🏆 Technical Innovations

- **World's First**: Web-based professional test & measurement hardware driver management
- **Standard Setting**: Establishing technical standards for T&M industry web transformation
- **Architectural Breakthrough**: Universal driver adapter design pattern
- **Performance Optimization**: WebGL + SignalR high-performance data streaming

## 🎯 Key Achievements

### ✅ **Completed Features**

#### **High-Performance Chart Controls**
- **StripChart**: Real-time data streaming display supporting 1GS/s data rates
- **Enhanced EasyChart**: Integrated FFT spectrum analysis with professional measurement tools
- **SpectrumChart**: Real-time frequency domain analysis with multiple window functions
- **Advanced Mathematical Analysis**: 15 statistical indicators, polynomial fitting, digital filters

#### **Professional Instrument Controls**
- **Signal Generator**: Arbitrary waveform generation with modulation capabilities
- **Digital Oscilloscope**: Multi-channel waveform analysis with complete trigger system
- **Data Acquisition Card**: 4-channel high-speed data acquisition (1kS/s - 2MS/s)
- **Temperature Acquisition Card**: 8 thermocouple types support with high-precision measurement
- **DIO Control Card**: Professional digital I/O control with comprehensive testing functions
- **Digital Multimeter**: 8 measurement functions with statistical analysis

#### **AI-Powered Control Generation System**
- **Natural Language Processing**: Generate controls from Chinese descriptions
- **Intelligent Code Generation**: Complete Vue 3 components with TypeScript support
- **Professional Quality Assurance**: Industry-standard code generation
- **Smart Fallback System**: Local simulation when API unavailable

#### **Multi-Hardware Platform Driver Integration** 🔌 **MAJOR BREAKTHROUGH**
- **JY5500 Series (PXI)**: Complete support for JY5510/5511/5515/5516 PXI data acquisition cards
  - Sampling rates up to 2MS/s, up to 32 channels
  - AI/AO/DI/DO/CI/CO full functionality support
  - Real-time device discovery and task management
- **JYUSB1601 Series (USB)**: Complete support for JYUSB1601 USB high-speed data acquisition cards
  - Sampling rates up to 1MS/s, 16 AI channels + 2 AO channels
  - 16-bit ADC/DAC resolution, ±10V/±5V/±2V/±1V voltage ranges
  - USB device auto-discovery (device index 0-7)
- **MockDriver**: Complete simulation driver for development and testing
  - Full functionality simulation without real hardware
  - Perfect for development, testing, and demonstration

#### **Universal Driver Management Architecture** 🏗️ **MAJOR BREAKTHROUGH**
- **Unified Interface Design**: All drivers implement the same IDriverAdapter interface
- **Hot-Swappable Support**: Runtime driver loading/unloading without service restart
- **Configuration Management**: JSON-based unified driver parameter management
- **Multi-Language Support**: Ready for C# DLL, Python, and C++ driver extensions

#### **Backend MISD Interface Layer**
- **Universal Driver Architecture**: Support for C# DLL, Python, C++ drivers
- **Device Discovery System**: Automatic detection of PXI, USB, PCIe devices
- **Real-time Data Processing**: SignalR-based high-performance data streaming
- **Complete API Documentation**: RESTful API with Swagger integration
- **High-Performance Data Storage**: 1GS/s write speed with intelligent compression
- **Multi-threaded Data Acquisition Engine**: Concurrent multi-device data acquisition

### 🚀 **Technical Highlights**

#### **World-Leading Performance**
- Supports 1GS/s data streaming in web browsers
- 32-channel synchronous display capability
- WebGL hardware acceleration with stable 60fps rendering
- TB-scale data management with intelligent compression

#### **Complete Scientific Analysis Tools**
- Real-time FFT spectrum analysis with Cooley-Tukey algorithm
- 6 professional window functions (Hanning, Hamming, Blackman, Kaiser, etc.)
- Advanced mathematical analysis with 15 statistical indicators
- Professional measurement tools with sub-pixel accuracy

#### **AI-Driven Development**
- Natural language to code generation in seconds
- Intelligent control templates with professional styling
- Complete development workflow automation
- Quality assurance with automatic code validation

## 📖 Documentation

- [Development Plan](docs/DEVELOPMENT_PLAN.md)
- [Project Reorganization Plan](docs/PROJECT_REORGANIZATION_PLAN.md)
- [API Documentation](http://localhost:5152/swagger) (Available when backend is running)

## 🤝 Contributing

Welcome to submit Issues and Pull Requests to help improve the project.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🌟 Milestones

- ✅ **Phase 1**: Frontend professional component library completed
- ✅ **Phase 2**: Backend MISD standardized interface layer completed
- ✅ **Phase 3**: Frontend-backend integration and testing completed
- ✅ **Phase 4**: Multi-hardware platform driver integration completed
- ✅ **Phase 5**: Real hardware driver integration testing completed
- 🔄 **Phase 6**: Performance optimization and production deployment (In Progress)

## 🔌 **Supported Hardware Platforms**

### **JY5500 Series (PXI Data Acquisition Cards)**
- **Models**: JY5510, JY5511, JY5515, JY5516
- **Sampling Rate**: Up to 2MS/s
- **Channels**: Up to 32 channels
- **Functions**: AI/AO/DI/DO/CI/CO
- **Resolution**: 16-bit ADC/DAC
- **Voltage Ranges**: ±10V, ±5V, ±2V, ±1V, ±0.5V, ±0.2V, ±0.1V
- **Applications**: High-end test systems, laboratory automation

### **JYUSB1601 Series (USB Data Acquisition Cards)**
- **Models**: JYUSB1601
- **Sampling Rate**: Up to 1MS/s
- **Channels**: 16 AI channels + 2 AO channels
- **Functions**: AI/AO/DI/DO
- **Resolution**: 16-bit ADC/DAC
- **Voltage Ranges**: ±10V, ±5V, ±2V, ±1V
- **USB Timeout**: 5000ms
- **Applications**: Field testing, portable data acquisition

### **MockDriver (Simulation Driver)**
- **Purpose**: Development and testing without real hardware
- **Features**: Complete functionality simulation
- **Channels**: Configurable multi-channel support
- **Data Generation**: Multiple waveform types with noise simulation
- **Applications**: Development, testing, demonstration

## 🏗️ **Driver Architecture Features**

### **Unified Interface Design**
- All drivers implement the same `IDriverAdapter` interface
- Consistent API across different hardware types
- Seamless switching between hardware platforms
- Complete backward compatibility guarantee

### **Hot-Swappable Driver System**
- Runtime driver loading/unloading without service restart
- Real-time driver status monitoring and management
- Configuration file hot-reload support
- Complete resource management and cleanup

### **Configuration Management**
- JSON-based unified driver parameter management
- Driver type, path, and parameter unified configuration
- Runtime configuration validation and error handling
- Configuration version management and migration support

### **Multi-Language Driver Support**
- **C# DLL**: Complete support for JYTek .dll drivers
- **Python**: Ready for Python driver integration
- **C++**: Prepared for C++ driver extensions
- **Cross-Language**: Bridge architecture for multi-language calls

## 💡 Technical Innovation Points

### 1. World's First Web-based Professional T&M Platform
- Complete web transformation of traditional desktop T&M software
- Professional-grade functionality comparable to desktop applications
- Modern web interface design with scientific instrument aesthetics

### 2. Ultra-High Performance Data Visualization
- Supports high-speed data streaming in web browsers
- Innovative data compression and rendering algorithms
- WebGL-based high-performance graphics rendering

### 3. Universal Driver Management Architecture
- Unified interface for all JYTek hardware drivers
- Runtime hot-swappable driver system
- Multi-language driver support (C#, Python, C++)

### 4. AI-Powered Development Tools
- Natural language to professional control code generation
- Intelligent template system with quality assurance
- Complete development workflow automation

## 🔧 Technology Stack

### Frontend
- **Framework**: Vue 3 + TypeScript + Composition API
- **UI Library**: Element Plus + Custom professional controls
- **Charts**: ECharts + Canvas + WebGL
- **State Management**: Pinia
- **Build Tool**: Vite
- **Styling**: SCSS + CSS Variables

### Backend
- **API Service**: ASP.NET Core 9.0 Web API
- **Real-time Communication**: SignalR (WebSocket)
- **Hardware Interface**: JYTek MISD standardized interface layer
- **Database**: InfluxDB (Time-series) + PostgreSQL (Metadata)
- **Caching**: Redis
- **Message Queue**: RabbitMQ

### AI Integration
- **Large Language Model**: Claude API
- **Natural Language Processing**: Control requirement understanding and code generation
- **Template Engine**: Dynamic control generation system

## 🎨 Design Philosophy

### Professional Aesthetics
- **Clean White Background**: Following scientific chart standards
- **Professional Grid System**: Precise measurement grids
- **Scientific Color Schemes**: Optimized for data visualization
- **Instrument-grade Styling**: Professional test equipment appearance

### Performance Optimization
- **Big Data Processing**: LTTB sampling for smooth performance
- **Real-time Updates**: Efficient data streaming
- **Memory Management**: Intelligent buffer management
- **Responsive Design**: Adapts to all screen sizes

## 🌐 Live Demo

Visit the live demo at: [https://seesharptools-web.vercel.app](https://seesharptools-web.vercel.app)

## 📞 Support

For questions, issues, or feature requests:

- Create an issue on GitHub
- Check the documentation
- Review example implementations

## 🔗 Related Links

- [Vue 3 Documentation](https://vuejs.org/)
- [TypeScript Documentation](https://www.typescriptlang.org/)
- [Element Plus Documentation](https://element-plus.org/)
- [ECharts Documentation](https://echarts.apache.org/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)

---

**SeeSharpTools Web** - Making professional test & measurement device web transformation a reality!

**Built with ❤️ for the Test & Measurement Community**
