# 提取戴森球计划游戏代码关键信息
# 使用 .NET 反射读取 DLL 元数据

$gameDllPath = "C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll"
$outputDir = ".\GameCodeReference"

Write-Host "=== 提取游戏代码信息 ===" -ForegroundColor Green
Write-Host "游戏 DLL: $gameDllPath" -ForegroundColor Cyan
Write-Host ""

# 检查文件是否存在
if (-not (Test-Path $gameDllPath)) {
    Write-Host "错误: 找不到游戏 DLL 文件" -ForegroundColor Red
    exit 1
}

# 创建输出目录
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

Write-Host "正在加载程序集..." -ForegroundColor Yellow

try {
    # 加载程序集
    Add-Type -Path $gameDllPath
    
    # 获取程序集
    $assembly = [System.Reflection.Assembly]::LoadFrom($gameDllPath)
    Write-Host "程序集加载成功！" -ForegroundColor Green
    Write-Host ""
    
    # 查找 StationComponent 类
    Write-Host "正在查找 StationComponent 类..." -ForegroundColor Yellow
    $stationComponentType = $assembly.GetTypes() | Where-Object { $_.Name -eq "StationComponent" } | Select-Object -First 1
    
    if ($stationComponentType) {
        Write-Host "找到 StationComponent 类！" -ForegroundColor Green
        Write-Host "命名空间: $($stationComponentType.Namespace)" -ForegroundColor Cyan
        Write-Host "完整名称: $($stationComponentType.FullName)" -ForegroundColor Cyan
        Write-Host ""
        
        # 查找 FindItemSource 方法
        Write-Host "正在查找 FindItemSource 方法..." -ForegroundColor Yellow
        $findItemSourceMethods = $stationComponentType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | 
            Where-Object { $_.Name -like "*FindItemSource*" -or $_.Name -like "*Find*Source*" }
        
        if ($findItemSourceMethods) {
            Write-Host "找到相关方法:" -ForegroundColor Green
            foreach ($method in $findItemSourceMethods) {
                $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                Write-Host "  - $($method.Name)($params) : $($method.ReturnType.Name)" -ForegroundColor White
            }
            Write-Host ""
        } else {
            Write-Host "未找到 FindItemSource 方法，尝试查找其他相关方法..." -ForegroundColor Yellow
            $allMethods = $stationComponentType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | 
                Where-Object { $_.Name -like "*Item*" -or $_.Name -like "*Source*" -or $_.Name -like "*Pickup*" }
            if ($allMethods) {
                Write-Host "找到相关方法:" -ForegroundColor Green
                foreach ($method in $allMethods | Select-Object -First 20) {
                    $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                    Write-Host "  - $($method.Name)($params) : $($method.ReturnType.Name)" -ForegroundColor White
                }
            }
            Write-Host ""
        }
        
        # 查找 CanPickupItem 方法
        Write-Host "正在查找 CanPickupItem 方法..." -ForegroundColor Yellow
        $canPickupMethods = $stationComponentType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | 
            Where-Object { $_.Name -like "*CanPickup*" -or $_.Name -like "*Pickup*" }
        
        if ($canPickupMethods) {
            Write-Host "找到相关方法:" -ForegroundColor Green
            foreach ($method in $canPickupMethods) {
                $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                Write-Host "  - $($method.Name)($params) : $($method.ReturnType.Name)" -ForegroundColor White
            }
            Write-Host ""
        }
        
        # 查找属性
        Write-Host "正在查找属性..." -ForegroundColor Yellow
        $properties = $stationComponentType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
        $relevantProperties = $properties | Where-Object { 
            $_.Name -like "*package*" -or 
            $_.Name -like "*Package*" -or 
            $_.Name -like "*protoId*" -or 
            $_.Name -like "*ProtoId*" -or
            $_.Name -like "*entityId*" -or
            $_.Name -like "*EntityId*" -or
            $_.Name -like "*pos*" -or
            $_.Name -like "*Pos*" -or
            $_.Name -like "*position*" -or
            $_.Name -like "*Position*"
        }
        
        if ($relevantProperties) {
            Write-Host "找到相关属性:" -ForegroundColor Green
            foreach ($prop in $relevantProperties) {
                Write-Host "  - $($prop.Name) : $($prop.PropertyType.Name)" -ForegroundColor White
            }
            Write-Host ""
        }
        
        # 保存信息到文件
        $infoFile = Join-Path $outputDir "StationComponent_信息.txt"
        $info = @"
=== StationComponent 类信息 ===

命名空间: $($stationComponentType.Namespace)
完整名称: $($stationComponentType.FullName)

=== 方法 ===
"@
        
        $allMethods = $stationComponentType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
        foreach ($method in $allMethods | Where-Object { $_.Name -like "*Item*" -or $_.Name -like "*Source*" -or $_.Name -like "*Pickup*" -or $_.Name -like "*Find*" }) {
            $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
            $info += "`n$($method.Name)($params) : $($method.ReturnType.Name)"
        }
        
        $info += "`n`n=== 属性 ==="
        foreach ($prop in $relevantProperties) {
            $info += "`n$($prop.Name) : $($prop.PropertyType.Name)"
        }
        
        $info | Out-File -FilePath $infoFile -Encoding UTF8
        Write-Host "信息已保存到: $infoFile" -ForegroundColor Green
        
    } else {
        Write-Host "未找到 StationComponent 类" -ForegroundColor Red
        Write-Host "尝试查找包含 'Station' 的类..." -ForegroundColor Yellow
        
        $stationClasses = $assembly.GetTypes() | Where-Object { $_.Name -like "*Station*" } | Select-Object -First 10
        if ($stationClasses) {
            Write-Host "找到相关类:" -ForegroundColor Green
            foreach ($type in $stationClasses) {
                Write-Host "  - $($type.FullName)" -ForegroundColor White
            }
        }
    }
    
    # 查找战场分析基站相关
    Write-Host ""
    Write-Host "正在查找战场分析基站相关类..." -ForegroundColor Yellow
    $battlefieldClasses = $assembly.GetTypes() | Where-Object { 
        $_.Name -like "*Battlefield*" -or 
        $_.Name -like "*Battle*" -or 
        $_.Name -like "*Analysis*" 
    } | Select-Object -First 10
    
    if ($battlefieldClasses) {
        Write-Host "找到相关类:" -ForegroundColor Green
        foreach ($type in $battlefieldClasses) {
            Write-Host "  - $($type.FullName)" -ForegroundColor White
        }
    } else {
        Write-Host "未找到战场分析基站相关类" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "=== 提取完成 ===" -ForegroundColor Green
    Write-Host "详细信息已保存到: $outputDir" -ForegroundColor Cyan
    
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "如果遇到加载错误，请使用 dnSpy 图形界面手动提取代码" -ForegroundColor Yellow
    Write-Host "参考: 使用dnSpy提取代码.md" -ForegroundColor Yellow
}
