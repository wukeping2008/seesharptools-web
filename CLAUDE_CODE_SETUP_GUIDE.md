# Claude Code Windows 配置指南

## 配置完成状态 ✅

环境变量 `CLAUDE_CODE_GIT_BASH_PATH` 已成功设置为：
```
D:\Program Files\Git\bin\bash.exe
```

## 配置步骤

### 1. 验证 Git 安装
- Git 已安装在: `D:\Program Files\Git\`
- bash.exe 路径: `D:\Program Files\Git\bin\bash.exe`

### 2. 设置环境变量
已使用以下命令设置环境变量：
```cmd
setx CLAUDE_CODE_GIT_BASH_PATH "D:\Program Files\Git\bin\bash.exe"
```

### 3. 验证配置
运行 `test_claude_code_config.bat` 来验证配置是否正确。

## 注意事项

1. **重启终端**: 设置环境变量后，需要重启终端或 VSCode 才能使新环境变量生效。
2. **系统范围**: 此设置仅对当前用户生效。
3. **验证方法**: 可以在新的终端窗口中运行 `echo %CLAUDE_CODE_GIT_BASH_PATH%` 来验证。

## 故障排除

如果 Claude Code 仍然无法找到 Git Bash：

1. 确认 Git 已正确安装
2. 检查 bash.exe 是否存在于指定路径
3. 重启 VSCode 或终端
4. 在 PowerShell 中运行: `Get-ItemProperty -Path 'HKCU:\Environment' -Name 'CLAUDE_CODE_GIT_BASH_PATH'`

## 手动配置方法

如果需要手动配置，可以通过以下方式：

### 方法一：通过系统属性
1. 右键点击"此电脑" → "属性"
2. 点击"高级系统设置"
3. 点击"环境变量"
4. 在"用户变量"中点击"新建"
5. 变量名: `CLAUDE_CODE_GIT_BASH_PATH`
6. 变量值: `D:\Program Files\Git\bin\bash.exe`

### 方法二：通过 PowerShell
```powershell
[Environment]::SetEnvironmentVariable("CLAUDE_CODE_GIT_BASH_PATH", "D:\Program Files\Git\bin\bash.exe", "User")
