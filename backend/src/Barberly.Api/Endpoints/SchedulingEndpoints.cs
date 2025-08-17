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
    }

    private static async Task<IResult> GetAvailability(Guid id, [FromQuery] DateTime? date, [FromQuery] Guid? serviceId, IAppointmentRepository apptRepo, IServiceRepository serviceRepo, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
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
    private static async Task<IResult> CreateAppointment([FromHeader(Name = "Idempotency-Key")] string? idemKey, [FromBody] CreateAppointmentRequest req, IAppointmentRepository apptRepo, BarberlyDbContext db, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
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

    public sealed class CreateAppointmentRequest
    {
        public Guid UserId { get; set; }
        public Guid BarberId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
    }
}
