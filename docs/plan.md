# Barberly Pro## 2. Kimlik ve Yetkilendirme (Identity & Access)

- [x] Azure AD B2C veya Auth0 ile OIDC/OAuth2 entegrasyonu
- [x] Kullanıcı ve berber kayıt/login API'ları (`POST /auth/register`, `POST /auth/login`)
- [ ] Policy/role tabanlı yetkilendirme
- [ ] Rate limiting ve güvenlik yapılandırması Haritası ve Takip Planı

Bu dosya, Barberly MVP ve sonraki fazlar için adım adım takip edilebilecek, işaretlenebilir bir proje planıdır. Her adım tamamlandıkça işaretleyebilirsiniz. Plan, Copilot ile modüler ve sürdürülebilir geliştirme için optimize edilmiştir.

---

## 1. Proje Kurulum ve Altyapı

- [x] Proje repo ve temel klasör yapısını oluştur
- [x] .NET 8 backend (API/BFF) projesini başlat
- [x] React + TypeScript frontend projesini başlat (Vite ile)
- [x] Temel CI/CD pipeline (GitHub Actions) kur
- [x] Temel dokümantasyon ve Copilot rehber dosyalarını ekle (`.github/copilot-instructions.md`, `/docs/prompts/`)

## 2. Kimlik ve Yetkilendirme (Identity & Access)

- [x] Azure AD B2C veya Auth0 ile OIDC/OAuth2 entegrasyonu
- [ ] Kullanıcı ve berber kayıt/login API’ları (`POST /auth/register`, `POST /auth/login`)
- [ ] Policy/role tabanlı yetkilendirme
- [ ] Rate limiting ve güvenlik yapılandırması

## 3. Directory (Salon/Barber/Service Profilleri)

- [ ] BarberShop, Barber, Service entity ve API’larını oluştur
- [ ] Salon/berber/hizmet CRUD endpoint’leri (`GET /shops`, `GET /barbers/{id}`, `GET /services?shopId=...`)
- [ ] Salon/berber/hizmet arama ve filtreleme (konum, hizmet, fiyat)
- [ ] Salon ve berber görsel yükleme (Blob Storage)

## 4. Scheduling (Randevu & Slot Yönetimi)

- [ ] Çalışma saatleri ve takvim kuralları (ScheduleTemplate)
- [ ] Slot üretimi ve önbellekleme (Redis)
- [ ] Barber müsaitlik API’si (`GET /barbers/{id}/availability`)
- [ ] Randevu oluşturma/iptal/erteleme (`POST /appointments`, `PATCH /appointments/{id}`)
- [ ] Idempotency-key ve concurrency kontrolü
- [ ] SignalR ile canlı slot güncellemeleri (v2)

## 5. Hair Profile (AI-ready)

- [ ] HairProfile entity ve API’ları (`POST /hair-profiles`, `GET /hair-profiles/me`)
- [ ] Saç profili sihirbazı (frontend)
- [ ] FreeText + etiketler + foto yükleme
- [ ] (Opsiyonel) Azure OpenAI ile FreeText’ten etiket üretimi (ParserService)

## 6. Messaging (Bildirimler)

- [ ] Outbox pattern ile bildirim kuyruğu
- [ ] Azure Service Bus ile entegrasyon
- [ ] SMS/e-posta/WhatsApp bildirim servisleri
- [ ] Bildirim API’si (`POST /notifications/test`)

## 7. Observability & Monitoring

- [ ] Serilog + OpenTelemetry + Azure App Insights entegrasyonu
- [ ] API health check endpoint’leri

## 8. Test & Kalite

- [ ] Unit testler (Domain, Application layer)
- [ ] Integration testler (API + EFCore)
- [ ] Contract testler (Pact)
- [ ] E2E testler (Playwright)
- [ ] Load testler (k6/Gatling)
- [ ] SonarQube, Snyk, OWASP Dependency Check entegrasyonu

## 9. DevOps & Altyapı

- [ ] IaC (Bicep/Terraform) ile temel kaynakların kurulumu
- [ ] Azure App Service ve PostgreSQL kurulumu
- [ ] Redis ve Blob Storage entegrasyonu
- [ ] Feature flag altyapısı (Unleash/LaunchDarkly)

## 10. UI/UX Akışları (MVP)

- [ ] Müşteri onboarding (SMS/e-posta doğrulama)
- [ ] Konum bazlı salon/berber listesi ve filtreleme
- [ ] Takvim tabanlı slot seçimi
- [ ] Saç profili sihirbazı (adım adım)
- [ ] Randevu özeti ve paylaşım (iCal, WhatsApp)

## 11. Ekstra (V2/V3 için hazırlık)

- [ ] Review/puanlama modülü (v2)
- [ ] AI saç profili parser ve öneri sistemi (v2)
- [ ] Ödeme/pos entegrasyonu (v3)
- [ ] Çoklu şube ve kurumsal panel (v3)

---

> Her adım tamamlandıkça işaretleyin. Detaylar ve şablonlar için `/docs/prompts/` klasörüne bakabilirsiniz.
