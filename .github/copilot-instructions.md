# GitHub Copilot Instructions — Barberly

AI assistant instructions for the Barberly barber shop management platform. This document provides essential knowledge for immediate productivity in this codebase.

**Stack**: .NET 8 + React 18 + TypeScript  
**Architecture**: DDD + Clean Architecture + CQRS + Minimal APIs

---

## 🏗️ Architecture Overview

```
[React 18 + TypeScript SPA] --HTTP--> [.NET 8 Minimal APIs]
                                      |-- EF Core 8 --> PostgreSQL 16
                                      |-- Distributed Cache --> Redis / In-Memory
                                      |-- [PLANNED] Service Bus --> Azure Service Bus
                                      '-- [PLANNED] OpenTelemetry --> Observability
```

**Layer Boundaries**:

- `Domain` (`Barberly.Domain`) — Entities, value objects, domain events
- `Application` (`Barberly.Application`) — CQRS handlers, validation, DTOs
- `Infrastructure` (`Barberly.Infrastructure`) — EF Core, persistence, outbox
- `API` (`Barberly.Api`) — Minimal APIs, authentication, rate limiting

---

## 📁 Key Project Structure

### Backend (.NET 8)

```
backend/src/
├── Barberly.sln                    # Main solution file
├── Barberly.Api/                   # HTTP API with Minimal APIs
│   ├── Endpoints/                  # Endpoint groups (Directory, Scheduling)
│   ├── Program.cs                  # DI container, middleware pipeline
│   └── Models/                     # Request/response models
├── Barberly.Application/           # CQRS handlers & business logic
│   ├── Directory/                  # Barber, Shop, Service features
│   │   ├── Commands/               # Write operations
│   │   ├── Queries/                # Read operations
│   │   └── Handlers/               # MediatR handlers
│   └── Interfaces/                 # Repository contracts
├── Barberly.Domain/                # Core business entities
│   └── Entities/                   # Barber, BarberShop, Service, etc.
└── Barberly.Infrastructure/        # Data access & external services
    └── Persistence/                # EF Core repositories

backend/tests/
├── Barberly.IntegrationTests/      # WebApplicationFactory tests
├── Barberly.Application.Tests/     # Handler unit tests
└── Barberly.Domain.Tests/          # Entity unit tests
```

### Frontend (React + TypeScript)

```
web/barberly-web/
├── package.json                    # Dependencies & scripts
├── src/
│   ├── components/ui/              # shadcn/ui components (Button, Card, etc.)
│   ├── pages/                      # Route components (HomePage, BarbersPage)
│   ├── lib/
│   │   ├── api/                    # API client & hooks
│   │   │   ├── client.ts           # Axios configuration
│   │   │   └── hooks.ts            # TanStack Query hooks
│   │   └── utils.ts                # Utilities (cn helper, etc.)
│   └── App.tsx                     # Router setup & providers
├── .storybook/                     # Storybook configuration
└── vitest.config.ts                # Test configuration
```

---

## 💻 Essential Development Commands

### From Repository Root

```bash
# Frontend
npm run dev                         # Start React dev server
npm run lint                        # ESLint check
npm run type-check                  # TypeScript validation
npm run test                        # Run Vitest tests

# Backend
npm run backend:build               # Build .NET solution
npm run backend:run                 # Start API server (localhost:5156)
npm run backend:test                # Run all .NET tests
```

### .NET Specific Commands

```bash
# From backend/src/
dotnet build Barberly.sln
dotnet run --project Barberly.Api/Barberly.Api.csproj

# Entity Framework migrations
dotnet ef migrations add [Name] --project Barberly.Infrastructure --startup-project Barberly.Api --context BarberlyDbContext
dotnet ef database update --project Barberly.Infrastructure --startup-project Barberly.Api
```

---

## 🛠️ Code Patterns & Conventions

### Backend: CQRS Pattern

**Command Example:**

```csharp
// Application/Directory/Commands/DirectoryCommands.cs
public record CreateBarberCommand(
    string FullName,
    string Email,
    string Phone,
    Guid BarberShopId,
    int YearsOfExperience,
    string? Bio) : IRequest<Guid>;

// Handler in Application/Directory/Handlers/DirectoryCommandHandlers.cs
public class CreateBarberCommandHandler : IRequestHandler<CreateBarberCommand, Guid>
{
    private readonly IBarberRepository _barberRepository;

    public async Task<Guid> Handle(CreateBarberCommand request, CancellationToken ct)
    {
        var barber = Barber.Create(
            request.FullName,
            request.Email,
            request.Phone,
            request.BarberShopId,
            request.YearsOfExperience,
            request.Bio);

        await _barberRepository.AddAsync(barber, ct);
        return barber.Id;
    }
}
```

**Query Example:**

```csharp
// Application/Directory/Queries/DirectoryQueries.cs
public record GetBarbersByShopIdQuery(
    Guid BarberShopId,
    int Page = 1,
    int PageSize = 20) : IRequest<List<BarberDto>>;

// Handler returns DTOs, never domain entities
public async Task<List<BarberDto>> Handle(GetBarbersByShopIdQuery request, CancellationToken ct)
{
    var barbers = await _barberRepository.GetByShopIdAsync(
        request.BarberShopId,
        request.Page,
        request.PageSize,
        ct);
    return barbers.Select(x => x.ToDto()).ToList();
}
```

### Minimal API Pattern

**Endpoint Registration:**

```csharp
// Barberly.Api/Endpoints/DirectoryEndpoints.cs
public static void MapDirectoryEndpoints(this WebApplication app)
{
    var directory = app.MapGroup("/api/v1").WithTags("Directory");

    directory.MapPost("/barbers", CreateBarber)
        .WithName("CreateBarber")
        .WithOpenApi()
        .RequireAuthorization();

    directory.MapGet("/barbers", GetBarbers)
        .WithName("GetBarbers")
        .WithOpenApi()
        .AllowAnonymous();
}

// Handler delegates to MediatR
static async Task<IResult> CreateBarber(
    CreateBarberCommand command,
    ISender sender)
{
    var result = await sender.Send(command);
    return Results.Created($"/api/v1/barbers/{result}", result);
}
```

### Frontend: TanStack Query + TypeScript

**API Hooks Pattern:**

```typescript
// src/lib/api/hooks.ts
export const useBarbers = (params?: { barberShopId?: string }) => {
  return useQuery({
    queryKey: ["barbers", params],
    queryFn: () => barbersApi.getAll(params).then((res) => res.data),
    staleTime: 5 * 60_000, // 5 minutes
  });
};

export const useCreateAppointment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (
      data: CreateAppointmentRequest & { idempotencyKey: string }
    ) => {
      const { idempotencyKey, ...appointmentData } = data;
      return appointmentsApi.create(appointmentData, idempotencyKey);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries([
        "barbers",
        variables.barberId,
        "availability",
      ]);
    },
  });
};
```

**Component Pattern (shadcn/ui + React Hook Form):**

```typescript
// src/pages/BarbersPage.tsx
export function BarbersPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const { data: barbers, isLoading } = useBarbers();

  const filteredBarbers = barbers?.filter((barber) =>
    barber.fullName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="container mx-auto py-8">
      <div className="mb-6">
        <Input
          placeholder="Search barbers..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="max-w-sm"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredBarbers?.map((barber) => (
          <Card key={barber.id}>
            <CardHeader>
              <CardTitle>{barber.fullName}</CardTitle>
              <CardDescription>{barber.bio}</CardDescription>
            </CardHeader>
          </Card>
        ))}
      </div>
    </div>
  );
}
```

---

## 🔐 Authentication & Authorization

**Current Setup**: JWT-based authentication with role-based policies

- **Policies**: `CustomerPolicy`, `BarberPolicy`, `ShopOwnerPolicy`, `AdminPolicy`
- **Rate Limiting**: 5 requests/min for auth endpoints, 100/min global
- **Security Headers**: XSS protection, CORS configured for frontend origins

**Authentication Flow:**

1. `POST /auth/register` - User registration with role validation
2. `POST /auth/login` - Returns JWT token
3. API endpoints use `RequireAuthorization()` or specific policies
4. Frontend stores token, adds to requests via Axios interceptors

---

## 🧪 Testing Conventions

### Integration Tests (Backend)

```csharp
// backend/tests/Barberly.IntegrationTests/
public class DirectoryEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    [Fact]
    public async Task GetBarbers_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/v1/barbers");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateBarber_WithoutAuth_ShouldReturnUnauthorized()
    {
        var request = new CreateBarberRequest(/* ... */);
        var response = await _client.PostAsJsonAsync("/api/v1/barbers", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

### Frontend Tests (Vitest + Testing Library)

```typescript
// src/components/__tests__/Button.test.tsx
import { render, screen } from "@testing-library/react";
import { Button } from "../ui/button";

test("renders button with text", () => {
  render(<Button>Click me</Button>);
  expect(screen.getByRole("button", { name: /click me/i })).toBeInTheDocument();
});
```

---

## 🎯 Key Integration Points

### API Client Configuration

```typescript
// src/lib/api/client.ts
export const api = axios.create({
  baseURL: "http://localhost:5156",
  timeout: 10000,
});

// Request interceptor adds auth token
api.interceptors.request.use((config) => {
  const token = getAuthToken(); // From auth context
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
```

### Database Seeding

- `DatabaseSeeder.SeedAsync()` in `Program.cs` creates sample data
- Includes test users, barber shops, barbers, and services
- Ahmet Yılmaz (barber ID: `9e862653-65c0-41b0-82e9-754f638baa49`) is primary test barber

### Key API Endpoints

```
# Directory Management
GET    /api/v1/shops                    # List barber shops
GET    /api/v1/shops/{id}               # Shop details
POST   /api/v1/shops                    # Create shop (auth required)
GET    /api/v1/barbers                  # List barbers
GET    /api/v1/barbers/{id}             # Barber details
GET    /api/v1/services                 # List services

# Scheduling
GET    /api/v1/barbers/{id}/availability  # Available time slots
POST   /api/v1/appointments               # Book appointment (with Idempotency-Key)
GET    /api/v1/appointments/{id}          # Appointment details
DELETE /api/v1/appointments/{id}          # Cancel appointment

# Authentication
POST   /auth/register                     # User registration
POST   /auth/login                        # User login
GET    /me                               # Current user info (auth required)

# Health Checks
GET    /health/live                      # Liveness probe
GET    /health/ready                     # Readiness probe
```

---

## 🚨 Important Notes

1. **Idempotency**: Appointment creation requires `Idempotency-Key` header
2. **Time Zones**: All appointment times are stored in UTC
3. **EF Migrations**: Always use meaningful migration names with timestamp prefix
4. **Query Keys**: Follow pattern `['entity', ...params]` for cache invalidation
5. **Error Handling**: API returns RFC 7807 ProblemDetails for errors
6. **CORS**: Frontend dev server (localhost:5173) is allowed origin

---

This file should be referenced when making changes to maintain architectural consistency and follow established patterns.
