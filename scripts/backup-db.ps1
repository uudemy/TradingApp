# ============================================================
# TradingApp DB Backup Script
# Her çalıştığında timestamp'li yeni dump oluşturur.
# 30 günden eski dump'ları siler.
# ============================================================

$ErrorActionPreference = "Stop"

$RootDir    = "F:\proje\TradingApp"
$BackupDir  = Join-Path $RootDir "backups"
$Container  = "trading-postgres"
$DbUser     = "trading"
$DbName     = "tradingdb"
$RetentionDays = 30

# 1) Klasör
if (-not (Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null
}

# 2) Timestamp
$ts       = Get-Date -Format "yyyyMMdd-HHmmss"
$fileName = "tradingdb-$ts.dump"
$hostPath = Join-Path $BackupDir $fileName
$tmpPath  = "/tmp/$fileName"

# 3) Postgres ayakta mı?
$running = docker ps --filter "name=$Container" --filter "status=running" --format "{{.Names}}"
if ($running -ne $Container) {
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] ERROR: $Container çalışmıyor." -ForegroundColor Red
    exit 1
}

# 4) Dump
Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Dump alınıyor..." -ForegroundColor Cyan
docker exec $Container pg_dump -U $DbUser -d $DbName -F c -f $tmpPath | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] ERROR: pg_dump başarısız." -ForegroundColor Red
    exit 1
}

# 5) Container → Host
docker cp "${Container}:${tmpPath}" $hostPath | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] ERROR: docker cp başarısız." -ForegroundColor Red
    exit 1
}

# 6) Geçici dosyayı sil
docker exec $Container rm $tmpPath | Out-Null

# 7) Boyut
$size = (Get-Item $hostPath).Length / 1KB
Write-Host "[$(Get-Date -Format 'HH:mm:ss')] OK: $fileName ($([math]::Round($size,1)) KB)" -ForegroundColor Green

# 8) Eski yedekleri temizle
$cutoff = (Get-Date).AddDays(-$RetentionDays)
$old = Get-ChildItem $BackupDir -Filter "tradingdb-*.dump" | Where-Object { $_.LastWriteTime -lt $cutoff }
foreach ($f in $old) {
    Remove-Item $f.FullName -Force
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Silindi (>{$RetentionDays}g): $($f.Name)" -ForegroundColor Yellow
}

# 9) Mevcut yedekleri listele
Write-Host ""
Write-Host "Mevcut yedekler:" -ForegroundColor Cyan
Get-ChildItem $BackupDir -Filter "tradingdb-*.dump" |
    Sort-Object LastWriteTime -Descending |
    Format-Table Name, @{Name="KB"; Expression={[math]::Round($_.Length/1KB,1)}}, LastWriteTime