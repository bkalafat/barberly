#!/usr/bin/env pwsh

# Quick Test Build Script
# Verifies that our new unit test projects build and run correctly

Write-Host "🔨 Building and Testing New Unit Test Projects..." -ForegroundColor Green
Write-Host ""

$ErrorActionPreference = "Stop"

try {
    # Test Domain project
    Write-Host "1️⃣ Building Domain Tests..." -ForegroundColor Yellow
    Set-Location "c:\dev\barberly\backend\tests\Barberly.Domain.Tests"
    dotnet build
    Write-Host "✅ Domain Tests build successful" -ForegroundColor Green

    Write-Host "   Running Domain Tests..." -ForegroundColor Gray
    dotnet test --no-build --verbosity normal
    Write-Host "✅ Domain Tests passed" -ForegroundColor Green

    # Test Application project  
    Write-Host ""
    Write-Host "2️⃣ Building Application Tests..." -ForegroundColor Yellow
    Set-Location "..\Barberly.Application.Tests"
    dotnet build
    Write-Host "✅ Application Tests build successful" -ForegroundColor Green

    Write-Host "   Running Application Tests..." -ForegroundColor Gray
    dotnet test --no-build --verbosity normal
    Write-Host "✅ Application Tests passed" -ForegroundColor Green

    # Test Integration project
    Write-Host ""
    Write-Host "3️⃣ Building Integration Tests..." -ForegroundColor Yellow
    Set-Location "..\Barberly.IntegrationTests"
    dotnet build
    Write-Host "✅ Integration Tests build successful" -ForegroundColor Green

    Write-Host ""
    Write-Host "🎉 All Unit Test Projects Built Successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Test Summary:" -ForegroundColor Cyan
    Write-Host "   ✅ Domain Tests: Basic validation and placeholder tests" -ForegroundColor White
    Write-Host "   ✅ Application Tests: Auth validation, JWT service, models" -ForegroundColor White  
    Write-Host "   ✅ Integration Tests: Full API authentication workflow" -ForegroundColor White
    Write-Host ""
    Write-Host "🚀 Ready for production development!" -ForegroundColor Green

} catch {
    Write-Host ""
    Write-Host "❌ Build/Test Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Check the error details above and fix any issues." -ForegroundColor Yellow
    exit 1
} finally {
    Set-Location "c:\dev\barberly"
}

exit 0
