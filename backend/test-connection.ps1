# Test Supabase Connection Script
# This script tests the Supabase connection before running migrations

param(
    [switch]$TestOnly = $false
)

Write-Host "Testing Supabase Database Connection..." -ForegroundColor Green
Write-Host "=" * 50

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
} else {
    Write-Error ".env file not found!"
    exit 1
}

# Get connection string
$connectionString = [Environment]::GetEnvironmentVariable("SUPABASE_CONNECTION")
if ([string]::IsNullOrEmpty($connectionString)) {
    Write-Error "SUPABASE_CONNECTION not found in .env file!"
    exit 1
}

Write-Host "Connection details:" -ForegroundColor Yellow
Write-Host "Host: $(($connectionString -split ';' | Where-Object {$_ -like 'Host=*'}) -replace 'Host=','')" 
Write-Host "Port: $(($connectionString -split ';' | Where-Object {$_ -like 'Port=*'}) -replace 'Port=','')"
Write-Host "Database: $(($connectionString -split ';' | Where-Object {$_ -like 'Database=*'}) -replace 'Database=','')"
Write-Host "User: $(($connectionString -split ';' | Where-Object {$_ -like 'Username=*'}) -replace 'Username=','')"

# Set the environment variable
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $connectionString, "Process")

# Change to src directory
Set-Location (Join-Path $PSScriptRoot "src")

# Test with a simple EF command (list migrations)
Write-Host "`nTesting connection with EF Core..." -ForegroundColor Cyan
dotnet ef migrations list --project Barberly.Infrastructure/Barberly.Infrastructure.csproj --startup-project Barberly.Api/Barberly.Api.csproj --context BarberlyDbContext 2>&1

$exitCode = $LASTEXITCODE
if ($exitCode -eq 0) {
    Write-Host "✅ Connection test successful!" -ForegroundColor Green
    if (-not $TestOnly) {
        Write-Host "`nProceeding with migrations..." -ForegroundColor Green
        dotnet ef database update --project Barberly.Infrastructure/Barberly.Infrastructure.csproj --startup-project Barberly.Api/Barberly.Api.csproj --context BarberlyDbContext
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Migrations completed successfully!" -ForegroundColor Green
        } else {
            Write-Error "❌ Migration failed with exit code $LASTEXITCODE"
            exit $LASTEXITCODE
        }
    }
} else {
    Write-Error "❌ Connection test failed with exit code $exitCode"
    Write-Host "`nTroubleshooting tips:" -ForegroundColor Yellow
    Write-Host "1. Check if your Supabase project is running"
    Write-Host "2. Verify the password in .env file"  
    Write-Host "3. Ensure you're using session mode (port 5432) not transaction mode (port 6543)"
    Write-Host "4. Try connecting directly to test: psql `"$connectionString`""
    exit $exitCode
}
