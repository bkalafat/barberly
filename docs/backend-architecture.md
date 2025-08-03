# Barberly Backend Architecture

## Overview
Barberly backend is built with .NET 8 following Clean Architecture, Domain-Driven Design (DDD), and CQRS patterns. The architecture emphasizes separation of concerns, testability, and maintainability.

## Technology Stack
- **.NET 8** - Core framework
- **ASP.NET Core** - Web API framework
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **Entity Framework Core 8** - ORM and data access
- **PostgreSQL 16** - Primary database
- **Redis** - Caching and session storage
- **Azure AD B2C** - Authentication and authorization
- **Azure Service Bus** - Message queuing (planned)
- **Serilog** - Structured logging
- **OpenTelemetry** - Observability and tracing

## Project Structure

```
backend/
├── src/
│   ├── Barberly.Api/              # Web API layer (Presentation)
│   ├── Barberly.Application/      # Application layer (Use cases)
│   ├── Barberly.Domain/           # Domain layer (Business logic)
│   ├── Barberly.Infrastructure/   # Infrastructure layer (External concerns)
│   └── Barberly.sln              # Solution file
└── tests/
    ├── Barberly.UnitTests/        # Unit tests
    ├── Barberly.IntegrationTests/ # Integration tests
    └── Barberly.ArchTests/        # Architecture tests
```

## Clean Architecture Layers

### 1. Domain Layer (`Barberly.Domain`)
**Purpose**: Contains business logic, entities, value objects, and domain events.

**Key Components**:
- **Entities**: Core business objects (User, Barber, BarberShop, Appointment, Service, HairProfile)
- **Value Objects**: Immutable objects (Address, TimeSlot, Price, ContactInfo)
- **Domain Events**: Business events (AppointmentCreated, UserRegistered, SlotBooked)
- **Aggregates**: Consistency boundaries for entities
- **Repository Interfaces**: Data access contracts
- **Domain Services**: Business logic that doesn't belong to entities

**Dependencies**: None (dependency-free)

### 2. Application Layer (`Barberly.Application`)
**Purpose**: Orchestrates business workflows, implements use cases via CQRS.

**Key Components**:
- **Commands**: Write operations (CreateAppointmentCommand, RegisterUserCommand)
- **Queries**: Read operations (GetBarberAvailabilityQuery, GetUserProfileQuery)
- **Handlers**: Command/query processors using MediatR
- **Validators**: Input validation using FluentValidation
- **DTOs**: Data transfer objects for API contracts
- **Mapping**: AutoMapper profiles
- **Services**: Application services (interfaces)

**Dependencies**: Domain layer only

**CQRS Pattern**:
```csharp
// Command example
public record CreateAppointmentCommand(
    Guid UserId,
    Guid BarberId,
    Guid ServiceId,
    DateTime StartTime,
    DateTime EndTime
) : IRequest<Guid>;

// Handler example
public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Guid>
{
    // Implementation
}
```

### 3. Infrastructure Layer (`Barberly.Infrastructure`)
**Purpose**: Implements external concerns and infrastructure services.

**Key Components**:
- **Persistence**: EF Core DbContext, repositories, configurations
- **Caching**: Redis implementation
- **Messaging**: Azure Service Bus implementation
- **Storage**: Azure Blob Storage for images
- **External APIs**: Third-party service integrations
- **Identity**: Azure AD B2C integration

**Dependencies**: Application and Domain layers

### 4. API Layer (`Barberly.Api`)
**Purpose**: HTTP API endpoints, middleware, and cross-cutting concerns.

**Key Components**:
- **Controllers/Endpoints**: Minimal APIs for HTTP endpoints
- **Middleware**: Authentication, CORS, exception handling
- **Filters**: Validation, authorization filters
- **Configuration**: Dependency injection, settings

**Dependencies**: Application and Infrastructure layers

## Key Patterns and Practices

### CQRS (Command Query Responsibility Segregation)
- **Commands**: Modify state, return void or simple results
- **Queries**: Read data, return DTOs
- **Handlers**: Process commands/queries via MediatR
- **Validation**: FluentValidation for all inputs

### Repository Pattern
```csharp
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
}
```

### Unit of Work Pattern
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### Outbox Pattern
- Domain events stored in database
- Background service publishes to message queue
- Ensures eventual consistency

## Authentication & Authorization

### Azure AD B2C Integration
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.TokenValidationParameters.NameClaimType = "name";
    },
    options => builder.Configuration.Bind("AzureAdB2C", options));
```

### Authorization Policies
- **CustomerPolicy**: `extension_UserType = "Customer"`
- **BarberPolicy**: `extension_UserType = "Barber"`
- **ShopOwnerPolicy**: `extension_UserType = "ShopOwner"`
- **AdminPolicy**: `extension_UserType = "Admin"`

## API Design

### RESTful Endpoints
```
# Authentication
POST /auth/register
POST /auth/login

# Users
GET /users/me
PUT /users/me

# Barbers
GET /barbers
GET /barbers/{id}
GET /barbers/{id}/availability

# Appointments
GET /appointments
POST /appointments
GET /appointments/{id}
PATCH /appointments/{id}
DELETE /appointments/{id}

# Hair Profiles
GET /hair-profiles/me
POST /hair-profiles
PUT /hair-profiles/{id}
```

### API Standards
- **Versioning**: `/v1/` prefix
- **Pagination**: `?page=1&pageSize=20`
- **Filtering**: `?status=active&date=2024-01-01`
- **Sorting**: `?orderBy=createdAt&orderDirection=desc`
- **Response Format**: JSON with consistent structure
- **Error Handling**: RFC 7807 Problem Details

### Minimal API Example
```csharp
app.MapPost("/v1/appointments", async (
    [FromBody] CreateAppointmentRequest request,
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
    ISender sender) =>
{
    var command = new CreateAppointmentCommand(
        request.UserId,
        request.BarberId,
        request.ServiceId,
        request.StartTime,
        request.EndTime);

    var result = await sender.Send(command);
    return Results.Created($"/v1/appointments/{result}", result);
})
.RequireAuthorization("CustomerPolicy")
.WithOpenApi();
```

## Data Management

### Entity Framework Core
- **Code-First**: Migrations for schema changes
- **Lazy Loading**: Disabled for performance
- **Change Tracking**: Optimized for read-heavy scenarios
- **Connection Resiliency**: Retry policies configured

### Database Design
- **PostgreSQL**: Primary database
- **Naming**: snake_case for database objects
- **Indexing**: Strategic indexes for query performance
- **Constraints**: Foreign keys, unique constraints

### Caching Strategy
- **Redis**: Distributed caching
- **Cache Keys**: Standardized format (`barbers:{id}:slots:{date}`)
- **TTL**: Short-lived cache (5-15 minutes)
- **Cache-Aside**: Pattern for data retrieval

## Observability

### Logging (Serilog)
- **Structured Logging**: JSON format
- **Correlation IDs**: Request tracing
- **Log Levels**: Appropriate verbosity
- **Sensitive Data**: Masked in logs

### Monitoring (OpenTelemetry)
- **Distributed Tracing**: Request flows
- **Metrics**: Performance counters
- **Health Checks**: `/health/ready`, `/health/live`

## Security

### Input Validation
- **FluentValidation**: All command/query inputs
- **Model Binding**: ASP.NET Core validation
- **Sanitization**: XSS prevention

### Security Headers
- **CORS**: Configured origins
- **HSTS**: HTTPS enforcement
- **Content Security Policy**: XSS protection

### Rate Limiting
- **Token Bucket**: Algorithm implementation
- **Per-User Limits**: Authenticated users
- **Global Limits**: Anonymous users

## Testing Strategy

### Unit Tests
- **Domain Logic**: Business rules testing
- **Application Handlers**: Use case testing
- **Validators**: Input validation testing

### Integration Tests
- **API Endpoints**: Full request/response testing
- **Database**: EF Core integration testing
- **Authentication**: JWT token validation

### Architecture Tests
- **Dependency Rules**: Layer dependency validation
- **Naming Conventions**: Consistent naming
- **Attribute Usage**: Required attributes validation

## Development Guidelines

### Coding Standards
- **Nullable Reference Types**: Enabled
- **Async/Await**: All I/O operations
- **Record Types**: For immutable data
- **ConfigureAwait(false)**: Library code
- **CancellationToken**: All async methods

### Error Handling
- **Global Exception Handler**: Centralized error handling
- **Problem Details**: RFC 7807 compliance
- **Logging**: Comprehensive error logging
- **User-Friendly Messages**: Client-appropriate responses

### Performance
- **Async Programming**: Non-blocking I/O
- **Connection Pooling**: Database connections
- **Memory Management**: Minimal allocations
- **Query Optimization**: Efficient database queries

## Deployment

### CI/CD Pipeline
- **GitHub Actions**: Automated build and test
- **Docker**: Containerized deployment
- **Azure App Service**: Production hosting
- **Environment Variables**: Configuration management

### Infrastructure as Code
- **Bicep/Terraform**: Azure resource provisioning
- **Environment Separation**: Dev, staging, production
- **Secret Management**: Azure Key Vault integration

## Future Enhancements

### Planned Features
- **Event Sourcing**: For audit trails
- **CQRS Read Models**: Optimized queries
- **Microservices**: Domain-based service splitting
- **GraphQL**: Flexible query interface
- **Webhook Support**: Real-time notifications

---

> This document serves as the single source of truth for Barberly backend architecture. Keep it updated as the system evolves.
