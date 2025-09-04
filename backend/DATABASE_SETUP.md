# Database Configuration Guide

This guide explains how to configure the database connection for different environments.

## Local Development

### Option 1: Using Local PostgreSQL

1. Keep the default `appsettings.Development.json` configuration
2. Ensure PostgreSQL is running locally on port 5432 with the credentials in the config

### Option 2: Using Supabase (Remote)

1. Copy `.env.example` to `.env` in the `backend/` directory
2. Fill in your actual Supabase credentials in `.env`
3. Run migrations using the provided PowerShell script:

```powershell
# Navigate to backend directory
cd backend

# Run migrations against Supabase
.\migrate.ps1 -UseSupabase

# Or run against local DB
.\migrate.ps1
```

## Environment Variables

The application reads these environment variables (in order of preference):

1. **ConnectionStrings\_\_DefaultConnection** - Direct connection string override
2. **SUPABASE_CONNECTION** - Full Supabase connection string from `.env`
3. **LOCAL_CONNECTION** - Local PostgreSQL connection string from `.env`
4. Fallback to `appsettings.json` configuration

## GitHub Actions / CI

Add the following secret to your GitHub repository:

1. Go to Repository Settings → Secrets and variables → Actions
2. Add a new repository secret:
   - **Name**: `SUPABASE_CONNECTION`
   - **Value**: `Host=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.cpibdbtyvpyduuyispxj;Password=!vxT8ap5Pxj._Kz;SSL Mode=Require;Trust Server Certificate=true`

The GitHub workflow will automatically use this secret to connect to Supabase and run migrations.

## Commands

### Manual Migration Commands

```powershell
# Set environment variable manually (PowerShell)
$env:ConnectionStrings__DefaultConnection = "Host=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.cpibdbtyvpyduuyispxj;Password=!vxT8ap5Pxj._Kz;SSL Mode=Require;Trust Server Certificate=true"

# Run migrations
dotnet ef database update --project backend/src/Barberly.Infrastructure/Barberly.Infrastructure.csproj --startup-project backend/src/Barberly.Api/Barberly.Api.csproj --context BarberlyDbContext

# Run tests
dotnet test backend/tests/Barberly.IntegrationTests/Barberly.IntegrationTests.csproj --verbosity minimal
```

## Security Notes

- Never commit `.env` files to version control (they're in `.gitignore`)
- Always use environment variables or GitHub Secrets for credentials
- Rotate database passwords regularly
- Use SSL/TLS connections (`SSL Mode=Require`) for remote databases
