#!/usr/bin/env powershell

Write-Host "Cleaning npm cache and node_modules..." -ForegroundColor Yellow
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue node_modules
npm cache clean --force

Write-Host "Installing dependencies with legacy peer deps to resolve Storybook/Vite conflicts..." -ForegroundColor Yellow
npm install --legacy-peer-deps

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Dependencies installed successfully!" -ForegroundColor Green
    Write-Host "🧪 Testing Storybook..." -ForegroundColor Yellow
    npm run build-storybook
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Storybook build successful!" -ForegroundColor Green
    } else {
        Write-Host "❌ Storybook build failed. Check configuration." -ForegroundColor Red
    }
} else {
    Write-Host "❌ Dependency installation failed." -ForegroundColor Red
}
