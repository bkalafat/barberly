Write-Host "Testing availability endpoint with real barber ID..." -ForegroundColor Green

$barberId = "9e862653-65c0-41b0-82e9-754f638baa49"
$date = "2025-01-15"
$serviceId = "1"
$url = "http://localhost:5156/v1/barbers/$barberId/availability?date=$date&serviceId=$serviceId"

Write-Host "Testing URL: $url" -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri $url -Method Get -ContentType "application/json"
    Write-Host "Success! Availability slots found:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
    Write-Host "Number of slots: $($response.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}
