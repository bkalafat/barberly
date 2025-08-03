# Barberly Projesi Otomatik Doğrulama Scripti

Write-Host "🏗️ Barberly Verification Pipeline Starting..." -ForegroundColor Green
Write-Host ""

Write-Host "==> Backend Build & Unit Tests"
Set-Location ./backend/src/Barberly.Api
if (-not (Test-Path "./Barberly.Api.csproj")) { Write-Error "Barberly.Api.csproj bulunamadı!"; exit 1 }

Write-Host "   Building API project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) { Write-Error "Backend build failed!"; exit $LASTEXITCODE }

Write-Host "   Running Unit Tests..." -ForegroundColor Yellow
Set-Location ../../tests
dotnet test Barberly.Domain.Tests/Barberly.Domain.Tests.csproj --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) { Write-Error "Domain tests failed!"; exit $LASTEXITCODE }

dotnet test Barberly.Application.Tests/Barberly.Application.Tests.csproj --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) { Write-Error "Application tests failed!"; exit $LASTEXITCODE }

Write-Host "   Running Integration Tests..." -ForegroundColor Yellow
dotnet test Barberly.IntegrationTests/Barberly.IntegrationTests.csproj --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) { Write-Error "Integration tests failed!"; exit $LASTEXITCODE }


Write-Host "==> Backend Health Check"
Set-Location ../src/Barberly.Api
Write-Host "   Starting API for health check..." -ForegroundColor Yellow
$backendProc = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru
Start-Sleep -Seconds 10

Write-Host "   Testing health endpoint..." -ForegroundColor Yellow
$response = $null
try {
  $response = Invoke-WebRequest -Uri "http://localhost:5000/health/ready" -UseBasicParsing -TimeoutSec 10
} catch { 
  Write-Error "Health check failed!"; 
  if ($backendProc) { $backendProc | Stop-Process -Force } 
  exit 1 
}
if ($response.StatusCode -ne 200) { 
  Write-Error "Health check returned status: $($response.StatusCode)"; 
  if ($backendProc) { $backendProc | Stop-Process -Force } 
  exit 1 
}

Write-Host "   Stopping API..." -ForegroundColor Yellow
if ($backendProc) { $backendProc | Stop-Process -Force }

Set-Location ../../../../../..


Write-Host "==> Frontend Lint & Type Check"
Set-Location ./web/barberly-web
if (-not (Test-Path "./package.json")) { 
  Write-Host "⚠️  Frontend not implemented yet, skipping frontend tests" -ForegroundColor Yellow
  Set-Location ../..
} else {
  Write-Host "   Installing dependencies..." -ForegroundColor Yellow
  npm install
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  
  Write-Host "   Running linter..." -ForegroundColor Yellow
  npm run lint
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  
  Write-Host "   Type checking..." -ForegroundColor Yellow
  npm run type-check
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  Write-Host "   Running unit tests..." -ForegroundColor Yellow
  npm test
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  Write-Host "   Building for production..." -ForegroundColor Yellow
  npm run build
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  Write-Host "   Running E2E tests (if available)..." -ForegroundColor Yellow
  if (Test-Path "./e2e") {
    npx playwright test
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  
  Set-Location ../..
}

Write-Host ""
Write-Host "🎉 TÜM ADIMLAR BAŞARILI!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Test Coverage Summary:" -ForegroundColor Cyan
Write-Host "   ✅ Unit Tests: Domain & Application layers" -ForegroundColor White
Write-Host "   ✅ Integration Tests: API endpoints" -ForegroundColor White
Write-Host "   ✅ Health Checks: API availability" -ForegroundColor White
Write-Host "   ✅ Manual Tests: JWT authentication (run test-jwt-auth.ps1)" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Ready for next development phase!" -ForegroundColor Green
exit 0
