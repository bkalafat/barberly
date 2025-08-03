# Barberly Projesi Otomatik Doğrulama Scripti


Write-Host "==> Backend Lint & Test"
Set-Location ./backend/src/Barberly.Api
if (-not (Test-Path "./Barberly.Api.csproj")) { Write-Error "Barberly.Api.csproj bulunamadı!"; exit 1 }
dotnet build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet test
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }


Write-Host "==> Backend Health Check"
$backendProc = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru
Start-Sleep -Seconds 10
$response = $null
try {
  $response = Invoke-WebRequest -Uri "http://localhost:5000/health/ready" -UseBasicParsing -TimeoutSec 10
} catch { Write-Error "Health check failed!"; if ($backendProc) { $backendProc | Stop-Process } exit 1 }
if ($response.StatusCode -ne 200) { Write-Error "Health check failed!"; if ($backendProc) { $backendProc | Stop-Process } exit 1 }
if ($backendProc) { $backendProc | Stop-Process }

Set-Location ../../../..


Write-Host "==> Frontend Lint & Type Check"
Set-Location ./web/barberly-web
if (-not (Test-Path "./package.json")) { Write-Error "package.json bulunamadı!"; exit 1 }
npm install
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
npm run lint
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
npm run type-check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }


Write-Host "==> Frontend Unit Test"
npm test
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }


Write-Host "==> Frontend Build"
npm run build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }


Write-Host "==> E2E Test (varsa)"
if (Test-Path "./e2e") {
  npx playwright test
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}


Write-Host "==> TÜM ADIMLAR BAŞARILI!"
exit 0
