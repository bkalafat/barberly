#!/usr/bin/env pwsh

# Barberly API JWT Test Script
# Shows how to get JWT tokens and test protected endpoints

$BaseUrl = "http://localhost:5000"

Write-Host "🔐 Testing Barberly API with JWT Authentication..." -ForegroundColor Green
Write-Host ""

# Test 1: Health Checks
Write-Host "1️⃣ Testing Health Checks..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/health/live" -Method GET
    Write-Host "✅ Health Check Passed" -ForegroundColor Green
} catch {
    Write-Host "❌ Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get JWT Tokens
Write-Host ""
Write-Host "2️⃣ Getting JWT Tokens..." -ForegroundColor Yellow

# Login as customer to get token
try {
    $customerLoginData = @{
        Email = "customer@example.com"
        Password = "password123"
    } | ConvertTo-Json

    $customerLoginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -Body $customerLoginData -ContentType "application/json"
    $customerToken = $customerLoginResponse.token
    Write-Host "✅ Customer Token Generated: $($customerLoginResponse.user.role)" -ForegroundColor Green
    Write-Host "   Token: $($customerToken.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "❌ Customer Login Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Login as barber to get token
try {
    $barberLoginData = @{
        Email = "barber@example.com"
        Password = "password123"
    } | ConvertTo-Json

    $barberLoginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -Body $barberLoginData -ContentType "application/json"
    $barberToken = $barberLoginResponse.token
    Write-Host "✅ Barber Token Generated: $($barberLoginResponse.user.role)" -ForegroundColor Green
    Write-Host "   Token: $($barberToken.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "❌ Barber Login Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 3: Protected Endpoints WITHOUT authentication (should fail)
Write-Host ""
Write-Host "3️⃣ Testing Protected Endpoints (no auth - should fail)..." -ForegroundColor Yellow

$protectedEndpoints = @(
    "/weatherforecast",
    "/me",
    "/customer-only",
    "/barber-only"
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

# Test 4: Protected Endpoints WITH customer authentication
Write-Host ""
Write-Host "4️⃣ Testing Protected Endpoints with Customer Token..." -ForegroundColor Yellow

$customerHeaders = @{
    "Authorization" = "Bearer $customerToken"
    "Content-Type" = "application/json"
}

# Test /me endpoint
try {
    $meResponse = Invoke-RestMethod -Uri "$BaseUrl/me" -Method GET -Headers $customerHeaders
    Write-Host "✅ /me endpoint success: User $($meResponse.Name) ($($meResponse.UserType))" -ForegroundColor Green
} catch {
    Write-Host "❌ /me endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test customer-only endpoint
try {
    $customerResponse = Invoke-RestMethod -Uri "$BaseUrl/customer-only" -Method GET -Headers $customerHeaders
    Write-Host "✅ /customer-only endpoint success: $customerResponse" -ForegroundColor Green
} catch {
    Write-Host "❌ /customer-only endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test barber-only endpoint (should fail for customer)
try {
    $barberOnlyResponse = Invoke-RestMethod -Uri "$BaseUrl/barber-only" -Method GET -Headers $customerHeaders
    Write-Host "❌ /barber-only should have failed for customer but didn't" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        Write-Host "✅ /barber-only correctly denied customer access (403)" -ForegroundColor Green
    } else {
        Write-Host "⚠️ /barber-only unexpected status: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
    }
}

# Test 5: Protected Endpoints WITH barber authentication
Write-Host ""
Write-Host "5️⃣ Testing Protected Endpoints with Barber Token..." -ForegroundColor Yellow

$barberHeaders = @{
    "Authorization" = "Bearer $barberToken"
    "Content-Type" = "application/json"
}

# Test barber-only endpoint
try {
    $barberResponse = Invoke-RestMethod -Uri "$BaseUrl/barber-only" -Method GET -Headers $barberHeaders
    Write-Host "✅ /barber-only endpoint success: $barberResponse" -ForegroundColor Green
} catch {
    Write-Host "❌ /barber-only endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test customer-only endpoint (should fail for barber)
try {
    $customerOnlyResponse = Invoke-RestMethod -Uri "$BaseUrl/customer-only" -Method GET -Headers $barberHeaders
    Write-Host "❌ /customer-only should have failed for barber but didn't" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        Write-Host "✅ /customer-only correctly denied barber access (403)" -ForegroundColor Green
    } else {
        Write-Host "⚠️ /customer-only unexpected status: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
    }
}

# Test 6: Generate specific test tokens
Write-Host ""
Write-Host "6️⃣ Generate Test Tokens for Swagger..." -ForegroundColor Yellow

try {
    $testTokenResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/test-token?email=admin@example.com&role=admin" -Method POST
    Write-Host "✅ Admin Test Token Generated" -ForegroundColor Green
    Write-Host "   Copy this for Swagger UI:" -ForegroundColor Cyan
    Write-Host "   Bearer $($testTokenResponse.token)" -ForegroundColor White
    Write-Host ""
} catch {
    Write-Host "❌ Test token generation failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎉 JWT Testing Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📖 How to use JWT tokens in Swagger UI:" -ForegroundColor Cyan
Write-Host "1. Open: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "2. Click the 'Authorize' button (🔒 icon)" -ForegroundColor White
Write-Host "3. Enter: Bearer {your-token-from-above}" -ForegroundColor White
Write-Host "4. Click 'Authorize' and then 'Close'" -ForegroundColor White
Write-Host "5. Now you can test protected endpoints!" -ForegroundColor White
Write-Host ""
Write-Host "🔗 Available test tokens:" -ForegroundColor Cyan
Write-Host "   Customer: Login with customer@example.com" -ForegroundColor White
Write-Host "   Barber: Login with barber@example.com" -ForegroundColor White
Write-Host "   Admin: Use /auth/test-token endpoint" -ForegroundColor White
