# Barberly

🪒 Berber/kuaför randevu ve müşteri profil yönetim platformu.

## 🎯 Proje Özeti

Barberly, berber ve kuaförlerin randevu yönetimini kolaylaştıran, müşterilerin saç profili ve tercihlerini detaylı şekilde kaydedebildiği modern bir platformdur.

### Temel Özellikler

- 📅 Akıllı randevu sistemi ve slot yönetimi
- 👤 Detaylı müşteri saç profili oluşturma
- 🏪 Berber/salon arama ve filtreleme
- 📱 SMS/e-posta ile bildirimler
- 🔐 Güvenli kimlik doğrulama (OIDC)

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

## 🚀 Başlangıç

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v20+)
- [PostgreSQL 16](https://www.postgresql.org/)
- [Redis](https://redis.io/)

### Kurulum

1. **Backend:**
   ```bash
   cd backend
   dotnet restore
   dotnet build
   cd src/Barberly.Api
   dotnet run
   ```

2. **Frontend:**
   ```bash
   cd web/barberly-web
   npm install
   npm run dev
   ```

### API Endpoint'leri

- `GET /shops?near=...&service=...`
- `GET /barbers/{id}/availability?date=...&serviceId=...`
- `POST /appointments`
- `POST /hair-profiles`

## 🧪 Test

```bash
# Backend
dotnet test

# Frontend
npm run test
```

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
