# Quick Commands Reference - Barberly Backend

This is a quick reference for common backend development tasks. Keep this handy for frequent operations.

## 🚀 Quick Build Commands

### Essential Build Commands
```powershell
# Navigate to solution and build everything
Set-Location c:\dev\barberly\backend\src; dotnet build Barberly.sln

# Clean build (when things get weird)
dotnet clean Barberly.sln; dotnet restore; dotnet build Barberly.sln

# Build with full output
dotnet build Barberly.sln --verbosity normal
```

### Quick Test Commands
```powershell
# Run all tests
dotnet test Barberly.sln

# Run specific test project
dotnet test ..\tests\Barberly.Application.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🔧 Troubleshooting Commands

### Package Issues
```powershell
# Clear NuGet cache and restore
dotnet nuget locals all --clear; dotnet restore Barberly.sln

# Check package versions
dotnet list package

# Find outdated packages
dotnet list package --outdated
```

### Build Issues
```powershell
# Check project references
dotnet list reference

# Build single project
dotnet build Barberly.Api\Barberly.Api.csproj

# Verbose build for debugging
dotnet build Barberly.sln --verbosity diagnostic
```

## 📁 Important File Locations

### Solution Structure
```
c:\dev\barberly\backend\src\Barberly.sln          # Main solution file
c:\dev\barberly\backend\src\Barberly.Api\         # HTTP API project
c:\dev\barberly\backend\src\Barberly.Application\ # Business logic
c:\dev\barberly\backend\src\Barberly.Domain\      # Core domain
c:\dev\barberly\backend\src\Barberly.Infrastructure\ # External services
c:\dev\barberly\backend\tests\                    # All test projects
```

### Key Configuration Files
```
Barberly.Infrastructure.csproj  # Azure packages, EF Core
Barberly.Application.csproj     # MediatR, FluentValidation
AuthValidationTests.cs          # Role validation logic
Program.cs                      # API configuration
```

## 🎯 Recently Fixed Issues

### Azure ServiceBus Package ✅
**Problem**: `NU1102: Azure.ServiceBus version 7.17.2 not found`
**Solution**: Use `Azure.Messaging.ServiceBus` version `7.20.1` instead
**File**: `Barberly.Infrastructure.csproj`

### Role Validation Test ✅  
**Problem**: "CUSTOMER" (uppercase) passed validation when it should fail
**Solution**: Made validation case-sensitive, only "customer"/"barber" allowed
**File**: `AuthValidationTests.cs`

### FluentValidation Versions ✅
**Problem**: Version conflicts between projects
**Solution**: Aligned all to version `11.9.0`
**Files**: All `.csproj` files

## 🏗️ Current Package Versions

### Core Dependencies
```xml
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.20.1" />
```

### Test Dependencies
```xml
<PackageReference Include="xunit" Version="2.6.1" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
```

## 🔍 Diagnostic Commands

### When Build Fails
```powershell
# Check .NET version
dotnet --version

# Check installed SDKs
dotnet --list-sdks

# Verbose restore
dotnet restore Barberly.sln --verbosity diagnostic

# Check for duplicate references
Get-Content **\*.csproj | Select-String "PackageReference.*FluentValidation"
```

### When Tests Fail
```powershell
# Run specific test with details
dotnet test --filter "RegisterRequest_InvalidRole_ShouldBeInvalid" --logger:"console;verbosity=detailed"

# Run authentication tests only
dotnet test --filter "Category=Auth"

# List all discovered tests
dotnet test --list-tests
```

## 🎨 Development Workflow

### Typical Development Session
1. Navigate to solution: `Set-Location c:\dev\barberly\backend\src`
2. Build solution: `dotnet build Barberly.sln`
3. Run tests: `dotnet test Barberly.sln`
4. Make changes
5. Test specific area: `dotnet test ..\tests\Barberly.Application.Tests`
6. Full test run: `dotnet test Barberly.sln`

### Before Committing
```powershell
# Full clean build
dotnet clean Barberly.sln
dotnet restore Barberly.sln
dotnet build Barberly.sln

# All tests pass
dotnet test Barberly.sln

# Check for warnings
dotnet build Barberly.sln --verbosity normal | Select-String "warning"
```

## 🚨 Common Pitfalls

1. **Wrong Directory**: Always ensure you're in `c:\dev\barberly\backend\src`
2. **Package Cache**: Clear NuGet cache if packages seem corrupted
3. **Version Mismatches**: Keep package versions aligned across projects
4. **Test Context**: Authentication tests are case-sensitive for roles
5. **Clean Architecture**: Don't add Infrastructure references to Integration tests

## 📚 Documentation References

- **Full Build Guide**: `docs/backend-build-guide.md`
- **Troubleshooting**: `docs/troubleshooting/backend-issues.md`
- **Recent Changes**: `docs/recent-fixes-context.md`
- **Architecture Guide**: `.github/copilot-instructions.md`

---

*Quick reference - keep this handy for daily development*
*Last updated: August 4, 2025*
