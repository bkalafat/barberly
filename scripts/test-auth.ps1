#!/usr/bin/env pwsh

# Barberly API Test Script
# Tests authentication, authorization policies, and rate limiting

$BaseUrl = "http://localhost:5000"

Write-Host "🧪 Testing Barberly API Authentication & Authorization..." -ForegroundColor Green
Write-Host ""

# Test 1: Health Checks
Write-Host "1️⃣ Testing Health Checks..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/health/live" -Method GET
    Write-Host "✅ Health Check Passed" -ForegroundColor Green
} catch {
    Write-Host "❌ Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Anonymous Endpoints (Auth)
Write-Host ""
Write-Host "2️⃣ Testing Anonymous Auth Endpoints..." -ForegroundColor Yellow

# Test Register
try {
    $registerData = @{
        Email = "test@example.com"
        Password = "password123"
        FullName = "Test User"
        Role = "customer"
    } | ConvertTo-Json

    $registerResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "✅ Register Endpoint: $($registerResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "❌ Register Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Login
try {
    $loginData = @{
        Email = "test@example.com"
        Password = "password123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    Write-Host "✅ Login Endpoint: $($loginResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "❌ Login Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Protected Endpoints (should fail without auth)
Write-Host ""
Write-Host "3️⃣ Testing Protected Endpoints (should fail without auth)..." -ForegroundColor Yellow

$protectedEndpoints = @(
    "/weatherforecast",
    "/me",
    "/customer-only",
    "/barber-only",
    "/shop-owner-only",
    "/admin-only"
)

foreach ($endpoint in $protectedEndpoints) {
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl$endpoint" -Method GET
        Write-Host "❌ $endpoint should have failed but didn't" -ForegroundColor Red
    } catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-Host "✅ $endpoint correctly returned 401 Unauthorized" -ForegroundColor Green
        } else {
            Write-Host "⚠️ $endpoint returned unexpected status: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
        }
    }
}

# Test 4: Input Validation (Rate limiting removed for MVP)
Write-Host ""
Write-Host "4️⃣ Testing Input Validation..." -ForegroundColor Yellow

# Test invalid email
try {
    $invalidEmailData = @{
        Email = "invalid-email"
        Password = "password123"
        FullName = "Test User"
        Role = "customer"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method POST -Body $invalidEmailData -ContentType "application/json"
    Write-Host "❌ Invalid email should have failed" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✅ Invalid email correctly rejected" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Unexpected response for invalid email" -ForegroundColor Orange
    }
}

# Test weak password
try {
    $weakPasswordData = @{
        Email = "test@example.com"
        Password = "123"
        FullName = "Test User"
        Role = "customer"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method POST -Body $weakPasswordData -ContentType "application/json"
    Write-Host "❌ Weak password should have failed" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✅ Weak password correctly rejected" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Unexpected response for weak password" -ForegroundColor Orange
    }
}

# Test invalid role
try {
    $invalidRoleData = @{
        Email = "test@example.com"
        Password = "password123"
        FullName = "Test User"
        Role = "invalid-role"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method POST -Body $invalidRoleData -ContentType "application/json"
    Write-Host "❌ Invalid role should have failed" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✅ Invalid role correctly rejected" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Unexpected response for invalid role" -ForegroundColor Orange
    }
}

Write-Host ""
Write-Host "🎉 Testing Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Start the API with: dotnet run --project backend/src/Barberly.Api/Barberly.Api.csproj" -ForegroundColor White
Write-Host "2. Open Swagger UI at: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "3. Run this test script again to verify all functionality" -ForegroundColor White
