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

        // Create real Trabzon barber shops
        var kadirAlkanShop = BarberShop.Create(
            "Kadir Alkan Hair Artist Erkek Kuaförü",
            "Profesyonel erkek kuaförü ve barber shop, modern tekniklerle klasik berberlik hizmeti",
            Address.Create("3 Nolu Ticari Yapı, Pazarkapı Caddesi", "Trabzon", "Ortahisar", "61030", "Turkey"),
            "0535 517 11 05",
            "info@kadiralkantrabzon.com",
            TimeOnly.Parse("09:00"),
            TimeOnly.Parse("20:00"),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
            41.0015m,
            39.7178m,
            "https://www.instagram.com/kadiralkantrabzon/"
        );

        var mehmetCelikShop = BarberShop.Create(
            "Mehmet Çelik Hair Stylist",
            "Uzman berber, saç kesimi ve sakal düzenleme konusunda 15+ yıl tecrübe",
            Address.Create("Ortahisar Merkez", "Trabzon", "Ortahisar", "61030", "Turkey"),
            "0531 766 20 56",
            "info@mehmetcelikhair.com",
            TimeOnly.Parse("10:00"),
            TimeOnly.Parse("21:00"),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
            41.0025m,
            39.7168m,
            "https://www.instagram.com/kuaformehmetcelikk/"
        );

        var muzoKuaforShop = BarberShop.Create(
            "Muzo Kuaför",
            "Trabzon'un en popüler erkek kuaförü, modern kesimler ve geleneksel berberlik",
            Address.Create("Kalkınma Mahallesi, Farabi Caddesi No:17/B", "Trabzon", "Ortahisar", "61080", "Turkey"),
            "0531 397 35 37",
            "randevu@muzokuafor.com",
            TimeOnly.Parse("09:00"),
            TimeOnly.Parse("19:00"),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
            41.0035m,
            39.7188m,
            "https://www.muzokuafor.com/"
        );

        var blackRazorShop = BarberShop.Create(
            "Black Razor Erkek Kuaförü",
            "Modern berber salonu, hassas tıraş ve saç kesimi uzmanı",
            Address.Create("Kalkınma Mahallesi, 114 No'lu Sokak No:8/A", "Trabzon", "Ortahisar", "61080", "Turkey"),
            "0531 881 56 76",
            "info@blackrazortrabzon.com",
            TimeOnly.Parse("10:00"),
            TimeOnly.Parse("23:30"),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday },
            41.0045m,
            39.7198m,
            null
        );

        var theBarberShop = BarberShop.Create(
            "The Barber Trabzon",
            "Premium erkek kuaförü ve berber salonu, kaliteli hizmet anlayışı",
            Address.Create("Kalkınma Mahallesi, Akif Saruhan Caddesi", "Trabzon", "Ortahisar", "61080", "Turkey"),
            "0462 321 09 90",
            "randevu@thebarbertrabzon.com",
            TimeOnly.Parse("09:00"),
            TimeOnly.Parse("20:00"),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
            41.0055m,
            39.7208m,
            null
        );

        var kuaforTuranShop = BarberShop.Create(
            "Kuaför Turan",
            "5+ yıllık tecrübe ile geleneksel berberlik hizmeti",
            Address.Create("Hava Alanı, Konaklar, Serhat Sokak", "Trabzon", "Ortahisar", "61290", "Turkey"),
            "0532 776 61 83",
            "info@kuaforturan.com",
            TimeOnly.Parse("08:00"),
            TimeOnly.Parse("22:00"),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
            41.0065m,
            39.7218m,
            null
        );

        var blackHairShop = BarberShop.Create(
            "Black Hair",
            "Modern saç kesimi ve styling uzmanı berber salonu",
            Address.Create("Uzun Sokak, 105A", "Trabzon", "Ortahisar", "61030", "Turkey"),
            "0535 123 45 67",
            "info@blackhairtrabzon.com",
            TimeOnly.Parse("09:00"),
            TimeOnly.Parse("19:00"),
            new[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
            41.0075m,
            39.7228m,
            null
        );

        var beratErkekKuaforShop = BarberShop.Create(
            "Berat Erkek Kuaförü",
            "Aydınlıkevler'de kaliteli erkek kuaförü hizmeti",
            Address.Create("Aydınlıkevler Mahallesi, Esen Sokak, 1", "Trabzon", "Ortahisar", "61030", "Turkey"),
            "0532 987 65 43",
            "info@beratkuafor.com",
            TimeOnly.Parse("09:00"),
            TimeOnly.Parse("19:00"),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
            41.0085m,
            39.7238m,
            null
        );

        await context.BarberShops.AddRangeAsync(kadirAlkanShop, mehmetCelikShop, muzoKuaforShop, blackRazorShop, theBarberShop, kuaforTuranShop, blackHairShop, beratErkekKuaforShop);
        await context.SaveChangesAsync();

        // Create real Trabzon barbers
        var barber1 = Barber.Create(
            "Kadir Alkan",
            "kadir@kadiralkantrabzon.com",
            "0535 517 11 05",
            kadirAlkanShop.Id,
            15,
            "Master berber, saç kesimi ve sakal düzenleme konusunda uzman",
            null
        );

        var barber2 = Barber.Create(
            "Mehmet Çelik",
            "mehmet@mehmetcelikhair.com",
            "0531 766 20 56",
            mehmetCelikShop.Id,
            18,
            "Hair stylist, modern kesimler ve geleneksel berberlik tekniklerinde uzman",
            null
        );

        var barber3 = Barber.Create(
            "Murat Muzo",
            "murat@muzokuafor.com",
            "0531 397 35 37",
            muzoKuaforShop.Id,
            12,
            "Popüler berber, trendy kesimler ve styling konusunda uzman",
            null
        );

        var barber4 = Barber.Create(
            "Emre Kara",
            "emre@blackrazortrabzon.com",
            "0531 881 56 76",
            blackRazorShop.Id,
            8,
            "Genç ve yetenekli berber, modern fade kesimler ve hassas tıraş uzmanı",
            null
        );

        var barber5 = Barber.Create(
            "Ahmet Turan",
            "ahmet@kuaforturan.com",
            "0532 776 61 83",
            kuaforTuranShop.Id,
            10,
            "Deneyimli berber, klasik kesimler ve geleneksel berberlik hizmetleri",
            null
        );

        var barber6 = Barber.Create(
            "Oğuz Black",
            "oguz@blackhairtrabzon.com",
            "0535 123 45 67",
            blackHairShop.Id,
            6,
            "Yaratıcı saç stilisti, modern kesimler ve renklendirme uzmanı",
            null
        );

        var barber7 = Barber.Create(
            "Berat Yıldız",
            "berat@beratkuafor.com",
            "0532 987 65 43",
            beratErkekKuaforShop.Id,
            9,
            "Aydınlıkevler'in güvenilir berberi, her yaşa uygun kesimler",
            null
        );

        await context.Barbers.AddRangeAsync(barber1, barber2, barber3, barber4, barber5, barber6, barber7);
        await context.SaveChangesAsync();

        // Create realistic services for Trabzon barbershops
        var classicHaircut = Service.Create(
            "Klasik Saç Kesimi",
            "Makine ve makasla geleneksel erkek saç kesimi",
            80.00m,
            30,
            kadirAlkanShop.Id,
            null
        );

        var modernFade = Service.Create(
            "Modern Fade Kesim",
            "Güncel trendlere uygun fade ve modern kesimler",
            100.00m,
            35,
            kadirAlkanShop.Id,
            null
        );

        var beardTrim = Service.Create(
            "Sakal Düzenleme",
            "Profesyonel sakal kesimi ve şekillendirme",
            50.00m,
            15,
            mehmetCelikShop.Id,
            null
        );

        var razorShave = Service.Create(
            "Ustura Tıraşı",
            "Geleneksel ustura ile hassas tıraş",
            70.00m,
            20,
            blackRazorShop.Id,
            null
        );

        var hairWash = Service.Create(
            "Saç Yıkama ve Styling",
            "Şampuan, saç bakımı ve styling hizmeti",
            40.00m,
            20,
            muzoKuaforShop.Id,
            null
        );

        var skinFade = Service.Create(
            "Skin Fade",
            "Derideki sıfır fade tekniği",
            90.00m,
            40,
            theBarberShop.Id,
            null
        );

        var eyebrowTrim = Service.Create(
            "Kaş Düzenleme",
            "Erkek kaş alımı ve şekillendirme",
            30.00m,
            10,
            kuaforTuranShop.Id,
            null
        );

        var hairColoring = Service.Create(
            "Saç Boyama",
            "Erkek saç boyama ve renklendirme hizmeti",
            120.00m,
            60,
            blackHairShop.Id,
            null
        );

        await context.Services.AddRangeAsync(classicHaircut, modernFade, beardTrim, razorShave, hairWash, skinFade, eyebrowTrim, hairColoring);
        await context.SaveChangesAsync();

        // Add services to barbers - realistic skill distribution
        // Kadir Alkan - Master berber, all services
        barber1.AddService(classicHaircut);
        barber1.AddService(modernFade);
        barber1.AddService(beardTrim);

        // Mehmet Çelik - Specialist in styling and beard work
        barber2.AddService(beardTrim);
        barber2.AddService(hairWash);
        barber2.AddService(eyebrowTrim);

        // Murat Muzo - Popular modern cuts
        barber3.AddService(modernFade);
        barber3.AddService(hairWash);
        barber3.AddService(classicHaircut);

        // Emre Kara - Razor specialist
        barber4.AddService(razorShave);
        barber4.AddService(skinFade);
        barber4.AddService(beardTrim);

        // Ahmet Turan - Traditional services
        barber5.AddService(classicHaircut);
        barber5.AddService(eyebrowTrim);
        barber5.AddService(hairWash);

        // Oğuz Black - Creative stylist
        barber6.AddService(hairColoring);
        barber6.AddService(modernFade);
        barber6.AddService(hairWash);

        // Berat Yıldız - General services
        barber7.AddService(classicHaircut);
        barber7.AddService(beardTrim);
        barber7.AddService(hairWash);

        await context.SaveChangesAsync();
    }
}
