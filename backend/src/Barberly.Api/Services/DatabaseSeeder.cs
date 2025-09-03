using Barberly.Domain.Entities;
using Barberly.Domain.ValueObjects;
using Barberly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Barberly.Api.Services;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(BarberlyDbContext context)
    {
        if (await context.BarberShops.AnyAsync())
        {
            return; // Database already seeded
        }

        // Create sample barber shops
        var shop1 = BarberShop.Create(
            "Classic Cuts Barbershop",
            "Traditional barber services with modern techniques",
            Address.Create("123 Main St", "Istanbul", "Istanbul", "34000", "Turkey"),
            "+90 216 555 0123",
            "info@classiccuts.com",
            TimeOnly.Parse("09:00"),
            TimeOnly.Parse("19:00"),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
            41.0082m,
            28.9784m,
            "https://classiccuts.com"
        );

        var shop2 = BarberShop.Create(
            "The Gentlemen's Cut",
            "Premium grooming services for the modern gentleman",
            Address.Create("456 Oak Ave", "Istanbul", "Istanbul", "34100", "Turkey"),
            "+90 216 555 0456",
            "contact@gentlemenscut.com",
            TimeOnly.Parse("10:00"),
            TimeOnly.Parse("20:00"),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
            41.0122m,
            28.9824m,
            "https://gentlemenscut.com"
        );

        await context.BarberShops.AddRangeAsync(shop1, shop2);
        await context.SaveChangesAsync();

        // Create sample barbers
        var barber1 = Barber.Create(
            "Ahmet Yılmaz",
            "ahmet.yilmaz@classiccuts.com",
            "+90 555 123 4567",
            shop1.Id,
            8,
            "Expert in classic cuts and traditional barbering techniques",
            null
        );

        var barber2 = Barber.Create(
            "Mehmet Kaya",
            "mehmet.kaya@classiccuts.com",
            "+90 555 234 5678",
            shop1.Id,
            12,
            "Specialist in modern fades and beard grooming",
            null
        );

        var barber3 = Barber.Create(
            "Ali Demir",
            "ali.demir@gentlemenscut.com",
            "+90 555 345 6789",
            shop2.Id,
            6,
            "Young and talented barber with creative styling skills",
            null
        );

        await context.Barbers.AddRangeAsync(barber1, barber2, barber3);
        await context.SaveChangesAsync();

        // Create sample services
        var service1 = Service.Create(
            "Classic Haircut",
            "Traditional men's haircut with clippers and scissors",
            75.00m,
            30,
            shop1.Id,
            null
        );

        var service2 = Service.Create(
            "Beard Trim",
            "Professional beard trimming and shaping",
            45.00m,
            15,
            shop1.Id,
            null
        );

        var service3 = Service.Create(
            "Hair Wash & Style",
            "Complete hair washing and styling service",
            60.00m,
            25,
            shop2.Id,
            null
        );

        await context.Services.AddRangeAsync(service1, service2, service3);
        await context.SaveChangesAsync();

        // Add services to barbers
        barber1.AddService(service1);
        barber1.AddService(service2);
        barber2.AddService(service1);
        barber2.AddService(service2);
        barber3.AddService(service3);

        await context.SaveChangesAsync();
    }
}
