using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using System.Security.Claims;
using Barberly.Api.Endpoints;
using FluentValidation;
using System.Reflection;
using Barberly.Api.Models;
using Barberly.Api.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Barberly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore; // <--- Added using directive

var builder = WebApplication.CreateBuilder(args);
// EF Core DbContext (PostgreSQL)
builder.Services.AddDbContext<Barberly.Infrastructure.Persistence.BarberlyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Not: appsettings.json'da DefaultConnection eklenmeli
// Dependency Injection: Repository interface-implementation bindings
builder.Services.AddScoped<Barberly.Application.Interfaces.IBarberShopRepository, Barberly.Infrastructure.Persistence.BarberShopRepository>();
builder.Services.AddScoped<Barberly.Application.Interfaces.IBarberRepository, Barberly.Infrastructure.Persistence.BarberRepository>();
builder.Services.AddScoped<Barberly.Application.Interfaces.IServiceRepository, Barberly.Infrastructure.Persistence.ServiceRepository>();
builder.Services.AddScoped<Barberly.Application.Interfaces.IUserRepository, Barberly.Infrastructure.Persistence.UserRepository>();
builder.Services.AddScoped<Barberly.Application.Interfaces.IAppointmentRepository, Barberly.Infrastructure.Persistence.AppointmentRepository>();
builder.Services.AddScoped<Barberly.Application.Interfaces.INotificationOutboxRepository, Barberly.Infrastructure.Persistence.NotificationOutboxRepository>();

// Add notification services
builder.Services.AddScoped<Barberly.Application.Notifications.Interfaces.IEmailService, Barberly.Infrastructure.Services.SmtpEmailService>();
builder.Services.AddScoped<Barberly.Application.Notifications.Interfaces.IEmailTemplateService, Barberly.Infrastructure.Services.EmailTemplateService>();

// Add services
builder.Services.AddScoped<Barberly.Application.Interfaces.ITokenService, MockJwtService>();
builder.Services.AddScoped<Barberly.Application.Interfaces.IPasswordHasher, PasswordHasher>();

// Add Authentication
if (builder.Environment.IsDevelopment())
{
    // Development: Use mock JWT tokens for testing
    var mockSecretKey = "this-is-a-very-long-secret-key-for-testing-purposes-only-do-not-use-in-production";
    var key = Encoding.ASCII.GetBytes(mockSecretKey);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "https://barberly-dev.b2clogin.com/mock-tenant-id/v2.0/",
                ValidateAudience = true,
                ValidAudience = "mock-api-client-id",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = "extension_UserType"
            };
        });
}
else
{
    // Production: Use Azure AD B2C
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(options =>
        {
            builder.Configuration.Bind("AzureAdB2C", options);
            options.TokenValidationParameters.NameClaimType = "name";
        },
        options => builder.Configuration.Bind("AzureAdB2C", options));
}

// Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    // Default policy: authenticated users
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Policy for customers
    options.AddPolicy("CustomerPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("extension_UserType", "Customer"));

    // Policy for barbers
    options.AddPolicy("BarberPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("extension_UserType", "Barber"));

    // Policy for shop owners
    options.AddPolicy("ShopOwnerPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("extension_UserType", "ShopOwner"));

    // Policy for admins
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("extension_UserType", "Admin"));
});

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    // Always register handlers from Barberly.Application by type
    cfg.RegisterServicesFromAssemblyContaining<Barberly.Application.Directory.Handlers.GetBarberShopsQueryHandler>();
    // Register domain event handlers from Barberly.Domain
    cfg.RegisterServicesFromAssemblyContaining<Barberly.Domain.Events.AppointmentBookedEvent>();
});

// Add Redis distributed cache (optional - development config in appsettings.Development.json)
var redisConn = builder.Configuration.GetConnectionString("Redis") ?? builder.Configuration.GetSection("ConnectionStrings")["Redis"];
// If Redis points to localhost or 127.0.0.1 and it's not explicitly configured in the environment,
// prefer the in-memory distributed cache for tests/local dev to avoid hard dependency on a running Redis instance.
if (!string.IsNullOrEmpty(redisConn) && !(redisConn.Contains("localhost", StringComparison.OrdinalIgnoreCase) || redisConn.Contains("127.0.0.1")))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
    });
}
else
{
    // Use in-memory distributed cache for local development / tests (safer when Redis not running locally)
    builder.Services.AddDistributedMemoryCache();
}

// Add FluentValidation
var appAssembly = AppDomain.CurrentDomain.GetAssemblies()
    .FirstOrDefault(a => a.GetName().Name == "Barberly.Application");
if (appAssembly != null)
{
    builder.Services.AddValidatorsFromAssembly(appAssembly);
}// Add CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Barberly API", Version = "v1" });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add background workers for email notifications
builder.Services.AddHostedService<Barberly.Infrastructure.Workers.EmailNotificationWorker>();
builder.Services.AddHostedService<Barberly.Infrastructure.Workers.AppointmentReminderWorker>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add security headers
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

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoints
app.MapHealthChecks("/health/live")
   .AllowAnonymous();

app.MapHealthChecks("/health/ready")
   .AllowAnonymous();

// Test authentication endpoint
app.MapGet("/me", (ClaimsPrincipal user) =>
{
    var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToArray();
    return new
    {
        UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        Name = user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("name")?.Value,
        Email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("emails")?.Value,
        UserType = user.FindFirst("extension_UserType")?.Value,
        Claims = claims
    };
})
.WithName("GetCurrentUser")
.WithOpenApi()
.RequireAuthorization();

// Test policy endpoints
app.MapGet("/customer-only", () => "This endpoint is for customers only")
   .WithName("CustomerOnly")
   .WithOpenApi()
   .RequireAuthorization("CustomerPolicy");

app.MapGet("/barber-only", () => "This endpoint is for barbers only")
   .WithName("BarberOnly")
   .WithOpenApi()
   .RequireAuthorization("BarberPolicy");

app.MapGet("/shop-owner-only", () => "This endpoint is for shop owners only")
   .WithName("ShopOwnerOnly")
   .WithOpenApi()
   .RequireAuthorization("ShopOwnerPolicy");

app.MapGet("/admin-only", () => "This endpoint is for admins only")
   .WithName("AdminOnly")
   .WithOpenApi()
   .RequireAuthorization("AdminPolicy");

// Directory endpoints
app.MapDirectoryEndpoints();
// Scheduling endpoints
app.MapSchedulingEndpoints();

// Seed database with sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BarberlyDbContext>();
    await DatabaseSeeder.SeedAsync(context);
}

// Auth endpoints with simplified approach (to be enhanced later)
app.MapPost("/auth/register", async (RegisterRequest request, Barberly.Application.Interfaces.IUserRepository userRepo, Barberly.Application.Interfaces.IPasswordHasher hasher) =>
{
    try
    {
        if (string.IsNullOrEmpty(request.Email) || !request.Email.Contains("@"))
            return Results.BadRequest(new { message = "Invalid email address" });

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6)
            return Results.BadRequest(new { message = "Password must be at least 6 characters" });

        if (string.IsNullOrEmpty(request.FullName))
            return Results.BadRequest(new { message = "Full name is required" });

        if (request.Role != "customer" && request.Role != "barber")
            return Results.BadRequest(new { message = "Role must be either 'customer' or 'barber'" });

        var existing = await userRepo.GetByEmailAsync(request.Email);
        if (existing != null)
            return Results.BadRequest(new { message = "User already exists" });

        var user = new Barberly.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = hasher.HashPassword(request.Password),
            FullName = request.FullName,
            Role = request.Role.ToLower(),
            CreatedAt = DateTime.UtcNow
        };
        await userRepo.AddAsync(user);
        return Results.Created($"/auth/register/{user.Id}", new { userId = user.Id, message = "User registered successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("RegisterUser")
.WithOpenApi()
.AllowAnonymous();

app.MapPost("/auth/login", async (LoginRequest request, Barberly.Application.Interfaces.IUserRepository userRepo, Barberly.Application.Interfaces.IPasswordHasher hasher, Barberly.Application.Interfaces.ITokenService jwtService) =>
{
    try
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return Results.BadRequest(new { message = "Email and password are required" });

        var user = await userRepo.GetByEmailAsync(request.Email);
        if (user == null)
            return Results.BadRequest(new { message = "Invalid credentials" });

        if (!hasher.VerifyPassword(request.Password, user.PasswordHash))
            return Results.BadRequest(new { message = "Invalid credentials" });

        var token = jwtService.GenerateToken(user.Email, user.Role, user.Id.ToString());

        return Results.Ok(new
        {
            token,
            message = "Login successful",
            user = new
            {
                id = user.Id,
                email = user.Email,
                role = user.Role
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("LoginUser")
.WithOpenApi()
.AllowAnonymous();

// Helper endpoint for generating test tokens
app.MapPost("/auth/test-token", (string email, string role, Barberly.Application.Interfaces.ITokenService jwtService) =>
{
    var userId = Guid.NewGuid().ToString();
    var token = jwtService.GenerateToken(email, role, userId);

    return Results.Ok(new
    {
        token,
        instructions = "Copy this token and use it in Swagger UI Authorization header as: Bearer {token}"
    });
})
.WithName("GenerateTestToken")
.WithOpenApi()
.AllowAnonymous();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
