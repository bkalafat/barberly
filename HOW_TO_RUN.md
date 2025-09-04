# How to Run the Barberly Application

This guide explains how to run both the backend (.NET 8) and frontend (React + TypeScript) components of the Barberly barber shop management platform.

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ and npm
- .NET 8 SDK
- PostgreSQL 16 (or Docker for running postgres)
- Git

### 1. Clone and Setup
```bash
git clone https://github.com/bkalafat/barberly.git
cd barberly
```

### 2. Backend Setup (from repository root)
```bash
# Build the backend solution
npm run backend:build

# Start PostgreSQL (if using Docker)
npm run docker:postgres    # or manually: docker run --name barberly-postgres -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres123 -e POSTGRES_DB=BarberlyDb -p 5432:5432 -d postgres:15

# Run EF migrations
npm run backend:migrate     # or manually: dotnet ef database update --project backend/src/Barberly.Infrastructure --startup-project backend/src/Barberly.Api

# Start the backend API server
npm run backend:run         # or manually: dotnet run --project backend/src/Barberly.Api/Barberly.Api.csproj
```

The backend API will be available at `http://localhost:5000`

### 3. Frontend Setup (from repository root)
```bash
# Install frontend dependencies
cd web/barberly-web
npm install

# Start the frontend development server  
npm run dev                 # or from root: npm run dev
```

The frontend will be available at `http://localhost:5173`

## 🔧 Development Scripts

### Root Package.json Scripts
Run these from the repository root (`c:\dev\barberly`):

#### Frontend Scripts
```bash
npm run lint              # ESLint check
npm run lint:fix          # ESLint fix
npm run format            # Prettier format  
npm run type-check        # TypeScript check
npm run dev               # Start dev server
npm run build             # Build for production
npm run test              # Run Vitest tests
npm run test:coverage     # Run tests with coverage
```

#### Backend Scripts
```bash
npm run backend:build     # Build .NET solution
npm run backend:run       # Start API server
npm run backend:test      # Run all .NET tests
```

### Frontend Package.json Scripts  
Run these from `web/barberly-web` directory:

```bash
npm run dev               # Vite dev server
npm run build             # Production build
npm run preview           # Preview production build
npm run lint              # ESLint check
npm run lint:fix          # ESLint with auto-fix
npm run format            # Prettier format
npm run type-check        # TypeScript validation
npm run test              # Vitest tests
npm run test:ui           # Vitest UI
npm run test:coverage     # Test coverage report
npm run e2e               # Playwright E2E tests
npm run e2e:ui            # Playwright UI
npm run storybook         # Storybook dev server
npm run build-storybook   # Build Storybook
```

## 🧪 Testing

### Backend Tests
```bash
# From repository root
npm run backend:test

# Or manually from backend/
dotnet test backend/tests/Barberly.Application.Tests/
dotnet test backend/tests/Barberly.IntegrationTests/
dotnet test backend/tests/Barberly.Domain.Tests/
```

### Frontend Tests
```bash
# From repository root
npm run test              # Run all tests
npm run test:coverage     # With coverage report

# From web/barberly-web/
npm run test              # Vitest tests
npm run test:ui           # Interactive Vitest UI
npm run e2e               # Playwright E2E tests
```

## 🌐 API Endpoints

Once the backend is running, you can access:

- **API Base URL**: `http://localhost:5000`
- **Health Check**: `http://localhost:5000/health/live`
- **Swagger UI**: `http://localhost:5000/swagger` (development only)

### Key Endpoints
```
# Directory Management
GET    /api/v1/shops                    # List barber shops
GET    /api/v1/barbers                  # List barbers  
GET    /api/v1/services                 # List services

# Scheduling  
GET    /api/v1/barbers/{id}/availability  # Available slots
POST   /api/v1/appointments               # Book appointment
GET    /api/v1/appointments/{id}          # Appointment details

# Authentication
POST   /auth/register                     # User registration
POST   /auth/login                        # User login  
GET    /me                               # Current user info
```

## 🐛 Troubleshooting

### Common Issues

#### TypeScript Errors
If you see `error TS2688: Cannot find type definition file for 'node'`:
```bash
cd web/barberly-web
npm install @types/node --save-dev
npm run type-check
```

#### Vitest Coverage Errors  
If you see `CACError: Unknown option --watchAll`:
- Use `npm run test:coverage -- --run` instead of `-- --watchAll=false`
- Or configure in `vitest.config.ts` with `test: { watch: false }`

#### Database Connection Issues
```bash
# Check if PostgreSQL is running
docker ps

# Restart PostgreSQL container
docker restart barberly-postgres

# Re-run migrations
dotnet ef database update --project backend/src/Barberly.Infrastructure --startup-project backend/src/Barberly.Api
```

#### Port Conflicts
- Frontend dev server: change port in `vite.config.ts`
- Backend API: change port in `appsettings.Development.json`

### Verification Scripts
```bash
# Verify frontend build pipeline
cd web/barberly-web
npm run lint && npm run type-check && npm run build

# Verify backend build pipeline  
npm run backend:build && npm run backend:test
```

## 📁 Project Structure

```
barberly/
├── backend/src/           # .NET 8 backend
│   ├── Barberly.Api/      # HTTP API (Minimal APIs)
│   ├── Barberly.Application/  # CQRS handlers
│   ├── Barberly.Domain/   # Domain entities
│   └── Barberly.Infrastructure/  # EF Core, persistence
├── backend/tests/         # .NET tests
├── web/barberly-web/      # React 18 + TypeScript frontend
│   ├── src/components/    # UI components
│   ├── src/pages/         # Route components  
│   └── src/lib/api/       # API client
├── docs/                  # Documentation
└── package.json           # Root npm scripts
```

## 🔐 Authentication

The application uses JWT-based authentication:

1. Register: `POST /auth/register`
2. Login: `POST /auth/login` (returns JWT token)
3. Use token: `Authorization: Bearer <token>` header

### Test Users (after seeding)
- **Customer**: `customer@example.com` / `Password123!`
- **Barber**: `barber@example.com` / `Password123!`  
- **Shop Owner**: `owner@example.com` / `Password123!`

## 📈 Performance & Monitoring

- **Frontend**: Lighthouse CI configured for performance monitoring
- **Backend**: Built-in health checks at `/health/live` and `/health/ready`
- **Database**: EF Core with connection pooling and retry policies

## 🚢 Deployment

- **Frontend**: Static site deployment (Vite build output)
- **Backend**: .NET 8 self-contained deployment
- **Database**: PostgreSQL with EF Core migrations

---

For more detailed information, see the documentation in the `docs/` directory.
