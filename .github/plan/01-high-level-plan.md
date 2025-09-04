# Barberly High Level Plan & Scope

## MVP Hedefleri

- Müşteri randevu alabilmeli
- Saç profili ve tercihlerini tanımlayabilmeli
- **Gerçek veri ile demo**: Trabzon kuaför/berberleri ile canlı sistem

## Bounded Context'ler

- ✅ Identity & Access (JWT auth, role policies, middleware complete)
- ✅ Directory (BarberShop, Barber, Service CRUD complete)
- ✅ Scheduling (Availability, Appointment CRUD, Redis caching complete)
- ✅ E2E Testing (Playwright MCP integration, 24+ comprehensive tests)
- 🔄 Real Data Seeding (Trabzon kuaförleri - Google Maps API integration)
- 🚧 Hair Profile (TODO - not implemented)
- 🚧 Messaging (Basic notifications TODO)

## Veri Modeli (Özet)

- User, BarberShop, Barber, Service, ScheduleTemplate, AvailabilitySlot, Appointment, HairProfile, Notification

## Kritik Akışlar (E2E Test Coverage ✅)

- ✅ **Randevu rezervasyonu** (Full E2E tested with Playwright)
- ✅ **Berber arama ve filtreleme** (E2E tested)
- ✅ **Salon görüntüleme ve navigasyon** (E2E tested)
- 🔄 **Saç profili oluşturma** (TODO)

## Test Otomasyonu ✅ **COMPLETE**

- **Playwright MCP**: VS Code'da doğal dil ile browser testi
- **Multi-browser**: Chrome, Firefox, Safari, Mobile support
- **API Mocking**: Error states, loading states, form validation
- **CI/CD Ready**: GitHub Actions integration, parallel execution

## Gerçek Veri Entegrasyonu 🔄 **IN PROGRESS**

- **Trabzon Maps Data**: Google Places API ile gerçek kuaför verisi
- **Database Seeding**: Test verilerinden gerçek verilere geçiş
- **Demo Preparation**: Canlı veri ile E2E test validation

Note: ✅ COMPLETE - Scheduling endpoints with Redis caching, conflict detection, and idempotency are fully implemented (see `backend/src/Barberly.Api/Endpoints/SchedulingEndpoints.cs`). Integration tests confirm all functionality works. E2E testing infrastructure is fully operational with Playwright MCP.

## Yol Haritası

- Sprint 1–2: ✅ Auth, Directory, Scheduling, Appointment + E2E Testing Complete
- **Sprint 3**: 🔄 Real Data Integration (Trabzon kuaförleri), HairProfile, Messaging
- Sprint 4: Real-time, iCal, arama, foto, admin panel
- V2: AI parser, öneri, review/ratings
- V3: Ödeme, kampanya, çoklu şube
