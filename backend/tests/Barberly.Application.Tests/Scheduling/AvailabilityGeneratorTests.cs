using Barberly.Domain.Entities;
using System;
using Xunit;

namespace Barberly.Application.Tests.Scheduling;

public class AppointmentTests
{
    [Fact]
    public void Create_ValidAppointment_SetsProperties()
    {
        var userId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddMinutes(30);

        var appt = Appointment.Create(userId, barberId, serviceId, start, end, "idem-1");

        Assert.Equal(userId, appt.UserId);
        Assert.Equal(barberId, appt.BarberId);
        Assert.Equal(serviceId, appt.ServiceId);
        Assert.Equal(start, appt.Start);
        Assert.Equal(end, appt.End);
        Assert.Equal("idem-1", appt.IdempotencyKey);
        Assert.NotEqual(Guid.Empty, appt.Id);
    }

    [Fact]
    public void Create_StartAfterEnd_Throws()
    {
        var userId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(2);
        var end = start.AddMinutes(-30);

        Assert.Throws<ArgumentException>(() => Appointment.Create(userId, barberId, serviceId, start, end));
    }
}
