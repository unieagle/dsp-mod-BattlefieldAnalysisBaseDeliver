# 将 manifest.json、README.md、CHANGELOG.md、icon.png 和 Release 构建的 DLL 打包成 zip（Thunderstore 需根目录 CHANGELOG.md 才显示更新说明）
# 用法: .\pack-mod.ps1  或  .\pack-mod.ps1 -OutputDir ".\dist"

param(
    [string]$OutputDir = "."
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

$dllName = "BattlefieldAnalysisBaseDeliver.dll"
$manifestPath = Join-Path $scriptDir "manifest.json"
$readmePath = Join-Path $scriptDir "README.md"
$changelogPath = Join-Path $scriptDir "CHANGELOG.md"
$iconPath = Join-Path $scriptDir "icon.png"
$binRelease = Join-Path $scriptDir "bin\Release"

# 先执行 Release 构建，确保打包使用最新 DLL
Write-Host "正在执行 Release 构建: dotnet build -c Release" -ForegroundColor Cyan
& dotnet build -c Release -p:SkipPostBuild=true --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Error "Release 构建失败，请检查项目后重试"
    exit 1
}

# 查找 DLL（支持 bin\Release\net472\ 等任意子目录）
$dllPath = Get-ChildItem -Path $binRelease -Filter $dllName -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

if (-not $dllPath) {
    Write-Error "未找到 $dllName（构建后仍未找到，请检查 csproj 输出路径）"
    exit 1
}

if (-not (Test-Path $manifestPath)) {
    Write-Error "未找到 manifest.json"
    exit 1
}

if (-not (Test-Path $readmePath)) {
    Write-Error "未找到 README.md"
    exit 1
}

# 从 manifest 读取版本号作为 zip 名
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$version = $manifest.version_number
$zipName = "BattlefieldAnalysisBaseDeliver-$version.zip"
$zipPath = Join-Path $OutputDir $zipName

# 确保输出目录存在
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# 创建临时目录并复制文件（zip 根目录即为 mod 根目录）
$tempDir = Join-Path $env:TEMP "BattlefieldAnalysisBaseDeliver-pack-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

try {
    Copy-Item $manifestPath (Join-Path $tempDir "manifest.json") -Force
    Copy-Item $readmePath (Join-Path $tempDir "README.md") -Force
    if (Test-Path $changelogPath) {
        Copy-Item $changelogPath (Join-Path $tempDir "CHANGELOG.md") -Force
    }
    if (Test-Path $iconPath) {
        Copy-Item $iconPath (Join-Path $tempDir "icon.png") -Force
    }
    Copy-Item $dllPath.FullName (Join-Path $tempDir $dllName) -Force

    # 删除已存在的同名 zip
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    Compress-Archive -Path "$tempDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "已生成: $zipPath" -ForegroundColor Green
}
finally {
    Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}
