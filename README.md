# Barberly ✅

🪒 **MVP COMPLETE** - Berber/kuaför randevu ve müşteri profil yönetim platformu.

## 🎯 Proje Özeti - OPERATIONAL ✅

Barberly, berber ve kuaförlerin randevu yönetimini kolaylaştıran, modern bir barber shop management platformudur. **Core MVP functionality is complete and fully tested.**

### ✅ Tamamlanan Özellikler (LIVE)

- 📅 **Randevu Sistemi** - Availability checking, booking, conflict detection, idempotency ✅
- 🏪 **Berber/Salon Directory** - Shop & barber listing, search, filtering ✅
- 🔐 **Authentication** - JWT + role-based authorization (Customer, Barber, ShopOwner, Admin) ✅
- 📊 **Dashboard** - Barber browsing with responsive UI ✅
- 🧪 **Testing** - Complete integration test coverage, all passing ✅
- 🗄️ **Database** - PostgreSQL with EF Core migrations, sample data seeding ✅
- ⚡ **Caching** - Redis integration with fallback mechanisms ✅
- 🛡️ **Security** - Rate limiting, CORS, security headers ✅

### � Nice-to-Have Features (Optional)
- 👤 Hair Profile system (customer hair preferences)
- 📱 Push notifications
- Advanced admin panels

## 🛠️ Teknoloji Yığını

### Backend
- .NET 8 (API/BFF)
- PostgreSQL 16
- Redis (Cache)
- Azure Service Bus
- OpenTelemetry + Serilog

### Frontend
- React 18 + TypeScript
- Vite
- TanStack Query
- React Hook Form + Zod
- Tailwind CSS + shadcn/ui

## 🚀 Başlangıç - READY TO RUN ✅

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v20+)
- [PostgreSQL 16](https://www.postgresql.org/)
- [Redis](https://redis.io/) (optional - has fallback)

### Hızlı Başlatma

1. **Tüm servisler (en kolay):**
   ```bash
   # Root directory'de
   npm run backend:run     # Backend API server
   npm run dev             # Frontend dev server
   ```

2. **Manuel Setup:**
   ```bash
   # Backend
   cd backend/src
   dotnet run --project Barberly.Api/Barberly.Api.csproj

   # Frontend (yeni terminal)
   cd web/barberly-web
   npm install
   npm run dev
   ```

### 🔧 Database Setup
```bash
# PostgreSQL database setup
cd backend/src
dotnet ef database update --project Barberly.Infrastructure --startup-project Barberly.Api --context BarberlyDbContext
```

### ✅ API Endpoints - ALL OPERATIONAL

#### Directory Management
- `GET /api/v1/shops` - List barber shops
- `GET /api/v1/barbers` - List barbers (with search)
- `GET /api/v1/services` - List services

#### Scheduling  
- `GET /api/v1/barbers/{id}/availability` - Check available slots
- `POST /api/v1/appointments` - Book appointment (requires Idempotency-Key header)
- `GET /api/v1/appointments/{id}` - Get appointment details
- `DELETE /api/v1/appointments/{id}` - Cancel appointment

#### Authentication
- `POST /auth/register` - User registration
- `POST /auth/login` - User login
- `GET /me` - Current user info

## 🧪 Testing - ALL PASSING ✅

```bash
# Backend tests (all passing)
npm run backend:test
# Or specifically:
dotnet test backend/tests/Barberly.IntegrationTests/

# Frontend tests  
npm run test

# Full verification
npm run lint && npm run type-check && npm run test
```

## 📊 Current Status

✅ **Authentication & Authorization** - Complete with JWT + role-based policies  
✅ **Directory Management** - Shops, barbers, services with search/filtering  
✅ **Scheduling System** - Full appointment booking with conflict detection  
✅ **Database** - PostgreSQL with EF Core, migrations, sample data  
✅ **Caching** - Redis integration with fallback mechanisms  
✅ **Frontend** - React barber browsing page with TanStack Query  
✅ **Testing** - Comprehensive integration test coverage  
🚧 **Hair Profiles** - TODO (optional enhancement)  
🚧 **Notifications** - TODO (optional enhancement)

## 📚 Dokümantasyon

Detaylı dokümantasyon için:
- [API Referansı](docs/api/openapi.yaml)
- [Mimari Diyagramlar](docs/architecture/)
- [Geliştirici Rehberi](.github/copilot-instructions.md)

## 🤝 Katkıda Bulunma

1. Fork'layın
2. Feature branch oluşturun (`git checkout -b feature/amazing`)
3. Commit'leyin (`git commit -m 'feat: amazing feature'`)
4. Push'layın (`git push origin feature/amazing`)
5. PR açın

## 📝 Lisans

MIT

---

> 💡 **Not**: Bu projenin geliştirme süreci ve adımları için [plan.md](docs/plan.md) dosyasına bakabilirsiniz.
