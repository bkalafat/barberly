# Authentication & Authorization Implementation

This document describes the authentication and authorization implementation for the Barberly API, including policies, rate limiting, and security configurations.

## Overview

The Barberly API implements a comprehensive security system using:
- **Azure AD B2C** integration for authentication
- **Policy-based authorization** for role-based access control
- **Rate limiting** to prevent abuse
- **Security headers** for protection against common attacks
- **Input validation** for data integrity

## Authentication Architecture

### Azure AD B2C Integration

The API is configured to work with Azure AD B2C for user authentication:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.TokenValidationParameters.NameClaimType = "name";
    }, 
    options => builder.Configuration.Bind("AzureAdB2C", options));
```

### Configuration

The application expects the following configuration in `appsettings.json`:

```json
{
  "AzureAdB2C": {
    "Instance": "https://{your-tenant-name}.b2clogin.com",
    "Domain": "{your-tenant-name}.onmicrosoft.com",
    "ClientId": "{your-client-id}",
    "SignUpSignInPolicyId": "B2C_1_susi"
  }
}
```

## Authorization Policies

### Implemented Policies

1. **Default Policy**: Requires authenticated user
2. **CustomerPolicy**: Requires `extension_UserType` claim with value "Customer"
3. **BarberPolicy**: Requires `extension_UserType` claim with value "Barber"
4. **ShopOwnerPolicy**: Requires `extension_UserType` claim with value "ShopOwner"
5. **AdminPolicy**: Requires `extension_UserType` claim with value "Admin"

### Policy Configuration

```csharp
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    
    options.AddPolicy("CustomerPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("extension_UserType", "Customer"));
    
    // Additional policies...
});
```

### Protected Endpoints

The following endpoints demonstrate policy-based authorization:

- `GET /customer-only` - Requires CustomerPolicy
- `GET /barber-only` - Requires BarberPolicy  
- `GET /shop-owner-only` - Requires ShopOwnerPolicy
- `GET /admin-only` - Requires AdminPolicy

## Rate Limiting

### Configuration

Three rate limiting policies are implemented:

1. **Global Limiter**: 100 requests per minute per user/IP
2. **AuthPolicy**: 5 requests per minute per IP (for auth endpoints)
3. **PublicPolicy**: 50 requests per minute per IP (for public endpoints)

### Implementation

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

### Applied Endpoints

- `/auth/register` and `/auth/login` use **AuthPolicy** (5 req/min)
- All other endpoints use the global limiter (100 req/min)

## Security Headers

The application adds security headers to all responses:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;");
    
    await next.Invoke();
});
```

## Authentication Endpoints

### POST /auth/register

Registers a new user with the following validation:
- Email must be valid format
- Password must be at least 6 characters
- Role must be either "customer" or "barber"
- Rate limited to 5 requests per minute per IP

**Request Example:**
```json
{
  "email": "user@example.com",
  "password": "password123",
  "fullName": "John Doe",
  "role": "customer"
}
```

**Response Example:**
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "message": "User registered successfully"
}
```

### POST /auth/login

Authenticates a user and returns a token:
- Email and password are required
- Rate limited to 5 requests per minute per IP

**Request Example:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response Example:**
```json
{
  "token": "mock-jwt-token-abc12345",
  "message": "Login successful"
}
```

## Testing the Implementation

### 1. Manual Testing

Use the provided PowerShell script to test all functionality:

```powershell
.\scripts\test-auth.ps1
```

### 2. Using Swagger UI

1. Start the API: `dotnet run --project backend/src/Barberly.Api/Barberly.Api.csproj`
2. Open Swagger UI: `http://localhost:5000/swagger`
3. Test endpoints using the interactive interface

### 3. Using curl

```bash
# Test registration
curl -X POST "http://localhost:5000/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123",
    "fullName": "Test User",
    "role": "customer"
  }'

# Test login
curl -X POST "http://localhost:5000/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123"
  }'

# Test protected endpoint (should fail without auth)
curl -X GET "http://localhost:5000/me"

# Test rate limiting (run multiple times quickly)
for i in {1..7}; do
  curl -X POST "http://localhost:5000/auth/login" \
    -H "Content-Type: application/json" \
    -d '{
      "email": "test'$i'@example.com",
      "password": "password123"
    }'
done
```

## Error Responses

### 400 Bad Request
```json
{
  "message": "Invalid email address"
}
```

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### 429 Too Many Requests
```
Too many requests. Please try again later.
```

### 500 Internal Server Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "Error details here"
}
```

## CORS Configuration

The API is configured to accept requests from:
- `http://localhost:5173` (Vite dev server)
- `https://localhost:5173` (Vite dev server with HTTPS)
- Additional origins can be configured in `appsettings.json`

## Health Checks

Two health check endpoints are available:
- `GET /health/live` - Liveness probe
- `GET /health/ready` - Readiness probe

Both endpoints are anonymous and return HTTP 200 when healthy.

## Next Steps

1. **Real JWT Implementation**: Replace mock tokens with actual JWT generation
2. **Database Integration**: Connect to PostgreSQL for user storage
3. **Azure B2C Integration**: Configure real Azure B2C tenant
4. **Enhanced Validation**: Add more sophisticated input validation
5. **Audit Logging**: Add comprehensive audit trails
6. **Refresh Tokens**: Implement token refresh mechanism

## Security Considerations

### Current Implementation
- ✅ Rate limiting prevents brute force attacks
- ✅ Security headers protect against common attacks
- ✅ Input validation prevents malformed data
- ✅ Policy-based authorization controls access
- ✅ CORS configuration limits cross-origin requests

### Production Recommendations
- 🔒 Enable HTTPS only
- 🔒 Use production-grade Azure B2C tenant
- 🔒 Implement proper JWT validation
- 🔒 Add comprehensive logging and monitoring
- 🔒 Regular security audits and penetration testing
- 🔒 Implement API versioning
- 🔒 Add request/response size limits
- 🔒 Enable SQL injection protection (when database is added)

## Troubleshooting

### Common Issues

1. **Rate Limiting Too Strict**
   - Solution: Adjust limits in `Program.cs` rate limiter configuration

2. **CORS Errors**
   - Solution: Add your frontend URL to `appsettings.json` CORS origins

3. **Authentication Failures**
   - Solution: Verify Azure B2C configuration and token format

4. **Policy Authorization Failures**
   - Solution: Ensure user claims include correct `extension_UserType` values

### Debugging

Enable detailed logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.Identity": "Debug",
      "Microsoft.AspNetCore.RateLimiting": "Information"
    }
  }
}
```

This will provide detailed information about authentication flows, policy evaluations, and rate limiting decisions.
