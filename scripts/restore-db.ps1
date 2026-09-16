# ============================================================
# Kullanım:  .\restore-db.ps1 -BackupFile backups\tradingdb-XXXX.dump
# ============================================================

param(
    [Parameter(Mandatory = $true)]
    [string]$BackupFile
)

$ErrorActionPreference = "Stop"
$Container = "trading-postgres"
$DbUser    = "trading"
$DbName    = "tradingdb"

if (-not (Test-Path $BackupFile)) {
    Write-Host "ERROR: Dosya bulunamadı: $BackupFile" -ForegroundColor Red
    exit 1
}

$fullPath = (Resolve-Path $BackupFile).Path

Write-Host "DİKKAT: Bu işlem mevcut DB'yi SİLECEK ve yedekten geri yükleyecek." -ForegroundColor Yellow
$confirm = Read-Host "Devam etmek için 'yes' yazın"
if ($confirm -ne "yes") { Write-Host "İptal edildi."; exit 0 }

# 1) Container'a kopyala
$tmp = "/tmp/restore.dump"
docker cp $fullPath "${Container}:${tmp}"

# 2) Aktif bağlantıları kes + drop & recreate
docker exec $Container psql -U $DbUser -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='$DbName' AND pid <> pg_backend_pid();"
docker exec $Container psql -U $DbUser -d postgres -c "DROP DATABASE IF EXISTS $DbName;"
docker exec $Container psql -U $DbUser -d postgres -c "CREATE DATABASE $DbName;"

# 3) Restore
docker exec $Container pg_restore -U $DbUser -d $DbName $tmp

# 4) Temizle
docker exec $Container rm $tmp

Write-Host "OK: Restore tamamlandı." -ForegroundColor Green