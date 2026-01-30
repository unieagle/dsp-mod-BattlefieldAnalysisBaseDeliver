# 使用 dnSpy 提取游戏关键代码的脚本
# 需要先找到游戏 DLL 路径

param(
    [string]$GameDllPath = "",
    [string]$DnSpyPath = "D:\Downloads\dnSpy-net-win64\dnSpy.exe",
    [string]$OutputDir = ".\GameCodeReference"
)

Write-Host "=== 游戏代码提取工具 ===" -ForegroundColor Green
Write-Host ""

# 如果未提供 DLL 路径，尝试自动查找
if ([string]::IsNullOrEmpty($GameDllPath)) {
    Write-Host "正在查找游戏 DLL..." -ForegroundColor Yellow
    
    # 常见的 Steam 安装路径
    $steamPaths = @(
        "$env:ProgramFiles\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll",
        "$env:ProgramFiles(x86)\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll",
        "D:\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll",
        "E:\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll"
    )
    
    foreach ($path in $steamPaths) {
        if (Test-Path $path) {
            $GameDllPath = $path
            Write-Host "找到游戏 DLL: $GameDllPath" -ForegroundColor Green
            break
        }
    }
    
    if ([string]::IsNullOrEmpty($GameDllPath)) {
        Write-Host "未找到游戏 DLL，请手动指定路径" -ForegroundColor Red
        Write-Host "使用方法: .\extract_game_code.ps1 -GameDllPath '你的游戏路径\DSPGAME_Data\Managed\Assembly-CSharp.dll'" -ForegroundColor Yellow
        exit 1
    }
}

# 检查文件是否存在
if (-not (Test-Path $GameDllPath)) {
    Write-Host "错误: 找不到游戏 DLL 文件: $GameDllPath" -ForegroundColor Red
    exit 1
}

# 检查 dnSpy 是否存在
if (-not (Test-Path $DnSpyPath)) {
    Write-Host "错误: 找不到 dnSpy: $DnSpyPath" -ForegroundColor Red
    Write-Host "请修改脚本中的 DnSpyPath 变量" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "游戏 DLL: $GameDllPath" -ForegroundColor Cyan
Write-Host "dnSpy 路径: $DnSpyPath" -ForegroundColor Cyan
Write-Host "输出目录: $OutputDir" -ForegroundColor Cyan
Write-Host ""

# 创建输出目录
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
    Write-Host "已创建输出目录: $OutputDir" -ForegroundColor Green
}

# 使用 ILSpy 命令行工具（如果可用）或者提供手动指导
Write-Host "=== 提取方法 ===" -ForegroundColor Green
Write-Host ""
Write-Host "由于 dnSpy.console.exe 可能有编码问题，建议使用以下方法：" -ForegroundColor Yellow
Write-Host ""
Write-Host "方法 1: 使用 dnSpy 图形界面（推荐）" -ForegroundColor Cyan
Write-Host "  1. 运行: $DnSpyPath" -ForegroundColor White
Write-Host "  2. File -> Open -> 选择: $GameDllPath" -ForegroundColor White
Write-Host "  3. 搜索 'StationComponent' 类" -ForegroundColor White
Write-Host "  4. 右键类 -> Export to Project -> 保存到: $OutputDir\StationComponent.cs" -ForegroundColor White
Write-Host ""
Write-Host "方法 2: 使用 ILSpy 命令行（如果已安装）" -ForegroundColor Cyan
Write-Host "  ilspycmd $GameDllPath -o $OutputDir" -ForegroundColor White
Write-Host ""

# 尝试使用 ILSpy 命令行（如果可用）
$ilspycmd = Get-Command "ilspycmd" -ErrorAction SilentlyContinue
if ($ilspycmd) {
    Write-Host "检测到 ILSpy 命令行工具，正在导出..." -ForegroundColor Green
    & ilspycmd $GameDllPath -o $OutputDir
    Write-Host "导出完成！" -ForegroundColor Green
} else {
    Write-Host "未检测到 ILSpy 命令行工具" -ForegroundColor Yellow
    Write-Host "建议下载 ILSpy: https://github.com/icsharpcode/ILSpy/releases" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== 需要查找的关键信息 ===" -ForegroundColor Green
Write-Host ""
Write-Host "1. StationComponent 类的位置和完整定义" -ForegroundColor Cyan
Write-Host "2. FindItemSource 方法的签名:" -ForegroundColor Cyan
Write-Host "   - 参数类型和名称" -ForegroundColor White
Write-Host "   - 返回值类型" -ForegroundColor White
Write-Host "3. CanPickupItem 方法的签名" -ForegroundColor Cyan
Write-Host "4. 战场分析基站的建筑 ID (protoId)" -ForegroundColor Cyan
Write-Host "5. 物品存储相关的属性 (package, GetItemCount 等)" -ForegroundColor Cyan
Write-Host ""

# 创建一个信息记录模板
$infoTemplate = @"
=== 关键信息记录 ===

1. StationComponent.FindItemSource 方法签名：
   方法名: 
   参数: 
   返回值: 
   可见性: 

2. StationComponent.CanPickupItem 方法签名：
   方法名: 
   参数: 
   返回值: 
   可见性: 

3. 战场分析基站信息：
   建筑 ID (protoId): 
   相关类名: 
   物品存储属性名: 

4. 物品存储相关：
   package 属性的类型: 
   GetItemCount 方法的签名: 

5. 其他重要信息：
   
"@

$infoFile = Join-Path $OutputDir "关键信息记录.txt"
$infoTemplate | Out-File -FilePath $infoFile -Encoding UTF8
Write-Host "已创建信息记录模板: $infoFile" -ForegroundColor Green
Write-Host ""

Write-Host "完成！请按照上述方法提取代码，并填写信息记录文件。" -ForegroundColor Green
