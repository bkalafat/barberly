# Barberly Frontend Simple Verification
# Run from the frontend directory

Write-Host "Barberly Frontend Verification Starting..." -ForegroundColor Green
Write-Host ""

# Clear npm cache first
Write-Host "   Clearing npm cache..." -ForegroundColor Yellow
npm cache clean --force

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
npm run test:coverage
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "   Building for production..." -ForegroundColor Yellow
npm run build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "ALL FRONTEND STEPS SUCCESSFUL!" -ForegroundColor Green
exit 0
