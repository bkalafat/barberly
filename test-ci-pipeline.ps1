#!/usr/bin/env pwsh
# Simulate GitHub Actions Frontend CI/CD Pipeline
# This script runs the same commands that GitHub Actions will execute

Write-Host "🚀 Running Frontend CI/CD Pipeline Simulation" -ForegroundColor Green
Write-Host "=" * 50

$webDir = "c:\dev\barberly\web\barberly-web"
Set-Location $webDir

Write-Host "`n📦 Installing dependencies..." -ForegroundColor Yellow
npm ci

Write-Host "`n🔍 Linting code..." -ForegroundColor Yellow
npm run lint
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Lint failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n🔎 Type checking..." -ForegroundColor Yellow
npm run type-check  
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Type check failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n🧪 Running tests with coverage..." -ForegroundColor Yellow
npm run test:coverage -- --run
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n🏗️ Building application..." -ForegroundColor Yellow
$env:NODE_ENV = "production"
npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ All pipeline steps completed successfully!" -ForegroundColor Green
Write-Host "🎉 Your GitHub Actions workflow should now pass!" -ForegroundColor Cyan
