---
applyTo: '**'
---

# Copilot Memory — barberly

## Genel
- Proje: barberly (main branch)
- Backend: .NET 8, Clean Architecture, DDD, CQRS, EF Core 8, PostgreSQL
- Frontend: React 18, TypeScript, TanStack Query, RHF + Zod, Tailwind, shadcn/ui
- Outbox, Observability, Policy-based Auth, RateLimit, Redis, Service Bus

## Aktif dallar ve ilerleme noktaları


#### 1.1. Mimari ve Altyapı
  - DDD ve Clean Architecture interface ayrımı, repository pattern, DI, EF Core context ve mapping’ler tamamlandı.

#### 1.2. Migration & DB
  - PostgreSQL migration ve connection string sorunları çözüldü.
  - ValueConverter ve ValueComparer ile koleksiyon mapping’leri (ör. WorkingDays) uygulandı.
  - Migration ve tablo oluşturma adımları, connection string formatı, local PostgreSQL kurulumu ve best practice’ler detaylı anlatıldı.
  - Local PostgreSQL için önerilen kullanıcı adı/şifre: postgres / postgres123
  - Son durum: Migration ve tablo oluşturma için PostgreSQL kurulumu ve connection string düzeltmesi bekleniyor.

#### 1.3. Kaldığımız Adımlar (Backend/EF Core)
  - [ ] PostgreSQL local kurulumu ve servis başlatılması
  - [ ] appsettings.Development.json connection string’in güncellenmesi (ör: postgres / postgres123)
  - [ ] dotnet ef database update ile migration’ın başarıyla uygulanması
  - [ ] Proje build ve test

### 2. Diğer dallar

#### 2.1. Frontend
  - Henüz aktif bir ilerleme yok.

#### 2.2. Test/CI-CD
  - Henüz aktif bir ilerleme yok.

#### 2.3. Dokümantasyon
  - Henüz aktif bir ilerleme yok.

## Notlar
- Her yeni dallanma veya önemli karar memory.md’ye eklenmeli.
- Kullanıcıdan gelen dallanma talepleri, branch isimleri ve ilerleme noktaları burada tutulacak.
- Her adımda güncel memory dosyası referans alınmalı.

---
Son güncelleme: 2025-08-06
