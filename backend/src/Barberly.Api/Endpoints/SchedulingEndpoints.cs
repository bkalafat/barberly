using Barberly.Application.Interfaces;
using Barberly.Domain.Entities;
using Barberly.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Barberly.Api.Endpoints;

public static class SchedulingEndpoints
{
    public static void MapSchedulingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Scheduling");

        group.MapGet("/barbers/{id:guid}/availability", GetAvailability)
            .WithName("GetBarberAvailability")
            .WithOpenApi()
            .AllowAnonymous();

        group.MapPost("/appointments", CreateAppointment)
            .WithName("CreateAppointment")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/appointments/{id:guid}", GetAppointment)
            .WithName("GetAppointment")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapDelete("/appointments/{id:guid}", CancelAppointment)
            .WithName("CancelAppointment")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPatch("/appointments/{id:guid}", RescheduleAppointment)
            .WithName("RescheduleAppointment")
            .WithOpenApi()
            .RequireAuthorization();
    }

    private static async Task<IResult> GetAvailability(Guid id, [FromQuery] DateTime? date, [FromQuery] Guid? serviceId, IAppointmentRepository apptRepo, IServiceRepository serviceRepo, IBarberRepository barberRepo, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
        // Check if barber exists
        var barber = await barberRepo.GetByIdAsync(id);
        if (barber == null)
            return Results.NotFound(new { message = "Barber not found" });

        var targetDate = date?.Date ?? DateTime.UtcNow.Date;
        var serviceDuration = 30; // default
        if (serviceId.HasValue)
        {
            var svc = await serviceRepo.GetByIdAsync(serviceId.Value);
            if (svc != null) serviceDuration = svc.DurationInMinutes;
        }

        // Use UTC times for storage; in the future convert by shop timezone
        var startOfDay = new DateTimeOffset(targetDate.Year, targetDate.Month, targetDate.Day, 9, 0, 0, TimeSpan.Zero);
        var endOfDay = new DateTimeOffset(targetDate.Year, targetDate.Month, targetDate.Day, 17, 0, 0, TimeSpan.Zero);

        var cacheKey = $"barbers:{id}:slots:{targetDate:yyyy-MM-dd}:{serviceId?.ToString() ?? "default"}";
        string? cached = null;
        try
        {
            cached = await cache.GetStringAsync(cacheKey);
        }
        catch (Exception)
        {
            // swallow cache errors (e.g. Redis not available) and continue generating slots
            cached = null;
        }
        if (!string.IsNullOrEmpty(cached))
        {
            // Cached JSON of slots
            return Results.Ok(System.Text.Json.JsonSerializer.Deserialize<object[]>(cached) ?? Array.Empty<object>());
        }

        var slots = new List<object>();
        var slotStart = startOfDay;
        while (slotStart.AddMinutes(serviceDuration) <= endOfDay)
        {
            var slotEnd = slotStart.AddMinutes(serviceDuration);
            var conflicts = await apptRepo.GetByBarberAndRangeAsync(id, slotStart, slotEnd);
            if (!conflicts.Any())
            {
                slots.Add(new { start = slotStart, end = slotEnd });
            }
            slotStart = slotStart.AddMinutes(serviceDuration);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(slots);
        var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
        try
        {
            await cache.SetStringAsync(cacheKey, json, opts);
        }
        catch (Exception)
        {
            // ignore cache write errors
        }
        return Results.Ok(slots);
    }
    private static async Task<IResult> CreateAppointment([FromHeader(Name = "Idempotency-Key")] string? idemKey, [FromBody] CreateAppointmentRequest req, IAppointmentRepository apptRepo, BarberlyDbContext db, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache, MediatR.IMediator mediator)
    {
        if (req == null) return Results.BadRequest();

        if (!string.IsNullOrEmpty(idemKey))
        {
            var existing = await apptRepo.GetByIdempotencyKeyAsync(idemKey);
            if (existing != null)
                return Results.Ok(new { id = existing.Id });
        }

        var conflicts = await apptRepo.GetByBarberAndRangeAsync(req.BarberId, req.Start, req.End);
        if (conflicts.Any())
            return Results.Conflict(new { message = "Time slot already booked" });
        var appt = Appointment.Create(req.UserId, req.BarberId, req.ServiceId, req.Start, req.End, idemKey);
        await apptRepo.AddAsync(appt);
        await db.SaveChangesAsync();
        
        // Publish domain event for email notification
        var appointmentBookedEvent = new Barberly.Domain.Events.AppointmentBookedEvent(
            appt.Id,
            appt.UserId,
            appt.BarberId,
            appt.ServiceId,
            appt.Start,
            appt.End);
        await mediator.Publish(appointmentBookedEvent);
        
        // invalidate cache for this barber/date/service
        var key = $"barbers:{req.BarberId}:slots:{req.Start.UtcDateTime:yyyy-MM-dd}:{req.ServiceId}";
        try
        {
            await cache.RemoveAsync(key);
        }
        catch (Exception)
        {
            // ignore cache remove errors
        }

        return Results.Created($"/api/v1/appointments/{appt.Id}", new { id = appt.Id });
    }

    private static async Task<IResult> GetAppointment(Guid id, IAppointmentRepository apptRepo)
    {
        var appointment = await apptRepo.GetByIdAsync(id);
        if (appointment == null)
            return Results.NotFound(new { message = "Appointment not found" });

        return Results.Ok(new
        {
            id = appointment.Id,
            userId = appointment.UserId,
            barberId = appointment.BarberId,
            serviceId = appointment.ServiceId,
            start = appointment.Start,
            end = appointment.End,
            isCancelled = appointment.IsCancelled,
            cancelledAtUtc = appointment.CancelledAtUtc,
            createdAtUtc = appointment.CreatedAtUtc
        });
    }

    private static async Task<IResult> CancelAppointment(Guid id, IAppointmentRepository apptRepo, BarberlyDbContext db, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache, MediatR.IMediator mediator)
    {
        var appointment = await apptRepo.GetByIdAsync(id);
        if (appointment == null)
            return Results.NotFound(new { message = "Appointment not found" });

        if (appointment.IsCancelled)
            return Results.BadRequest(new { message = "Appointment is already cancelled" });

        if (appointment.Start <= DateTimeOffset.UtcNow)
            return Results.BadRequest(new { message = "Cannot cancel appointment that has already started" });

        try
        {
            appointment.Cancel();
            await db.SaveChangesAsync();

            // Publish domain event for email notification
            var appointmentCancelledEvent = new Barberly.Domain.Events.AppointmentCancelledEvent(
                appointment.Id,
                appointment.UserId,
                appointment.BarberId,
                appointment.ServiceId,
                appointment.Start,
                appointment.End,
                appointment.CancelledAtUtc ?? DateTimeOffset.UtcNow);
            await mediator.Publish(appointmentCancelledEvent);

            // Invalidate cache for this barber/date/service
            var key = $"barbers:{appointment.BarberId}:slots:{appointment.Start.UtcDateTime:yyyy-MM-dd}:{appointment.ServiceId}";
            try
            {
                await cache.RemoveAsync(key);
            }
            catch (Exception)
            {
                // ignore cache remove errors
            }

            return Results.Ok(new { message = "Appointment cancelled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> RescheduleAppointment(Guid id, [FromBody] RescheduleAppointmentRequest req, IAppointmentRepository apptRepo, BarberlyDbContext db, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
        if (req == null)
            return Results.BadRequest(new { message = "Request body is required" });

        var appointment = await apptRepo.GetByIdAsync(id);
        if (appointment == null)
            return Results.NotFound(new { message = "Appointment not found" });

        if (appointment.IsCancelled)
            return Results.BadRequest(new { message = "Cannot reschedule cancelled appointment" });

        // Check for conflicts at new time
        var conflicts = await apptRepo.GetByBarberAndRangeAsync(appointment.BarberId, req.NewStart, req.NewEnd);
        if (conflicts.Any(a => a.Id != id))
            return Results.Conflict(new { message = "New time slot is already booked" });

        try
        {
            appointment.Reschedule(req.NewStart, req.NewEnd);
            await db.SaveChangesAsync();

            // Invalidate cache for both old and new time slots
            var oldKey = $"barbers:{appointment.BarberId}:slots:{appointment.Start.UtcDateTime:yyyy-MM-dd}:{appointment.ServiceId}";
            var newKey = $"barbers:{appointment.BarberId}:slots:{req.NewStart.UtcDateTime:yyyy-MM-dd}:{appointment.ServiceId}";

            try
            {
                await cache.RemoveAsync(oldKey);
                await cache.RemoveAsync(newKey);
            }
            catch (Exception)
            {
                // ignore cache remove errors
            }

            return Results.Ok(new
            {
                message = "Appointment rescheduled successfully",
                appointment = new
                {
                    id = appointment.Id,
                    start = appointment.Start,
                    end = appointment.End
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    public sealed class CreateAppointmentRequest
    {
        public Guid UserId { get; set; }
        public Guid BarberId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
    }

    public sealed class RescheduleAppointmentRequest
    {
        public DateTimeOffset NewStart { get; set; }
        public DateTimeOffset NewEnd { get; set; }
    }
}
