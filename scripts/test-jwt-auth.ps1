#!/usr/bin/env pwsh

# Barberly API JWT Test Script
# Shows how to get JWT tokens and test protected endpoints

param(
    [string]$BaseUrl = "http://localhost:5156"
)

Write-Host "Testing Barberly API with JWT Authentication..." -ForegroundColor Green
Write-Host "API URL: $BaseUrl" -ForegroundColor Cyan
Write-Host ""

# Test 1: Health Checks
Write-Host "1. Testing Health Checks..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/health/live" -Method GET
    Write-Host "[PASS] Health Check Passed" -ForegroundColor Green
} catch {
    Write-Host "[FAIL] Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "INFO: Make sure API is running with: dotnet run --project backend/src/Barberly.Api/Barberly.Api.csproj" -ForegroundColor Yellow
    exit 1
}

# Test 2: Get JWT Tokens
Write-Host ""
Write-Host "2. Getting JWT Tokens..." -ForegroundColor Yellow

# Login as customer to get token
try {
    $customerLoginData = @{
        Email = "customer@example.com"
        Password = "password123"
    } | ConvertTo-Json

    $customerLoginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -Body $customerLoginData -ContentType "application/json"
    $customerToken = $customerLoginResponse.token
    Write-Host "[PASS] Customer Token Generated: $($customerLoginResponse.user.role)" -ForegroundColor Green
    Write-Host "   Token: $($customerToken.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "[FAIL] Customer Login Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "INFO: Trying registration fallback..." -ForegroundColor Yellow

    try {
        $registerData = @{
            Email = "customer@example.com"
            Password = "password123"
            FullName = "Test Customer"
            Role = "customer"
        } | ConvertTo-Json

        $registerResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method POST -Body $registerData -ContentType "application/json"
        Write-Host "[PASS] Customer registered successfully" -ForegroundColor Green

        # Retry login
        $customerLoginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -Body $customerLoginData -ContentType "application/json"
        $customerToken = $customerLoginResponse.token
        Write-Host "[PASS] Customer Token Generated: $($customerLoginResponse.user.role)" -ForegroundColor Green
    } catch {
        Write-Host "[FAIL] Customer registration/login failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Login as barber to get token
try {
    $barberLoginData = @{
        Email = "barber@example.com"
        Password = "password123"
    } | ConvertTo-Json

    $barberLoginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -Body $barberLoginData -ContentType "application/json"
    $barberToken = $barberLoginResponse.token
    Write-Host "[PASS] Barber Token Generated: $($barberLoginResponse.user.role)" -ForegroundColor Green
    Write-Host "   Token: $($barberToken.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "[FAIL] Barber Login Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "INFO: Trying registration fallback..." -ForegroundColor Yellow

    try {
        $registerData = @{
            Email = "barber@example.com"
            Password = "password123"
            FullName = "Test Barber"
            Role = "barber"
        } | ConvertTo-Json

        $registerResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method POST -Body $registerData -ContentType "application/json"
        Write-Host "[PASS] Barber registered successfully" -ForegroundColor Green

        # Retry login
        $barberLoginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -Body $barberLoginData -ContentType "application/json"
        $barberToken = $barberLoginResponse.token
        Write-Host "[PASS] Barber Token Generated: $($barberLoginResponse.user.role)" -ForegroundColor Green
    } catch {
        Write-Host "[FAIL] Barber registration/login failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Test 3: Protected Endpoints WITHOUT authentication (should fail)
Write-Host ""
Write-Host "3. Testing Protected Endpoints (no auth - should fail)..." -ForegroundColor Yellow

$protectedEndpoints = @(
    "/weatherforecast",
    "/me",
    "/customer-only",
    "/barber-only"
)

foreach ($endpoint in $protectedEndpoints) {
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl$endpoint" -Method GET
        Write-Host "[FAIL] $endpoint should have failed but didn't" -ForegroundColor Red
    } catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-Host "[PASS] $endpoint correctly returned 401 Unauthorized" -ForegroundColor Green
        } else {
            Write-Host "[WARN] $endpoint returned unexpected status: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
        }
    }
}

# Test 4: Protected Endpoints WITH customer authentication
Write-Host ""
Write-Host "4. Testing Protected Endpoints with Customer Token..." -ForegroundColor Yellow

$customerHeaders = @{
    "Authorization" = "Bearer $customerToken"
    "Content-Type" = "application/json"
}

# Test /me endpoint
try {
    $meResponse = Invoke-RestMethod -Uri "$BaseUrl/me" -Method GET -Headers $customerHeaders
    Write-Host "[PASS] /me endpoint success: User $($meResponse.Name) ($($meResponse.UserType))" -ForegroundColor Green
} catch {
    Write-Host "[FAIL] /me endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test customer-only endpoint
try {
    $customerResponse = Invoke-RestMethod -Uri "$BaseUrl/customer-only" -Method GET -Headers $customerHeaders
    Write-Host "[PASS] /customer-only endpoint success: $customerResponse" -ForegroundColor Green
} catch {
    Write-Host "[FAIL] /customer-only endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test barber-only endpoint (should fail for customer)
try {
    $barberOnlyResponse = Invoke-RestMethod -Uri "$BaseUrl/barber-only" -Method GET -Headers $customerHeaders
    Write-Host "[FAIL] /barber-only should have failed for customer but didn't" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        Write-Host "[PASS] /barber-only correctly denied customer access (403)" -ForegroundColor Green
    } else {
        Write-Host "[WARN] /barber-only unexpected status: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
    }
}

# Test 5: Protected Endpoints WITH barber authentication
Write-Host ""
Write-Host "5. Testing Protected Endpoints with Barber Token..." -ForegroundColor Yellow

$barberHeaders = @{
    "Authorization" = "Bearer $barberToken"
    "Content-Type" = "application/json"
}

# Test barber-only endpoint
try {
    $barberResponse = Invoke-RestMethod -Uri "$BaseUrl/barber-only" -Method GET -Headers $barberHeaders
    Write-Host "[PASS] /barber-only endpoint success: $barberResponse" -ForegroundColor Green
} catch {
    Write-Host "[FAIL] /barber-only endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test customer-only endpoint (should fail for barber)
try {
    $customerOnlyResponse = Invoke-RestMethod -Uri "$BaseUrl/customer-only" -Method GET -Headers $barberHeaders
    Write-Host "[FAIL] /customer-only should have failed for barber but didn't" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        Write-Host "[PASS] /customer-only correctly denied barber access (403)" -ForegroundColor Green
    } else {
        Write-Host "[WARN] /customer-only unexpected status: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
    }
}

# Test 6: Generate specific test tokens
Write-Host ""
Write-Host "6. Generate Test Tokens for Swagger..." -ForegroundColor Yellow

try {
    $testTokenResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/test-token?email=admin@example.com&role=admin" -Method POST
    Write-Host "[PASS] Admin Test Token Generated" -ForegroundColor Green
    Write-Host ""
    Write-Host "JWT TOKEN FOR SWAGGER UI:" -ForegroundColor Cyan
    Write-Host "Bearer $($testTokenResponse.token)" -ForegroundColor White
    Write-Host ""
} catch {
    Write-Host "[FAIL] Test token generation failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Input Validation
Write-Host ""
Write-Host "7. Testing Input Validation..." -ForegroundColor Yellow

# Test invalid email
try {
    $invalidEmailData = @{
        Email = "invalid-email"
        Password = "password123"
        FullName = "Test User"
        Role = "customer"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method POST -Body $invalidEmailData -ContentType "application/json"
    Write-Host "[FAIL] Invalid email should have failed" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "[PASS] Invalid email correctly rejected (400)" -ForegroundColor Green
    } else {
        Write-Host "[WARN] Unexpected response for invalid email: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
    }
}

# Test weak password
try {
    $weakPasswordData = @{
        Email = "weakpass@example.com"
        Password = "123"
        FullName = "Test User"
        Role = "customer"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method POST -Body $weakPasswordData -ContentType "application/json"
    Write-Host "[FAIL] Weak password should have failed" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "[PASS] Weak password correctly rejected (400)" -ForegroundColor Green
    } else {
        Write-Host "[WARN] Unexpected response for weak password: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
    }
}

# Test invalid role
try {
    $invalidRoleData = @{
        Email = "invalidrole@example.com"
        Password = "password123"
        FullName = "Test User"
        Role = "invalid-role"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method POST -Body $invalidRoleData -ContentType "application/json"
    Write-Host "[FAIL] Invalid role should have failed" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "[PASS] Invalid role correctly rejected (400)" -ForegroundColor Green
    } else {
        Write-Host "[WARN] Unexpected response for invalid role: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
    }
}

Write-Host ""
Write-Host "JWT TESTING COMPLETE!" -ForegroundColor Green
Write-Host ""
Write-Host "How to use JWT tokens in Swagger UI:" -ForegroundColor Cyan
Write-Host "1. Open: $BaseUrl/swagger" -ForegroundColor White
Write-Host "2. Click the 'Authorize' button (lock icon)" -ForegroundColor White
Write-Host "3. Enter: Bearer {your-token-from-above}" -ForegroundColor White
Write-Host "4. Click 'Authorize' and then 'Close'" -ForegroundColor White
Write-Host "5. Now you can test protected endpoints!" -ForegroundColor White
Write-Host ""
Write-Host "Available test tokens:" -ForegroundColor Cyan
Write-Host "   Customer: Login with customer@example.com" -ForegroundColor White
Write-Host "   Barber: Login with barber@example.com" -ForegroundColor White
Write-Host "   Admin: Use /auth/test-token endpoint" -ForegroundColor White
Write-Host ""
Write-Host "Usage examples:" -ForegroundColor Cyan
Write-Host "   .\scripts\test-jwt-auth.ps1" -ForegroundColor White
Write-Host "   .\scripts\test-jwt-auth.ps1 -BaseUrl 'http://localhost:5000'" -ForegroundColor White