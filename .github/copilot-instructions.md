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
- `API` — HTTP uçları, authZ/policy, rate limit, ProblemDetails.

**Frontend ilkeleri**
- Server state: **TanStack Query**, Form: **RHF + Zod**.
- UI: **Tailwind + shadcn/ui**, i18n: **i18next**.
- API istemcisi: **OpenAPI generator** ile tip güvenli.

---

## 2) Klasör Yapısı (2 seviye)

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
│   └── terraform/
├── .github/
│   └── workflows/
└── .vscode/

````

> Detaylar:
> `backend/src` → `Barberly.Api`, `Barberly.Application`, `Barberly.Domain`, `Barberly.Infrastructure`
> `backend/tests` → `Barberly.UnitTests`, `Barberly.IntegrationTests`
> `docs/api` → `openapi.yaml` (kaynak sözleşme)

---

## 3) Kodlama Kuralları (Özet)

### C# / .NET
- **Nullability açık** (`<Nullable>enable</Nullable>`), **async** metotlarda `Async` soneki.
- **Records**: value-object’ler için; **sealed classes**: entity/service.
- **ProblemDetails** ile hata yanıtı; doğrulama hataları 400, `traceId` döndür.
- **EF Core**: Lazy Loading **kapalı**; spesifik `Include`/`Select`; **migration adları** anlamlı: `yyyyMMddHHmm_AddAppointment`.
- **Idempotency**: Randevu oluşturma `Idempotency-Key` header + unique composite index.
- **Caching**: Redis key standardı — `barbers:{id}:slots:{yyyy-MM-dd}:{serviceId}`; TTL kısa (5–15 dk).
- **Options pattern** ve **IHttpClientFactory** kullan.

### TypeScript / React
- **Strict TypeScript** (`"strict": true`), **ESLint** + **Prettier**.
- **Dosya adları**: `kebab-case.tsx`, bileşenler `PascalCase`.
- **Hooks** `use*` ile başlar; durum yönetiminde local state veya **Zustand**.
- **Form**: RHF + Zod resolver; şema kaynaklı tip üret.
- **Query** adları: `['barbers', barberId, 'availability', date, serviceId]`.
- **Erişilebilirlik**: WAI-ARIA, `aria-*` ve odak durumları.

---

## 4) API Tasarım İlkeleri

- **Versiyonlama**: `/v1/*`; **OpenAPI/Swagger** üretilir.
- **Kaynaklar**
  - `GET /shops?near=...&service=...`
  - `GET /barbers/{id}`
  - `GET /barbers/{id}/availability?date=...&serviceId=...`
  - `POST /appointments` (201 + `Location` header)
  - `PATCH /appointments/{id}` (cancel/reschedule)
  - `POST /hair-profiles`, `GET /hair-profiles/me`
- **Pagination**: `?page=&pageSize=` + `X-Total-Count` header.
- **ETag/If-None-Match**: read-heavy uçlarda aç.
- **Security**: OIDC (B2C/Auth0), policy-based authZ, **RateLimit** (token bucket).

---

## 5) Şablonlar (Copilot Tamamlama İçin)

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
        await uow.SaveChangesAsync(ct); // Outbox event üretir

        return Result.Ok(appt.Id);
    }
}
````

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
      <button type="submit" disabled={mutation.isPending}>Kaydet</button>
      {mutation.isError && <p role="alert">Kaydedilemedi</p>}
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

## 6) Test & Kalite

* **Unit**: Domain ve Handler seviyesinde; edge-case’ler için veriye bağlı olmayan testler.
* **Integration**: Testcontainers + Postgres; gerçek EF ve API pipeline.
* **Contract**: Pact (web ↔ api).
* **E2E**: Playwright (randevu akışı, saç profili sihirbazı).
* **Load**: k6 (availability + appointment).

---

## 7) Observability

* **Serilog**: yapılandırılmış log; `RequestId`, `UserId`, `Tenant/ShopId`.
* **OpenTelemetry**: trace-id, spans; HTTP client’larda otomatik instrumentation.
* **Health Checks**: `/health/ready`, `/health/live`.

---

## 8) Güvenlik ve Performans

* **OIDC** (B2C/Auth0), policy-based authZ; PII alanlar için **field-level encryption**.
* **Rate Limiting**: public GET uçlarında burst kontrolü.
* **CORS**: sadece gerekli origin’ler; dev’de geniş, prod’da kısıtlı.
* **HTTP**: gzip/brotli, ETag; CDN üzerinden statik dosyalar.

---

## 9) OpenAPI & İstemci Üretimi

* API: Swagger JSON → `docs/api/openapi.yaml` kaynağına export et.
* Frontend:

  ```
  npx openapi-typescript http://localhost:5000/swagger/v1/swagger.json \
    -o web/barberly-web/src/lib/api/types.ts
  ```
* İstemci `api.ts`: Axios instance + interceptors (auth, retry, RFC 6585 backoff).

---

## 10) İş Akışları (Kısa)

* **Randevu**: slot sorgula → `POST /appointments` (Idempotency-Key) → Outbox → Service Bus → SMS/Email → iCal.
* **Saç Profili**: RHF + Zod → kaydet → (v2) ParserService ile tag öner → kullanıcı düzeltmesi.

---

## 11) Konvansiyonlar

* **Conventional Commits**: `feat:`, `fix:`, `chore:`, `refactor:`, `test:`
* **Branch’ler**: `feat/*`, `fix/*`, `docs/*`
* **PR Şablonu**: kapsam, kabul kriteri, test kapsamı, risk/rollback.

---

## 12) Definition of Done (MVP)

* Birim ve entegrasyon testleri geçti, kapsam ≥ %70 (kritik yol).
* Swagger güncel; endpoint örnekleri çalışır.
* Log/trace’ler görünüyor; health checks yeşil.
* Güvenlik (authZ, rate limit) aktif; hata yanıtları ProblemDetails.
* Dokümantasyon: README + ilgili `docs/*` sayfaları güncel.

```

---

### Bu dosyayı nereye koymalısın?
- Yol: **`/.github/copilot-instructions.md`**
- `README.md` içinde “Geliştirme Rehberi” bölümünden bu dosyaya link ver.
- VS Code + Copilot eklentisi bu dosyayı repo bağlamında otomatik dikkate alır.
