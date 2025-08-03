# Copilot API Geliştirme Prompt'ları

Bu dosya, Barberly API geliştirme sürecinde Copilot'un tutarlı ve verimli kod üretmesi için şablonlar içerir.

## MediatR Command/Query + FluentValidation + Handler Pattern

### Command Şablonu
```csharp
// Application/[Module]/Commands/[Action]Command.cs
public sealed record CreateAppointmentCommand(
    Guid UserId,
    Guid BarberId,
    Guid ServiceId,
    Instant Start,
    Instant End)
    : IRequest<Result<Guid>>;

public sealed class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BarberId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.Start)
            .LessThan(x => x.End)
            .WithMessage("Start time must be before end time");
    }
}

public sealed class CreateAppointmentHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IIdempotencyService idempotencyService,
    ILogger<CreateAppointmentHandler> logger)
    : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand command, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(CreateAppointmentHandler));

        // Idempotency check
        if (!await idempotencyService.EnsureNotProcessedAsync(command, cancellationToken))
        {
            logger.LogWarning("Duplicate appointment creation attempt for {UserId}", command.UserId);
            return Result.Fail("Duplicate request");
        }

        // Domain logic
        var appointment = Appointment.Create(
            command.UserId,
            command.BarberId,
            command.ServiceId,
            command.Start,
            command.End);

        await repository.AddAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken); // Triggers Outbox events

        logger.LogInformation("Appointment {AppointmentId} created for user {UserId}",
            appointment.Id, command.UserId);

        return Result.Ok(appointment.Id);
    }
}
```

## Minimal API Endpoint Pattern

```csharp
// Extensions/EndpointExtensions.cs
public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/appointments")
            .WithTags("Appointments")
            .WithOpenApi();

        group.MapPost("", CreateAppointment)
            .AddEndpointFilterFactory(ValidationFilter.Factory)
            .AddEndpointFilterFactory(IdempotencyFilter.Factory);

        group.MapGet("{id:guid}", GetAppointment);
        group.MapPatch("{id:guid}", UpdateAppointment);
    }

    private static async Task<IResult> CreateAppointment(
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CreateAppointmentRequest request,
        ISender sender,
        HttpContext context)
    {
        context.Response.Headers["Idempotency-Key"] = idempotencyKey ?? "";

        var command = new CreateAppointmentCommand(
            request.UserId,
            request.BarberId,
            request.ServiceId,
            request.Start,
            request.End);

        var result = await sender.Send(command);

        return result.IsSuccess
            ? Results.Created($"/v1/appointments/{result.Value}", null)
            : Results.Problem(
                title: "Appointment creation failed",
                statusCode: StatusCodes.Status409Conflict,
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = Activity.Current?.Id
                });
    }
}
```

## ProblemDetails & Idempotency

```csharp
app.MapPost("/v1/appointments", ...).AddEndpointFilterFactory(ValidationFilter.Factory);
```Prompt’ları (Copilot için)

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
