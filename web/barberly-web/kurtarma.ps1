#!/usr/bin/env powershell

Write-Host "Temizlik basliyor..." -ForegroundColor Yellow

# Remove node_modules if exists
if (Test-Path "node_modules") {
    Write-Host "node_modules siliniyor..." -ForegroundColor Gray
    Remove-Item -Recurse -Force node_modules
}

# Remove package-lock.json if exists
if (Test-Path "package-lock.json") {
    Write-Host "package-lock.json siliniyor..." -ForegroundColor Gray
    Remove-Item -Force package-lock.json
}

# Clear npm cache
Write-Host "npm cache temizleniyor..." -ForegroundColor Gray
npm cache clean --force

# Install with legacy peer deps
Write-Host "Paketler yukleniyor..." -ForegroundColor Green
npm install --legacy-peer-deps

if ($LASTEXITCODE -eq 0) {
    Write-Host "Paketler basariyla yuklendi!" -ForegroundColor Green
} else {
    Write-Host "Paket yukleme hatasi." -ForegroundColor Red
}
