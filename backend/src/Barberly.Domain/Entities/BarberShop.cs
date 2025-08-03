using Barberly.Domain.Common;
using Barberly.Domain.ValueObjects;

namespace Barberly.Domain.Entities;

public sealed class BarberShop : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Website { get; private set; }
    public bool IsActive { get; private set; }
    
    // Business hours
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public DayOfWeek[] WorkingDays { get; private set; } = Array.Empty<DayOfWeek>();
    
    // Location for search
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    
    // Rating
    public decimal AverageRating { get; private set; }
    public int TotalReviews { get; private set; }    // Navigation properties
    public IReadOnlyList<Barber> Barbers => _barbers.AsReadOnly();
    private readonly List<Barber> _barbers = new();

    public IReadOnlyList<Service> Services => _services.AsReadOnly();
    private readonly List<Service> _services = new();

    private BarberShop() { } // EF Core

    private BarberShop(
        string name,
        string description,
        Address address,
        string phone,
        string email,
        TimeOnly openTime,
        TimeOnly closeTime,
        DayOfWeek[] workingDays,
        decimal latitude,
        decimal longitude,
        string? website = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty", nameof(phone));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (workingDays.Length == 0)
            throw new ArgumentException("At least one working day must be specified", nameof(workingDays));

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Address = address;
        Phone = phone;
        Email = email;
        Website = website;
        OpenTime = openTime;
        CloseTime = closeTime;
        WorkingDays = workingDays;
        Latitude = latitude;
        Longitude = longitude;
        IsActive = true;
        AverageRating = 0;
        TotalReviews = 0;
    }

    public static BarberShop Create(
        string name,
        string description,
        Address address,
        string phone,
        string email,
        TimeOnly openTime,
        TimeOnly closeTime,
        DayOfWeek[] workingDays,
        decimal latitude,
        decimal longitude,
        string? website = null)
    {
        return new BarberShop(name, description, address, phone, email, openTime, closeTime, workingDays, latitude, longitude, website);
    }

    public void UpdateInfo(string name, string description, string phone, string email, string? website)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
        Description = description;
        Phone = phone;
        Email = email;
        Website = website;
    }

    public void UpdateBusinessHours(TimeOnly openTime, TimeOnly closeTime, DayOfWeek[] workingDays)
    {
        if (workingDays.Length == 0)
            throw new ArgumentException("At least one working day must be specified", nameof(workingDays));

        OpenTime = openTime;
        CloseTime = closeTime;
        WorkingDays = workingDays;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void AddBarber(Barber barber)
    {
        if (_barbers.Any(b => b.Id == barber.Id))
            throw new InvalidOperationException("Barber already exists in this shop");

        _barbers.Add(barber);
    }

    public void AddService(Service service)
    {
        if (_services.Any(s => s.Id == service.Id))
            throw new InvalidOperationException("Service already exists in this shop");

        _services.Add(service);
    }
}
