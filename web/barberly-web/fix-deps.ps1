#!/usr/bin/env pwsh
# Fix npm dependency conflicts and install packages

Write-Host "Fixing npm dependency conflicts..." -ForegroundColor Green

# Remove node_modules and package-lock.json to start fresh
if (Test-Path "node_modules") {
    Write-Host "Removing node_modules..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force "node_modules"
}

if (Test-Path "package-lock.json") {
    Write-Host "Removing package-lock.json..." -ForegroundColor Yellow
    Remove-Item -Force "package-lock.json"
}

# Clear npm cache
Write-Host "Clearing npm cache..." -ForegroundColor Yellow
npm cache clean --force

# Install dependencies with legacy peer deps to handle conflicts
Write-Host "Installing dependencies..." -ForegroundColor Green
npm install --legacy-peer-deps

Write-Host "✅ Dependencies installed successfully!" -ForegroundColor Green
Write-Host "You can now run:" -ForegroundColor Cyan
Write-Host "  npm run dev      # Start development server" -ForegroundColor Cyan
Write-Host "  npm run storybook # Start Storybook" -ForegroundColor Cyan
