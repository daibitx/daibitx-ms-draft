# ======================================================
# 批量推送 NuGet 包
# 作者：ChatGPT
# ======================================================

# 你的 NuGet 服务源
$source = "https://www.nfeed.site/v3/index.json"

# 你的 API Key
$apiKey = "你的ApiKey"

# 需要推送的目录（可自行添加）
$directories = @(
    "D:\项目\My\daibitx-ms-draft\tools",
    "D:\项目\My\daibitx-ms-draft\components"
)

Write-Host "==== 开始批量推送 NuGet 包 ====" -ForegroundColor Cyan

foreach ($dir in $directories) {

    if (-not (Test-Path $dir)) {
        Write-Host "目录不存在: $dir" -ForegroundColor DarkRed
        continue
    }

    Write-Host "`n>>> 扫描目录: $dir" -ForegroundColor Yellow

    # 查找所有 nupkg（排除符号包 .snupkg）
    $packages = Get-ChildItem -Path $dir -Recurse -Filter *.nupkg | Where-Object { $_.Name -notlike "*.snupkg" }

    if ($packages.Count -eq 0) {
        Write-Host "无可推送包。" -ForegroundColor DarkGray
        continue
    }

    foreach ($pkg in $packages) {
        Write-Host "推送包: $($pkg.FullName)" -ForegroundColor Green

        # 执行 push
        $result = dotnet nuget push $pkg.FullName `
            --source $source `
            --api-key $apiKey `
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
