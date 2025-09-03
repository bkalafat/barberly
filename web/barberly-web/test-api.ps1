Write-Host "Testing API endpoint..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5156/api/v1/barbers" -Method GET
    Write-Host "API Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 10)
    Write-Host "`nResponse type: $($response.GetType())" -ForegroundColor Cyan
    if ($response -is [Array]) {
        Write-Host "Array length: $($response.Count)" -ForegroundColor Cyan
    }
    Write-Host "`nAPI is working! Found $($response.Count) barbers." -ForegroundColor Green
}
catch {
    Write-Host "API Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Full error: $($_.ErrorDetails.Message)" -ForegroundColor Red
}
