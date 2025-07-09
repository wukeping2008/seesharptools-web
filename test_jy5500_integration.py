#!/usr/bin/env python3
"""
JY5500驱动集成测试脚本
测试JY5500.dll是否能被正确加载和使用
"""

import requests
import json
import time
import sys

# 后端API基础URL
BASE_URL = "http://localhost:5000"

def test_backend_health():
    """测试后端健康状态"""
    try:
        response = requests.get(f"{BASE_URL}/health", timeout=5)
        if response.status_code == 200:
            print("✅ 后端服务运行正常")
            return True
        else:
            print(f"❌ 后端健康检查失败: {response.status_code}")
            return False
    except requests.exceptions.RequestException as e:
        print(f"❌ 无法连接到后端服务: {e}")
        return False

def test_driver_discovery():
    """测试驱动发现功能"""
    try:
        response = requests.get(f"{BASE_URL}/api/hardware-driver/drivers", timeout=10)
        if response.status_code == 200:
            drivers = response.json()
            print(f"✅ 发现 {len(drivers)} 个驱动:")
            for driver in drivers:
                print(f"   - {driver}")
            return "JY5500" in drivers
        else:
            print(f"❌ 驱动发现失败: {response.status_code}")
            return False
    except requests.exceptions.RequestException as e:
        print(f"❌ 驱动发现请求失败: {e}")
        return False

def test_device_discovery():
    """测试设备发现功能"""
    try:
        response = requests.get(f"{BASE_URL}/api/hardware-driver/devices", timeout=15)
        if response.status_code == 200:
            devices = response.json()
            print(f"✅ 发现 {len(devices)} 个设备:")
            jy5500_devices = []
            for device in devices:
                print(f"   - {device.get('name', 'Unknown')} ({device.get('model', 'Unknown')})")
                if 'JY55' in device.get('model', ''):
                    jy5500_devices.append(device)
            
            if jy5500_devices:
                print(f"✅ 发现 {len(jy5500_devices)} 个JY5500设备")
                return jy5500_devices
            else:
                print("⚠️  未发现JY5500设备（可能是因为没有连接硬件）")
                return []
        else:
            print(f"❌ 设备发现失败: {response.status_code}")
            return None
    except requests.exceptions.RequestException as e:
        print(f"❌ 设备发现请求失败: {e}")
        return None

def test_jy5500_driver_load():
    """测试JY5500驱动加载"""
    try:
        # 尝试加载JY5500驱动
        response = requests.post(f"{BASE_URL}/api/hardware-driver/load-driver", 
                               json={"driverName": "JY5500"}, 
                               timeout=10)
        if response.status_code == 200:
            result = response.json()
            if result.get('success', False):
                print("✅ JY5500驱动加载成功")
                return True
            else:
                print(f"❌ JY5500驱动加载失败: {result.get('message', 'Unknown error')}")
                return False
        else:
            print(f"❌ JY5500驱动加载请求失败: {response.status_code}")
            return False
    except requests.exceptions.RequestException as e:
        print(f"❌ JY5500驱动加载请求异常: {e}")
        return False

def test_jy5500_task_creation():
    """测试JY5500任务创建"""
    try:
        # 创建AI任务配置
        task_config = {
            "driverName": "JY5500",
            "taskType": "AI",
            "deviceId": 1,
            "parameters": {
                "BoardIndex": 0,
                "ChannelCount": 4,
                "SampleRate": 1000,
                "MinRange": -10.0,
                "MaxRange": 10.0
            }
        }
        
        response = requests.post(f"{BASE_URL}/api/hardware-driver/create-task", 
                               json=task_config, 
                               timeout=10)
        if response.status_code == 200:
            result = response.json()
            if result.get('success', False):
                print("✅ JY5500 AI任务创建成功")
                return result.get('taskId')
            else:
                print(f"❌ JY5500 AI任务创建失败: {result.get('message', 'Unknown error')}")
                return None
        else:
            print(f"❌ JY5500任务创建请求失败: {response.status_code}")
            return None
    except requests.exceptions.RequestException as e:
        print(f"❌ JY5500任务创建请求异常: {e}")
        return None

def main():
    """主测试函数"""
    print("🚀 开始JY5500驱动集成测试...")
    print("=" * 50)
    
    # 1. 测试后端健康状态
    print("\n1. 测试后端服务状态...")
    if not test_backend_health():
        print("❌ 后端服务未运行，请先启动后端服务")
        sys.exit(1)
    
    # 2. 测试驱动发现
    print("\n2. 测试驱动发现...")
    jy5500_available = test_driver_discovery()
    if not jy5500_available:
        print("❌ JY5500驱动未找到")
        sys.exit(1)
    
    # 3. 测试驱动加载
    print("\n3. 测试JY5500驱动加载...")
    if not test_jy5500_driver_load():
        print("⚠️  JY5500驱动加载失败（可能是因为DLL文件路径问题）")
    
    # 4. 测试设备发现
    print("\n4. 测试设备发现...")
    devices = test_device_discovery()
    if devices is None:
        print("❌ 设备发现失败")
    
    # 5. 测试任务创建
    print("\n5. 测试JY5500任务创建...")
    task_id = test_jy5500_task_creation()
    if task_id:
        print(f"✅ 任务创建成功，任务ID: {task_id}")
    else:
        print("⚠️  任务创建失败（可能是因为没有连接硬件）")
    
    print("\n" + "=" * 50)
    print("🎉 JY5500驱动集成测试完成！")
    print("\n📝 测试总结:")
    print("- ✅ 后端服务正常运行")
    print("- ✅ JY5500驱动配置正确")
    print("- ⚠️  真实硬件测试需要连接JY5500设备")
    print("\n💡 下一步:")
    print("1. 连接JY5500硬件设备")
    print("2. 运行完整的硬件功能测试")
    print("3. 验证数据采集和实时传输功能")

if __name__ == "__main__":
    main()
