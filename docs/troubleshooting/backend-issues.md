# Backend Troubleshooting Guide - Barberly

This document contains solutions to common issues encountered while building and testing the Barberly backend.

## Recent Issues Resolved (August 2025)

### 1. Azure ServiceBus Package Not Found (NU1102)

**Error Message:**
```
error NU1102: (>= 7.17.2) sürümüne sahip Azure.ServiceBus paketi bulunamıyor
```

**Root Cause:**
The project was referencing the legacy `Azure.ServiceBus` package (last updated 2015) instead of the modern Azure Service Bus client library.

**Solution:**
Updated `Barberly.Infrastructure.csproj` to use the modern package:

```xml
<!-- OLD (INCORRECT) -->
<PackageReference Include="Azure.ServiceBus" Version="7.17.2" />

<!-- NEW (CORRECT) -->
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.20.1" />
```

**Verification:**
```powershell
Set-Location c:\dev\barberly\backend\src
dotnet build Barberly.sln
# Should build successfully without NU1102 errors
```

### 2. FluentValidation Version Conflicts (NU1603)

**Error Message:**
```
warning NU1603: FluentValidation 11.8.3 depends on FluentValidation (>= 11.8.3) but version FluentValidation 11.9.0 was resolved
```

**Root Cause:**
Version mismatch between different projects in the solution:
- `Barberly.Application` had FluentValidation 11.8.3
- `Barberly.Application.Tests` had FluentValidation 11.9.0

**Solution:**
Aligned all projects to use FluentValidation 11.9.0:

**Files Updated:**
- `Barberly.Application.csproj` - Updated from 11.8.3 to 11.9.0
- `Barberly.Application.Tests.csproj` - Removed FluentValidation.TestHelper package

**Verification:**
```powershell
dotnet list package | Select-String "FluentValidation"
# All projects should show version 11.9.0
```


### Kimlik/Yetkilendirme ile İlgili Bilinen Sorunlar

- Register/login endpoint'leri için parola hashleme ve doğrulama eklendi. Parola doğrulama hatalarında 400 ve ProblemDetails döner.
- Test kullanıcıları ile /auth/register ve /auth/login uçları test edilebilir.
- Migration sonrası Users tablosu oluşmazsa, migration geçmişi ve DB bağlantısı kontrol edilmeli.
- EF Core migration dosyası elle eklendiğinde derleme hatası oluşur. CLI ile migration oluşturulmalı.
- Duplicate migration class hatası: Aynı migration iki dosyada varsa silinmeli.
- Migration uygulandıktan sonra veritabanında tablo oluşmuyorsa, connection string ve migration geçmişi kontrol edilmeli.

**Error Message:**
```
Expected IsValidRole(request.Role) to be false, but found True.
Test: RegisterRequest_InvalidRole_ShouldBeInvalid(role: "CUSTOMER")
```

**Root Cause:**
The test helper method `IsValidRole` was:
1. Using case-insensitive comparison (`ToLower()`)
2. Allowing 4 roles instead of the business requirement of 2 roles

Business requirement: Only "customer" and "barber" (case-sensitive) are valid roles.

**Solution:**
Updated `AuthValidationTests.cs`:

```csharp
// OLD (INCORRECT)
private static bool IsValidRole(string role)
{
    var validRoles = new[] { "customer", "barber", "shop_owner", "admin" };
    return validRoles.Contains(role?.ToLower());
}

// NEW (CORRECT)
private static bool IsValidRole(string role)
{
    var validRoles = new[] { "customer", "barber" };
    return validRoles.Contains(role);
}
```

**Test Data Updated:**
- Valid roles: "customer", "barber"
- Invalid roles: "", "invalid", "CUSTOMER", "BARBER", "user", "shop_owner", "admin"

**Verification:**
```powershell
dotnet test ..\tests\Barberly.Application.Tests\Auth\AuthValidationTests.cs
# All tests should pass
```

### 4. Infrastructure Project Reference in Integration Tests

**Error Context:**
Integration tests were referencing Infrastructure project directly, causing Azure.ServiceBus dependency issues.

**Solution:**
Removed Infrastructure project reference from `Barberly.IntegrationTests.csproj` following Clean Architecture principles:

```xml
<!-- REMOVED -->
<ProjectReference Include="..\..\src\Barberly.Infrastructure\Barberly.Infrastructure.csproj" />
```

Integration tests should only test through the API layer, not directly access Infrastructure components.

## Common Build Issues

### MSB1003: No project or solution file specified

**Symptoms:**
```
MSBUILD : error MSB1003: Bir proje veya çözüm dosyası belirtin
```

**Causes:**
- Wrong working directory
- Solution file not found

**Solutions:**
```powershell
# Ensure correct directory
Set-Location c:\dev\barberly\backend\src

# Verify solution file exists
Get-ChildItem *.sln
# Should show: Barberly.sln

# Build with explicit path
dotnet build Barberly.sln
```

### Package Restore Issues

**Symptoms:**
- NU1101: Package not found
- NU1102: Unable to find package

**Solutions:**
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore Barberly.sln

# If still failing, check package sources
dotnet nuget list source
```

### Test Discovery Issues

**Symptoms:**
- Tests not running
- "No tests found" messages

**Solutions:**
```powershell
# Rebuild test projects
dotnet clean ..\tests\
dotnet build ..\tests\

# Run with verbose output
dotnet test --logger:"console;verbosity=detailed"
```

## Package Version Alignment

### Current Aligned Versions

**Core Packages:**
```xml
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
```

**Azure Packages:**
```xml
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.20.1" />
<PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" />
```

**Test Packages:**
```xml
<PackageReference Include="xunit" Version="2.6.1" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
```

### Version Alignment Commands

```powershell
# Check for version mismatches
dotnet list package --include-transitive | Select-String "FluentValidation|MediatR|EntityFramework"

# Update specific package across solution
dotnet add package FluentValidation --version 11.9.0

# Check outdated packages
dotnet list package --outdated
```

## Clean Architecture Compliance

### Project Reference Rules

**✅ Allowed References:**
- API → Application, Infrastructure, Domain
- Application → Domain
- Infrastructure → Application, Domain
- Tests → Respective layer + shared test utilities

**❌ Forbidden References:**
- Domain → Any other project (pure business logic)
- Integration Tests → Infrastructure (test through API only)
- Application → Infrastructure (use interfaces)

### Validation Commands

```powershell
# Check project references
Get-Content **\*.csproj | Select-String "ProjectReference"

# Verify dependency direction
dotnet list reference
```

## Performance Monitoring

### Build Time Optimization

```powershell
# Build with timing
dotnet build Barberly.sln --verbosity diagnostic > build-log.txt

# Parallel build (faster)
dotnet build Barberly.sln --verbosity minimal -m
```

### Test Performance

```powershell
# Run tests with timing
dotnet test --logger trx --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=Unit
```

## Debug Information

### Useful Commands for Diagnostics

```powershell
# Show detailed package information
dotnet list package --include-transitive --format json

# Show project dependencies
dotnet list reference

# Show runtime information
dotnet --info

# Check for security vulnerabilities
dotnet list package --vulnerable
```

## Contact and Escalation

For unresolved issues:
1. Check this troubleshooting guide first
2. Review recent changes in git history
3. Check GitHub issues for similar problems
4. Consult architecture documentation in `docs/architecture/`

---

*Last updated: August 4, 2025*
*Related: docs/backend-build-guide.md*
