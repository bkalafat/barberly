# Barberly Frontend Otomatik Doğrulama Scripti
# Only runs frontend (web/barberly-web) checks

Write-Host "Barberly Frontend Verification Starting..." -ForegroundColor Green
Write-Host ""

Set-Location ./web/barberly-web
if (-not (Test-Path "./package.json")) {
    Write-Host "   [SKIP] Frontend not implemented yet, following MVP plan" -ForegroundColor Yellow
    Set-Location ../..
    exit 0
}

Write-Host "   Installing dependencies..." -ForegroundColor Yellow
npm ci  # Use ci for faster, reproducible builds
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "   Running linter..." -ForegroundColor Yellow
npm run lint
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "   Type checking..." -ForegroundColor Yellow
npm run type-check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "   Running unit tests..." -ForegroundColor Yellow
npm test -- --coverage
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "   Building for production..." -ForegroundColor Yellow
npm run build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# E2E tests (Playwright as per copilot-instructions)
Write-Host "   Running E2E tests (if available)..." -ForegroundColor Yellow
if (Test-Path "./e2e") {
    npx playwright test
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Set-Location ../..

Write-Host ""
Write-Host "ALL FRONTEND STEPS SUCCESSFUL!" -ForegroundColor Green
exit 0
