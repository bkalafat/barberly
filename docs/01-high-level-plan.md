# Barberly — High Level Plan & Scope

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

Note: Basic scheduling endpoints (availability query and appointment creation) exist in the API (see `backend/src/Barberly.Api/Endpoints/SchedulingEndpoints.cs`). Redis-backed slot caching is planned but still TODO as part of the Scheduling work.

## Yol Haritası
- Sprint 1–2: Auth, Directory, Scheduling, Appointment, HairProfile, Messaging
- Sprint 3–4: Real-time, iCal, arama, foto, admin panel
- V2: AI parser, öneri, review/ratings
- V3: Ödeme, kampanya, çoklu şube
