# API Geliştirme Prompt’ları (Copilot için)

## MediatR Command/Query + FluentValidation + Mapster

- Command örneği, Validator, Handler, Mapster mapping, ProblemDetails, idempotency-key kullanımı.

```csharp
public sealed record CreateAppointmentCommand(...);
public sealed class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand> { ... }
public sealed class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<Guid>> { ... }
```

## ProblemDetails & Idempotency

```csharp
app.MapPost("/v1/appointments", ...).AddEndpointFilterFactory(ValidationFilter.Factory);
```
