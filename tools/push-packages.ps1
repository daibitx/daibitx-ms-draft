# ======================================================
# 批量推送 NuGet 包
# 作者：daibitx
# ======================================================

param(
    [Parameter(Mandatory = $false)]
    [string]$ApiKey = "ApiKey",

    [Parameter(Mandatory = $false)]
    [string]$Source = "https://www.nfeed.site/v3/index.json",

    [Parameter(Mandatory = $false)]
    [string[]]$Directories = @(
        "D:\项目\My\daibitx-ms-draft\tools",
        "D:\项目\My\daibitx-ms-draft\components"
    )
)

Write-Host "==== 开始批量推送 NuGet 包 ====" -ForegroundColor Cyan

foreach ($dir in $Directories) {

    if (-not (Test-Path $dir)) {
        Write-Host "目录不存在: $dir" -ForegroundColor DarkRed
        continue
    }

    Write-Host "`n>>> 扫描目录: $dir" -ForegroundColor Yellow

    $packages = Get-ChildItem -Path $dir -Recurse -Filter *.nupkg |
                Where-Object { $_.Name -notlike "*.snupkg" }

    if ($packages.Count -eq 0) {
        Write-Host "无可推送包。" -ForegroundColor DarkGray
        continue
    }

    foreach ($pkg in $packages) {
        Write-Host "推送包: $($pkg.FullName)" -ForegroundColor Green

        $result = dotnet nuget push $pkg.FullName `
            --source $Source `
            --api-key $ApiKey `
            --skip-duplicate `
            2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✔ 推送成功: $($pkg.Name)" -ForegroundColor Cyan
        } else {
            Write-Host "✘ 推送失败: $($pkg.Name)" -ForegroundColor Red
            Write-Host $result -ForegroundColor DarkRed
        }
    }
}

Write-Host "`n==== 所有包推送完成 ====" -ForegroundColor Cyan
