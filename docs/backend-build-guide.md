# Backend Build Guide - Barberly

This document provides comprehensive instructions for building and testing the Barberly backend solution.

## Quick Start

### Build the Entire Solution
```powershell
# Navigate to solution directory and build
Set-Location c:\dev\barberly\backend\src
dotnet build Barberly.sln

# Alternative: Build with verbose output
dotnet build Barberly.sln --verbosity normal
```

### Clean and Rebuild
```powershell
Set-Location c:\dev\barberly\backend\src
dotnet clean Barberly.sln
dotnet restore Barberly.sln
dotnet build Barberly.sln
```

## Solution Structure

The `Barberly.sln` file is located in `c:\dev\barberly\backend\src\` and contains:

### Core Projects
- **Barberly.Api** - HTTP endpoints, authentication, rate limiting
- **Barberly.Domain** - Business entities, value objects, domain events
- **Barberly.Application** - CQRS handlers, use cases, validation
- **Barberly.Infrastructure** - EF Core, Redis, Azure services, external integrations

### Test Projects (under Tests folder)
- **Barberly.Domain.Tests** - Domain unit tests
- **Barberly.Application.Tests** - Application layer unit tests
- **Barberly.IntegrationTests** - HTTP integration tests

## Build Commands Reference

### Basic Commands
```powershell
# Build only
dotnet build Barberly.sln

# Build specific project
dotnet build Barberly.Api\Barberly.Api.csproj

# Build in Release mode
dotnet build Barberly.sln --configuration Release
```

### Test Commands
```powershell
# Run all tests
dotnet test Barberly.sln

# Run specific test project
dotnet test ..\tests\Barberly.Application.Tests\

# Run tests with coverage
dotnet test Barberly.sln --collect:"XPlat Code Coverage"
```

### Package Management
```powershell
# Restore packages
dotnet restore Barberly.sln

# List outdated packages
dotnet list package --outdated

# Update packages (be careful with breaking changes)
dotnet add package <PackageName> --version <Version>
```

## Recent Issues and Fixes

### Fixed: Azure ServiceBus Package Issue
**Problem**: `Azure.ServiceBus` version 7.17.2 was not found
**Solution**: Updated to modern `Azure.Messaging.ServiceBus` package v7.20.1
**File**: `Barberly.Infrastructure.csproj`

### Fixed: FluentValidation Version Conflicts
**Problem**: Version mismatch between Application (11.8.3) and Tests (11.9.0)
**Solution**: Aligned all projects to FluentValidation 11.9.0
**Files**: `Barberly.Application.csproj`, `Barberly.Application.Tests.csproj`

### Fixed: Role Validation Test Failure
**Problem**: Test expected "CUSTOMER" (uppercase) to be invalid but validation was case-insensitive
**Solution**: Updated test helper to match business rules (only "customer" and "barber" allowed, case-sensitive)
**File**: `AuthValidationTests.cs`

## Build Troubleshooting

### Common Issues

1. **MSB1003: No project or solution file specified**
   - Ensure you're in the correct directory: `c:\dev\barberly\backend\src`
   - Use full path to solution file: `dotnet build Barberly.sln`

2. **Package not found errors (NU1101/NU1102)**
   - Run `dotnet restore` first
   - Check package versions in .csproj files
   - Verify package exists on NuGet.org

3. **Version conflicts (NU1603)**
   - Align package versions across all projects
   - Use `dotnet list package` to see current versions

4. **Build warnings about nullable references**
   - These are warnings, not errors (project has `<Nullable>enable</Nullable>`)
   - Can be addressed by adding null checks or nullable annotations

### Project Dependencies

```
Barberly.Api
├── Barberly.Application
├── Barberly.Infrastructure
└── Barberly.Domain

Barberly.Application
└── Barberly.Domain

Barberly.Infrastructure
├── Barberly.Application
└── Barberly.Domain

Test Projects
├── Barberly.Domain.Tests → Barberly.Domain
├── Barberly.Application.Tests → Barberly.Application + Barberly.Api.Models
└── Barberly.IntegrationTests → Barberly.Api + Barberly.Application + Barberly.Domain
```

## Package Versions (Current)

### Core Dependencies
- **MediatR**: 12.2.0 (CQRS)
- **FluentValidation**: 11.9.0 (Validation)
- **EntityFrameworkCore**: 8.0.0 (Data access)
- **Azure.Messaging.ServiceBus**: 7.20.1 (Messaging)
- **Azure.Storage.Blobs**: 12.19.1 (File storage)

### Test Dependencies
- **xUnit**: 2.6.1 (Test framework)
- **FluentAssertions**: 6.12.0 (Test assertions)
- **Microsoft.AspNetCore.Mvc.Testing**: 8.0.0 (Integration tests)

## IDE Setup

### Visual Studio Code
- Ensure C# extension is installed
- Use integrated terminal for build commands
- Tasks.json can be configured for common build operations

### Visual Studio
- Open `Barberly.sln` directly
- Use Build → Build Solution (Ctrl+Shift+B)
- All projects should build successfully

## Continuous Integration

For CI/CD pipelines, use:
```yaml
# Example GitHub Actions step
- name: Build Backend
  run: |
    cd backend/src
    dotnet restore Barberly.sln
    dotnet build Barberly.sln --configuration Release --no-restore
    dotnet test Barberly.sln --configuration Release --no-build
```

## Next Steps

1. **Add health checks** - Already partially implemented in API
2. **Implement proper authentication** - JWT structure is in place
3. **Add database migrations** - EF Core is configured
4. **Implement domain entities** - Following DDD patterns
5. **Add comprehensive logging** - Serilog integration planned

---

*Last updated: August 4, 2025*
*For issues or updates, see: docs/troubleshooting/backend-issues.md*
