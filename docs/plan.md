# Barberly Proje Haritası ve Takip Planı

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
- [x] Kullanıcı ve berber kayıt/login API'ları (`POST /auth/register`, `POST /auth/login`)
- [x] Policy/role tabanlı yetkilendirme
- [x] Rate limiting ve güvenlik yapılandırması

## 3. Directory (Salon/Barber/Service Profilleri)

- [ ] BarberShop, Barber, Service entity ve API’larını oluştur
- [ ] Salon/berber/hizmet CRUD endpoint’leri (`GET /shops`, `GET /barbers/{id}`, `GET /services?shopId=...`)
- [ ] Salon/berber/hizmet arama ve filtreleme (konum, hizmet, fiyat)
- [ ] Salon ve berber görsel yükleme (Blob Storage)

## 4. Scheduling (Randevu & Slot Yönetimi)

- [ ] Çalışma saatleri ve takvim kuralları (ScheduleTemplate)
- [ ] Slot üretimi (önbellekleme/cache: Redis entegrasyonu MVP'de gerekli değil — sonlara ertelendi)
- [x] Barber müsaitlik API’si (`GET /barbers/{id}/availability`)
- [x] Randevu oluşturma (`POST /appointments`)
- [ ] Randevu iptal (`DELETE /appointments/{id}`)
- [ ] Randevu erteleme/değiştirme (`PATCH /appointments/{id}`)
- [x] Idempotency-key (server-side check implemented)
- [ ] Concurrency control (further work: stronger transactional/locking guarantees)
- [ ] SignalR ile canlı slot güncellemeleri (v2)

> Not: Scheduling endpoints (`GET /barbers/{id}/availability` and `POST /appointments`) are implemented in the API (`SchedulingEndpoints.cs`). The implementation includes idempotency-key handling and basic conflict detection. Redis-based caching for slot generation is still TODO and should be tracked here as part of the MVP scheduling work. Cancel/reschedule endpoints are not yet implemented.

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

- [x] Unit testler (Domain, Application layer)
- [x] Integration testler (API + EFCore)
- [ ] Contract testler (Pact)
- [ ] E2E testler (Playwright)
- [ ] Load testler (k6/Gatling)
- [ ] SonarQube, Snyk, OWASP Dependency Check entegrasyonu

## 9. DevOps & Altyapı

- [ ] IaC (Bicep/Terraform) ile temel kaynakların kurulumu
- [ ] Azure App Service ve PostgreSQL kurulumu
- [ ] Blob Storage entegrasyonu
- [ ] Redis entegrasyonu (cache) — deferred / sonlar: MVP sonrası
- [ ] Feature flag altyapısı (Unleash/LaunchDarkly)

## 10. UI/UX Stack & Development (MVP)

### 10.1 Modern UI Stack (AI/Backend-Developer Friendly)
- [x] React 18 + TypeScript + Vite (already scaffolded)
- [x] **TanStack Query** (server state management) + React Hook Form + Zod (forms)
- [x] **shadcn/ui** + Tailwind CSS (component system + styling)
- [x] **Lucide React** (icons) + **date-fns** (date utilities)
- [x] **React Router** v6 (navigation) + **i18next** (internationalization)
- [x] **Storybook** (component development) + **Vitest** + **Testing Library** (unit tests)
- [x] **Playwright** (E2E tests) + **MSW** (API mocking)

### 10.2 API Integration & Type Safety
- [x] OpenAPI type generation (`openapi-typescript` from backend Swagger)
- [x] Axios client with interceptors (auth, retry, error handling)
- [x] TanStack Query setup with proper cache invalidation
- [x] Error boundary + toast notifications

### 10.3 Core UI Components (Progressive Implementation)
- [x] **Phase 1**: Layout, Navigation, Auth forms
- [x] **Phase 2**: Barber/Shop listing + search/filter
- [x] **Phase 3**: Availability calendar + slot selection
- [x] **Phase 4**: Appointment booking form + confirmation
- [ ] **Phase 5**: Hair profile wizard (multi-step form)

### 10.4 UI/UX User Flows (Customer-Facing Demo)
- [x] **Flow 1**: Browse shops → Select barber → View availability
- [x] **Flow 2**: Choose time slot → Fill appointment details → Confirm booking
- [ ] **Flow 3**: Create hair profile → Upload photo → Add preferences
- [ ] **Flow 4**: View appointment history + upcoming bookings
- [ ] **Flow 5**: Responsive mobile experience (MVP requirement)

### 10.5 Testing Strategy (AI-Assisted Development)
- [x] Component tests with Storybook + Testing Library
- [x] Integration tests with MSW (mock backend)
- [x] E2E tests with Playwright (critical user journeys)
- [ ] Visual regression tests (Chromatic + Storybook)
- [ ] Accessibility tests (axe-core + manual testing)

## 11. Ekstra (V2/V3 için hazırlık)

- [ ] Review/puanlama modülü (v2)
- [ ] AI saç profili parser ve öneri sistemi (v2)
- [ ] Ödeme/pos entegrasyonu (v3)
- [ ] Çoklu şube ve kurumsal panel (v3)

---

> Her adım tamamlandıkça işaretleyin. Detaylar ve şablonlar için `/docs/prompts/` klasörüne bakabilirsiniz.
