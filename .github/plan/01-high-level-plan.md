# Barb## Bounded Context'ler
- ✅ Identity & Access (JWT auth, role policies, middleware complete)
- ✅ Directory (BarberShop, Barber, Service CRUD complete)
- ✅ Scheduling (Availability, Appointment CRUD, Redis caching complete)  
- 🚧 Hair Profile (TODO - not implemented)
- 🚧 Messaging (Basic notifications TODO) High Level Plan & Scope

## MVP Hedefleri
- Müşteri randevu alabilmeli
- Saç profili ve tercihlerini tanımlayabilmeli

## Bounded Context’ler
- Identity & Access
- Directory
- Scheduling
- Hair Profile
- Messaging

## Veri Modeli (Özet)
- User, BarberShop, Barber, Service, ScheduleTemplate, AvailabilitySlot, Appointment, HairProfile, Notification

## Kritik Akışlar
- Randevu rezervasyonu
- Saç profili oluşturma

Note: ✅ COMPLETE - Scheduling endpoints with Redis caching, conflict detection, and idempotency are fully implemented (see `backend/src/Barberly.Api/Endpoints/SchedulingEndpoints.cs`). Integration tests confirm all functionality works.

## Yol Haritası
- Sprint 1–2: Auth, Directory, Scheduling, Appointment, HairProfile, Messaging
- Sprint 3–4: Real-time, iCal, arama, foto, admin panel
- V2: AI parser, öneri, review/ratings
- V3: Ödeme, kampanya, çoklu şube
