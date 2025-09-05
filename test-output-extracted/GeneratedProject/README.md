# Generated JYUSB-1601 Project

## Prerequisites

1. **JYUSB-1601 Driver**: Install the JYUSB-1601 driver package from JYTEK
2. **JYUSB1601.dll**: Locate the DLL file (usually in `C:\Program Files\JYTEK\JYUSB1601\lib\`)
3. **.NET 8.0 SDK**: Required to build and run the project

## Setup Instructions

### Step 1: Configure JYUSB1601.dll Reference

Edit `GeneratedProject.csproj` and uncomment the appropriate section:

```xml
<!-- If JYUSB1601.dll is installed in the default location -->
<ItemGroup>
  <Reference Include="JYUSB1601">
    <HintPath>C:\Program Files\JYTEK\JYUSB1601\lib\JYUSB1601.dll</HintPath>
  </Reference>
</ItemGroup>
```

OR

```xml
<!-- If you have JYUSB1601.dll in a local lib folder -->
<ItemGroup>
  <Reference Include="JYUSB1601">
    <HintPath>lib\JYUSB1601.dll</HintPath>
  </Reference>
</ItemGroup>
```

### Step 2: Build the Project

```bash
dotnet build
```

### Step 3: Run the Application

```bash
dotnet run
```

## Common Issues

### "JYUSB1601 not found"
- Ensure the JYUSB-1601 driver is installed
- Verify the DLL path in the .csproj file
- Copy JYUSB1601.dll to the project's `lib` folder

### "Device not found"
- Check the USB connection
- Verify the board number (usually "0")
- Ensure no other application is using the device

## USB-1601 Modes

- **Single Mode**: Single point acquisition
- **Continuous Mode**: Real-time continuous acquisition
- **Finite Mode**: Fixed number of samples

## Support

For JYTEK USB-1601 support, visit the JYTEK website or refer to the USB-1601 documentation.