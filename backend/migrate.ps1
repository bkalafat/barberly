# Barberly Database Migration Script
# This script loads environment variables from .env file and runs EF migrations

param(
    [string]$Environment = "Development",
    [switch]$UseSupabase = $false
)

Write-Host "Loading environment variables from .env file..." -ForegroundColor Green

# Load .env file if it exists
$envFile = Join-Path $PSScriptRoot ".env"
if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
        if ($_ -match '^([^#][^=]+)=(.*)$') {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($name, $value, "Process")
            Write-Host "Set $name" -ForegroundColor Yellow
        }
    }
}
else {
    Write-Warning ".env file not found. Please create one based on .env.example"
    exit 1
}

# Set the connection string based on the flag
if ($UseSupabase) {
    # Use session mode pooler (port 5432) for migrations - better than transaction mode (6543)
    $connectionString = [Environment]::GetEnvironmentVariable("SUPABASE_CONNECTION")
    Write-Host "Using Supabase connection (session mode - port 5432)" -ForegroundColor Cyan
    Write-Host "Note: This uses session mode pooler which is better for migrations than transaction mode" -ForegroundColor Green
}
else {
    $connectionString = [Environment]::GetEnvironmentVariable("LOCAL_CONNECTION")
    if ([string]::IsNullOrEmpty($connectionString)) {
        $connectionString = "Host=localhost;Port=5432;Database=BarberlyDb;Username=postgres;Password=postgres123;"
    }
    Write-Host "Using local connection" -ForegroundColor Cyan
}

# Set the environment variable that .NET will read
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $connectionString, "Process")

Write-Host "Connection string set: $($connectionString.Substring(0, 50))..." -ForegroundColor Green

# Change to the correct directory
Set-Location (Join-Path $PSScriptRoot "src")

# Ensure dotnet-ef is available
Write-Host "Ensuring dotnet-ef is available..." -ForegroundColor Green
dotnet tool restore

# Run migrations
Write-Host "Running EF migrations..." -ForegroundColor Green
dotnet ef database update `
    --project Barberly.Infrastructure/Barberly.Infrastructure.csproj `
    --startup-project Barberly.Api/Barberly.Api.csproj `
    --context BarberlyDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migrations completed successfully!" -ForegroundColor Green
}
else {
    Write-Error "Migration failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Optional: Run a quick test
Write-Host "Would you like to run integration tests? (y/n)" -ForegroundColor Yellow
$runTests = Read-Host
if ($runTests -eq 'y' -or $runTests -eq 'Y') {
    Write-Host "Running integration tests..." -ForegroundColor Green
    Set-Location ..
    dotnet test tests/Barberly.IntegrationTests/Barberly.IntegrationTests.csproj --verbosity minimal
}
