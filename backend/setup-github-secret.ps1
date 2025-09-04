# GitHub Secrets Setup Script
# This script helps you add the Supabase connection string to GitHub Secrets

param(
    [switch]$SetSecret = $false
)

$connectionString = "Host=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.cpibdbtyvpyduuyispxj;Password=!vxT8ap5Pxj._Kz;SSL Mode=Require;Trust Server Certificate=true"

Write-Host "Supabase Database Configuration for GitHub Actions" -ForegroundColor Green
Write-Host "=" * 50

if ($SetSecret) {
    Write-Host "Setting GitHub secret using gh CLI..." -ForegroundColor Yellow
    
    # Check if gh CLI is available
    $ghVersion = & gh --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "GitHub CLI (gh) not found. Please install it first: https://cli.github.com/"
        Write-Host "Alternative: Set the secret manually in GitHub web UI" -ForegroundColor Yellow
        exit 1
    }
    
    # Set the secret
    $connectionString | & gh secret set SUPABASE_CONNECTION
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Secret 'SUPABASE_CONNECTION' set successfully!" -ForegroundColor Green
    } else {
        Write-Error "Failed to set secret. Please set it manually in GitHub web UI."
        exit 1
    }
} else {
    Write-Host "To set the GitHub secret, you have two options:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option 1: Use GitHub CLI (automatic)" -ForegroundColor Cyan
    Write-Host "  .\setup-github-secret.ps1 -SetSecret"
    Write-Host ""
    Write-Host "Option 2: Set manually in GitHub web UI" -ForegroundColor Cyan
    Write-Host "  1. Go to: https://github.com/bkalafat/barberly/settings/secrets/actions"
    Write-Host "  2. Click 'New repository secret'"
    Write-Host "  3. Name: SUPABASE_CONNECTION"
    Write-Host "  4. Value (copy this):"
    Write-Host ""
    Write-Host $connectionString -ForegroundColor Green
    Write-Host ""
    Write-Host "After setting the secret, GitHub Actions will use Supabase instead of local PostgreSQL."
}
