using Barberly.Domain.Common;

namespace Barberly.Domain.Entities;

public sealed class Service : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int DurationInMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public string? ImageUrl { get; private set; }
    
    // Shop relationship
    public Guid BarberShopId { get; private set; }
    public BarberShop BarberShop { get; private set; } = null!;
    
    // Barbers who can provide this service
    public IReadOnlyList<Barber> Barbers => _barbers.AsReadOnly();
    private readonly List<Barber> _barbers = new();

    private Service() { } // EF Core

    private Service(
        string name,
        string description,
        decimal price,
        int durationInMinutes,
        Guid barberShopId,
        string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (durationInMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationInMinutes));

        if (barberShopId == Guid.Empty)
            throw new ArgumentException("BarberShop ID cannot be empty", nameof(barberShopId));

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        DurationInMinutes = durationInMinutes;
        BarberShopId = barberShopId;
        ImageUrl = imageUrl;
        IsActive = true;
    }

    public static Service Create(
        string name,
        string description,
        decimal price,
        int durationInMinutes,
        Guid barberShopId,
        string? imageUrl = null)
    {
        return new Service(name, description, price, durationInMinutes, barberShopId, imageUrl);
    }

    public void UpdateInfo(string name, string description, decimal price, int durationInMinutes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (durationInMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationInMinutes));

        Name = name;
        Description = description;
        Price = price;
        DurationInMinutes = durationInMinutes;
    }

    public void UpdateImage(string? imageUrl)
    {
        ImageUrl = imageUrl;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void AssignBarber(Barber barber)
    {
        if (_barbers.Any(b => b.Id == barber.Id))
            throw new InvalidOperationException("Barber already assigned to this service");

        _barbers.Add(barber);
    }

    public void UnassignBarber(Barber barber)
    {
        _barbers.Remove(barber);
    }
}
