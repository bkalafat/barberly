# GitHub Copilot Instructions — Barberly

Bu dosya, Copilot’un tutarlı ve üretken çalışması için **kurallar, örnekler ve şablonlar** içerir.
Hedef teknoloji yığını: **.NET 8 (API/BFF) + React 18 + TypeScript**.
Mimari ilkeler: **DDD + Clean Architecture + CQRS + Outbox + Observability-by-default**.

---

## 1) Hedef Mimari (High-Level)

```

[React (TS) SPA] --HTTP--> [BFF/API (.NET 8)]
|-- EF Core 8 --> [PostgreSQL 16]
|-- Redis ------> [Cache]
|-- Blob -------> [Saç/Salon Foto]
|-- Service Bus-> [Bildirim Kuyrukları]
'-- OTel/Logs --> [App Insights/ELK]

```

**Sınırlar**
- `Domain` — İş kuralları, entity & value object’ler, domain events.
- `Application` — CQRS (MediatR), use-case’ler, validation/mapping.
- `Infrastructure` — EF Core, Redis, Service Bus, Outbox, entegrasyonlar.
```instructions
# GitHub Copilot Instructions — Barberly

This file contains rules, examples and templates to help Copilot be consistent and productive in this repository.
Target stack: **.NET 8 (API/BFF) + React 18 + TypeScript**.
Architectural principles: **DDD + Clean Architecture + CQRS + Outbox + Observability-by-default**.

---

## 1) High-level architecture

```

[React (TS) SPA] --HTTP--> [BFF/API (.NET 8)]
|-- EF Core 8 --> [PostgreSQL 16]
|-- Redis ------> [Cache]
|-- Blob -------> [Photos (barber / shop)]
|-- Service Bus-> [Notification queues]
'-- OTel/Logs --> [App Insights / ELK]

```

Boundaries
- `Domain` — business rules, entities & value objects, domain events.
- `Application` — CQRS (MediatR), use-cases, validation/mapping.
- `Infrastructure` — EF Core, Redis, Service Bus, Outbox, integrations.
- `API` — HTTP endpoints, authZ/policies, rate limits, ProblemDetails.

Frontend principles
- Server state: **TanStack Query**, Forms: **RHF + Zod**.
- UI: **Tailwind + shadcn/ui**, i18n: **i18next**.
- API client: generated types via **openapi-typescript**.

---

## 2) Folder layout (top 2 levels)

```
barberly/
├── docs/
│   ├── prompts/
│   ├── architecture/
│   └── api/
├── backend/
│   ├── src/
│   └── tests/
├── web/
│   └── barberly-web/
├── infra/
│   ├── bicep/
   └── terraform/
├── .github/
│   └── workflows/
└── .vscode/
```

Details: (explicit paths for AI automation)

- `backend/src` projects and their .csproj paths:
  - `backend/src/Barberly.Api/Barberly.Api.csproj` (HTTP API / Minimal API + Swagger)
  - `backend/src/Barberly.Application/Barberly.Application.csproj` (CQRS/Handlers/Validators)
  - `backend/src/Barberly.Domain/Barberly.Domain.csproj` (Entities, ValueObjects, Domain Events)
  - `backend/src/Barberly.Infrastructure/Barberly.Infrastructure.csproj` (EF Core, Persistence, Outbox)
  - Solution file: `backend/src/Barberly.sln`

- `backend/tests` test projects and .csproj paths:
  - `backend/tests/Barberly.Application.Tests/Barberly.Application.Tests.csproj`
  - `backend/tests/Barberly.Domain.Tests/Barberly.Domain.Tests.csproj`
  - `backend/tests/Barberly.IntegrationTests/Barberly.IntegrationTests.csproj`

Example dotnet commands (run from repo root):

```powershell
dotnet restore backend/src/Barberly.sln
dotnet build backend/src/Barberly.sln

# Run a single test project
dotnet test backend/tests/Barberly.Domain.Tests/Barberly.Domain.Tests.csproj
dotnet test backend/tests/Barberly.Application.Tests/Barberly.Application.Tests.csproj
dotnet test backend/tests/Barberly.IntegrationTests/Barberly.IntegrationTests.csproj
```

Note: automation or Copilot prompts targeting test projects should use these exact paths under `backend/tests/`. If new test projects are added, scan `backend/tests/` for `.csproj` files.

---

## 3) Coding rules (summary)

### C# / .NET
- Nullability enabled (`<Nullable>enable</Nullable>`). Async methods use the `Async` suffix.
- Use `record` types for value objects; `sealed` classes for entities/services.
- Errors returned as `ProblemDetails`; validation errors return 400 with `traceId`.
- EF Core: Lazy Loading disabled; prefer explicit `Include`/`Select`. Migration names should be meaningful: `yyyyMMddHHmm_AddAppointment`.
- Idempotency: appointment creation uses `Idempotency-Key` header + unique composite index.
- Caching: Redis key standard — `barbers:{id}:slots:{yyyy-MM-dd}:{serviceId}`; TTL short (5–15 minutes).
- Use Options pattern and `IHttpClientFactory` for external calls.

### TypeScript / React
- Strict TypeScript (`"strict": true`), ESLint + Prettier.
- File naming: `kebab-case.tsx` for files; components in `PascalCase`.
- Hooks start with `use*`. Prefer local state or Zustand where appropriate.
- Forms: React Hook Form + Zod resolver; schema-first typing.
- Query keys example: `['barbers', barberId, 'availability', date, serviceId]`.
- Accessibility: WAI-ARIA attributes and focus management.

---

## 4) API design principles

- Versioning: `/v1/*`; OpenAPI/Swagger is generated.
- Key endpoints
  - `GET /shops?near=...&service=...`
  - `GET /barbers/{id}`
  - `GET /barbers/{id}/availability?date=...&serviceId=...`
  - `POST /appointments` (201 + `Location` header)
  - `PATCH /appointments/{id}` (cancel/reschedule)
  - `POST /hair-profiles`, `GET /hair-profiles/me`
- Pagination: `?page=&pageSize=` + `X-Total-Count` header.
- ETag/If-None-Match used for read-heavy endpoints.
- Security: OIDC (B2C/Auth0), policy-based authorization, rate limiting (token bucket).

---

## 5) Templates (for Copilot completions)

### 5.1 C# — Command/Query + Validator + Handler

```csharp
// Application/Appointments/Commands/CreateAppointmentCommand.cs
public sealed record CreateAppointmentCommand(
    Guid UserId, Guid BarberId, Guid ServiceId, Instant Start, Instant End)
    : IRequest<Result<Guid>>;

public sealed class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BarberId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.Start).LessThan(x => x.End);
    }
}

public sealed class CreateAppointmentHandler(
    IAppointmentRepository repo,
    IUnitOfWork uow,
    IIdempotencyService idem)
    : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand cmd, CancellationToken ct)
    {
        if (!await idem.EnsureNotProcessedAsync(cmd, ct))
            return Result.Fail("Duplicate");

        var appt = Appointment.Create(cmd.UserId, cmd.BarberId, cmd.ServiceId, cmd.Start, cmd.End);
        await repo.AddAsync(appt, ct);
        await uow.SaveChangesAsync(ct); // produces Outbox event

        return Result.Ok(appt.Id);
    }
}
```

### 5.2 C# — Minimal API (ProblemDetails + Idempotency)

```csharp
app.MapPost("/v1/appointments", async (
    [FromHeader(Name = "Idempotency-Key")] string idemKey,
    CreateAppointmentRequest req,
    ISender sender,
    HttpContext ctx) =>
{
    ctx.Response.Headers["Idempotency-Key"] = idemKey ?? "";
    var cmd = new CreateAppointmentCommand(req.UserId, req.BarberId, req.ServiceId, req.Start, req.End);
    var result = await sender.Send(cmd);
    return result.IsSuccess
        ? Results.Created($"/v1/appointments/{result.Value}", null)
        : Results.Problem(title: "Create failed", statusCode: StatusCodes.Status409Conflict);
})
.AddEndpointFilterFactory(ValidationFilter.Factory);
```

### 5.3 TS/React — Zod + RHF Form + Mutation

```ts
const HairProfileSchema = z.object({
  length: z.enum(['short','medium','long']),
  texture: z.enum(['straight','wavy','curly','coily']),
  density: z.enum(['low','medium','high']),
  fade: z.enum(['none','low','mid','skin']).optional(),
  guards: z.array(z.number()).max(5).optional(),
  freeText: z.string().max(500).optional()
});
type HairProfile = z.infer<typeof HairProfileSchema>;

export function HairProfileForm() {
  const { register, handleSubmit, formState: { errors } } =
    useForm<HairProfile>({ resolver: zodResolver(HairProfileSchema) });

  const mutation = useMutation({
    mutationFn: (data: HairProfile) => api.post('/v1/hair-profiles', data)
  });

  return (
    <form onSubmit={handleSubmit((d)=>mutation.mutate(d))}>
      {/* inputs … */}
      <button type="submit" disabled={mutation.isPending}>Save</button>
      {mutation.isError && <p role="alert">Save failed</p>}
    </form>
  );
}
```

### 5.4 TS/React — Availability Query

```ts
const useAvailability = (barberId: string, date: string, serviceId: string) =>
  useQuery({
    queryKey: ['barbers', barberId, 'availability', date, serviceId],
    queryFn: () => api.get(`/v1/barbers/${barberId}/availability`, { params:{ date, serviceId }})
                    .then(r => r.data),
    staleTime: 60_000
  });
```

---

## 6) Tests & quality

* Unit: Domain and handler-level tests; prefer data-free tests for edge cases.
* Integration: Testcontainers + Postgres; use real EF and API pipeline.
* Contract: Pact (web ↔ api).
* E2E: Playwright (appointment flow, hair-profile wizard).
* Load: k6 (availability + appointment).

---

## 7) Observability

* Serilog: structured logging; include `RequestId`, `UserId`, `Tenant/ShopId`.
* OpenTelemetry: traces and spans; auto-instrument HTTP clients.
* Health checks: `/health/ready`, `/health/live`.

---

## 8) Security & performance

* OIDC (B2C/Auth0), policy-based authorization; field-level encryption for PII.
* Rate limiting: burst control on public GET endpoints.
* CORS: narrow in prod; permissive in dev.
* HTTP: gzip/brotli, ETag; serve static assets via CDN where possible.

---

## 9) OpenAPI & client generation

* API: export Swagger JSON → `docs/api/openapi.yaml` as the source contract.
* Frontend client generation example:

```
npx openapi-typescript http://localhost:5000/swagger/v1/swagger.json \
  -o web/barberly-web/src/lib/api/types.ts
```

* The client `api.ts` should provide an Axios instance with interceptors (auth, retry, backoff).

---

## 10) Workflows (short)

* Appointment: query slots → `POST /appointments` (Idempotency-Key) → Outbox → Service Bus → SMS/Email → iCal.
* Hair profile: RHF + Zod → save → (v2) ParserService suggests tags → user edits.

---

## 11) Conventions

* Conventional Commits: `feat:`, `fix:`, `chore:`, `refactor:`, `test:`
* Branches: `feat/*`, `fix/*`, `docs/*`
* PR template: include scope, acceptance criteria, test coverage, rollback notes.

---

## 12) Definition of Done (MVP)

* Unit and integration tests pass, coverage ≥ 70% on critical paths.
* Swagger is up to date and examples work.
* Logs/traces are visible; health checks are green.
* Security (authZ, rate limit) is enforced; error responses use ProblemDetails.
* Documentation: README + related `docs/*` pages are updated.

```

---

### Where to put this file?
- Path: **`/.github/copilot-instructions.md`**
- Link this file from README's "Developer Guide" section.
- VS Code + Copilot will automatically consider this file when providing repo-scoped completions.

```
