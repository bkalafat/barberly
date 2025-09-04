# Quick commit script to test the fix

Write-Host "Staging changes..." -ForegroundColor Green
git add .

Write-Host "Committing the test fix..." -ForegroundColor Green
git commit -m "fix: update GetAvailability test to use dynamic barber ID

- Replace hardcoded GUID with API query to find actual barber
- Test now queries /api/v1/barbers to get real seeded data
- This fixes the 404 'Barber not found' error in CI
- Consistent with other tests that query for actual IDs"

Write-Host "Pushing to trigger GitHub Actions..." -ForegroundColor Green
git push

Write-Host "✅ Changes pushed! Check GitHub Actions at:" -ForegroundColor Green
Write-Host "https://github.com/bkalafat/barberly/actions" -ForegroundColor Cyan
