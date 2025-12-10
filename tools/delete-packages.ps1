# ======================================================
# 批量删除 NuGet 包
# 作者：daibitx
# ======================================================

param(
    [Parameter(Mandatory = $false)]
    [string]$Source = "https://www.nfeed.site/v3/index.json",
    
    [Parameter(Mandatory = $false)]
    [string]$ApiKey,

    [Parameter(Mandatory = $false)]
    [string[]]$Directories = @(
        "D:\项目\My\daibitx-ms-draft\tools",
        "D:\项目\My\daibitx-ms-draft\components"
    )
)

# 设置编码
[Console]::InputEncoding = [Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 获取 API Key（参数 > 环境变量 > 默认值）
if (-not $ApiKey) {
    $ApiKey = if ($env:NUGET_API_KEY) { $env:NUGET_API_KEY } else { "你的APIKey" }
}

# 安全确认
Write-Host "⚠️  警告：即将永久删除以下 NuGet 包！" -ForegroundColor Red
Write-Host "源服务器: $Source" -ForegroundColor Yellow
Write-Host "目标目录: $($Directories -join ', ')" -ForegroundColor Yellow
Write-Host "`n[按 Enter 继续，Ctrl+C 取消]" -ForegroundColor Gray
Read-Host | Out-Null

Write-Host "`n==== 开始批量删除 NuGet 包 ====" -ForegroundColor Cyan

$stats = @{ Found = 0; Deleted = 0; Failed = 0 }

foreach ($dir in $Directories) {
    if (-not (Test-Path $dir)) {
        Write-Host "`n✘ 目录不存在: $dir" -ForegroundColor DarkRed
        continue
    }

    Write-Host "`n>>> 扫描目录: $dir" -ForegroundColor Yellow
    
    $packages = Get-ChildItem -Path $dir -Recurse -Filter *.nupkg | 
                Where-Object { $_.Name -notlike "*.snupkg" }

    if ($packages.Count -eq 0) {
        Write-Host "   未找到包文件" -ForegroundColor Gray
        continue
    }

    foreach ($pkg in $packages) {
        $stats.Found++
        
        # 解析包ID和版本（修复核心bug）
        $fileName = [System.IO.Path]::GetFileNameWithoutExtension($pkg.Name)
        
        # 使用正则匹配 SemVer 2.0 版本号
        $regex = '^(?<id>.+?)\.(?<version>\d+\.\d+\.\d+(?:-[a-zA-Z0-9\-\.]+)?(?:\+[a-zA-Z0-9\-\.]+)?)$'
        $match = [regex]::Match($fileName, $regex)
        
        if (-not $match.Success) {
            Write-Host "   ✘ 无法解析: $($pkg.Name)" -ForegroundColor Red
            $stats.Failed++
            continue
        }
        
        $packageId = $match.Groups['id'].Value
        $version = $match.Groups['version'].Value
        
        Write-Host "   [$($stats.Found)] 删除: $packageId v$version" -ForegroundColor Green
        
        # 执行删除（使用正确的双横线参数）
        $result = dotnet nuget delete $packageId $version `
            --source $Source `
            --api-key $ApiKey `
            --non-interactive `
            2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "         ✔ 成功" -ForegroundColor Cyan
            $stats.Deleted++
        } else {
            Write-Host "         ✘ 失败: $result" -ForegroundColor Red
            $stats.Failed++
        }
    }
}

Write-Host "`n==== 删除完成统计 ====" -ForegroundColor Cyan
Write-Host "找到: $($stats.Found) | 成功: $($stats.Deleted) | 失败: $($stats.Failed)" -ForegroundColor White