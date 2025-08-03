using Barberly.Domain.Common;

namespace Barberly.Domain.Entities;

public sealed class Barber : Entity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public bool IsActive { get; private set; }
    public int YearsOfExperience { get; private set; }

    // Ratings
    public decimal AverageRating { get; private set; }
    public int TotalReviews { get; private set; }

    // Shop relationship
    public Guid BarberShopId { get; private set; }
    public BarberShop BarberShop { get; private set; } = null!;

    // Services this barber provides
    public IReadOnlyList<Service> Services => _services.AsReadOnly();
    private readonly List<Service> _services = new();

    private Barber() { } // EF Core

    private Barber(
        string fullName,
        string email,
        string phone,
        Guid barberShopId,
        int yearsOfExperience = 0,
        string? bio = null,
        string? profileImageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty", nameof(fullName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty", nameof(phone));

        if (barberShopId == Guid.Empty)
            throw new ArgumentException("BarberShop ID cannot be empty", nameof(barberShopId));

        if (yearsOfExperience < 0)
            throw new ArgumentException("Years of experience cannot be negative", nameof(yearsOfExperience));

        Id = Guid.NewGuid();
        FullName = fullName;
        Email = email;
        Phone = phone;
        BarberShopId = barberShopId;
        YearsOfExperience = yearsOfExperience;
        Bio = bio;
        ProfileImageUrl = profileImageUrl;
        IsActive = true;
        AverageRating = 0;
        TotalReviews = 0;
    }

    public static Barber Create(
        string fullName,
        string email,
        string phone,
        Guid barberShopId,
        int yearsOfExperience = 0,
        string? bio = null,
        string? profileImageUrl = null)
    {
        return new Barber(fullName, email, phone, barberShopId, yearsOfExperience, bio, profileImageUrl);
    }

    public void UpdateInfo(string fullName, string email, string phone, string? bio)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty", nameof(fullName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty", nameof(phone));

        FullName = fullName;
        Email = email;
        Phone = phone;
        Bio = bio;
    }

    public void UpdateProfileImage(string? profileImageUrl)
    {
        ProfileImageUrl = profileImageUrl;
    }

    public void UpdateExperience(int yearsOfExperience)
    {
        if (yearsOfExperience < 0)
            throw new ArgumentException("Years of experience cannot be negative", nameof(yearsOfExperience));

        YearsOfExperience = yearsOfExperience;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void AddService(Service service)
    {
        if (_services.Any(s => s.Id == service.Id))
            throw new InvalidOperationException("Service already assigned to this barber");

        _services.Add(service);
    }

    public void RemoveService(Service service)
    {
        _services.Remove(service);
    }
}
