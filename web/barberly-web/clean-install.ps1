Write-Host "Temizlik basliyor..." -ForegroundColor Yellow

if (Test-Path "node_modules") {
    Write-Host "node_modules siliniyor..." -ForegroundColor Gray
    Remove-Item -Recurse -Force node_modules
}

if (Test-Path "package-lock.json") {
    Write-Host "package-lock.json siliniyor..." -ForegroundColor Gray
    Remove-Item -Force package-lock.json
}

Write-Host "npm cache temizleniyor..." -ForegroundColor Gray
npm cache clean --force

Write-Host "Paketler yukleniyor..." -ForegroundColor Green
npm install --legacy-peer-deps

if ($LASTEXITCODE -eq 0) {
    Write-Host "Paketler basariyla yuklendi!" -ForegroundColor Green
} else {
    Write-Host "Paket yukleme hatasi." -ForegroundColor Red
}
