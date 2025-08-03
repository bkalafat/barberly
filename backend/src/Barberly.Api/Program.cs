using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add Azure AD B2C Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.TokenValidationParameters.NameClaimType = "name";
    }, 
    options => builder.Configuration.Bind("AzureAdB2C", options));

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

// Add CORS
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
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoints
app.MapHealthChecks("/health/live")
   .AllowAnonymous();

app.MapHealthChecks("/health/ready")
   .AllowAnonymous();

// Demo endpoints
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi()
.RequireAuthorization();

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
