# Interface & Dependency Management Pattern (Barberly)

## Amaç
Domain ve Application katmanlarını Infrastructure’dan izole etmek, Clean Architecture ve DDD prensiplerine uygun, sürdürülebilir ve test edilebilir bir yapı kurmak.

## Katmanlar ve Bağımlılıklar

```
Barberly.Api
└── Barberly.Application (interface)
    └── Barberly.Domain
Barberly.Infrastructure (implementasyon)
└── Barberly.Application (interface)
```

- **Domain**: Sadece iş kuralları, entity, value object, event.
- **Application**: CQRS, use-case, validation, repository/service interface’leri.
- **Infrastructure**: Sadece Application’daki interface’lerin implementasyonları.
- **API**: DI ile interface-implementasyon eşleşmesi.

## Interface Yönetimi

- **Application/Interfaces/** altında repository ve servis interface’leri tanımlanır.
- **Infrastructure/Persistence/** altında bu interface’lerin implementasyonları bulunur.
- **Program.cs** veya composition root’ta DI ile bağlanır.

### Örnek

**Application/Interfaces/IBarberRepository.cs**
```csharp
public interface IBarberRepository
{
    Task<Barber?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Barber barber, CancellationToken ct = default);
}
```

**Infrastructure/Persistence/BarberRepository.cs**
```csharp
public class BarberRepository : IBarberRepository
{
    // EF Core implementasyonu
}
```

**Program.cs**
```csharp
builder.Services.AddScoped<IBarberRepository, BarberRepository>();
```

## Neden PostgreSQL?
- Modern .NET ve EF Core ile tam uyumlu
- JSONB, spatial, concurrency, transaction desteği güçlü
- Açık kaynak, bulutlarda yönetilen servisi var
- Barberly gibi randevu, katalog, log, outbox gibi karmaşık veri modelleri için ideal

## Uygulama Adımları
1. Application/Interfaces altında repository/service interface’lerini oluştur
2. Infrastructure’da implementasyonlarını yaz
3. Program.cs’de DI ile bağla
4. Testlerde interface’leri mock’la
5. Domain ve Application katmanları Infrastructure’dan izole kalır

---
Bu doküman, Barberly projesinde interface ve bağımlılık yönetimi için referans alınmalıdır.
