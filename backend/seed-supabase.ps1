# Seed Supabase Database Script
# This script runs the seeding manually for Supabase

param()

Write-Host "Seeding Supabase Database..." -ForegroundColor Green

# Load .env file
$envFile = Join-Path $PSScriptRoot ".env"
if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
        if ($_ -match '^([^#][^=]+)=(.*)$') {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($name, $value, "Process")
        }
    }
    Write-Host "✅ Loaded .env file" -ForegroundColor Green
}
else {
    Write-Error ".env file not found!"
    exit 1
}

# Set connection string
$connectionString = [Environment]::GetEnvironmentVariable("SUPABASE_CONNECTION")
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $connectionString, "Process")

# Start the API temporarily to trigger seeding
Write-Host "Starting API to trigger database seeding..." -ForegroundColor Cyan
Set-Location (Join-Path $PSScriptRoot "src")

# Start the API in background
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "Barberly.Api/Barberly.Api.csproj", "--urls", "http://localhost:5000" -PassThru -WindowStyle Hidden

Write-Host "Waiting 10 seconds for seeding to complete..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Stop the API
if ($apiProcess -and !$apiProcess.HasExited) {
    Write-Host "Stopping API..." -ForegroundColor Yellow
    $apiProcess.Kill()
    $apiProcess.WaitForExit(5000)
}

Write-Host "✅ Database seeding completed!" -ForegroundColor Green
Write-Host "You can now run integration tests against the seeded Supabase database." -ForegroundColor Cyan
