# Barberly

Modern barber shop management and appointment booking platform built with .NET 8 and React 18.

> **🤖 AI-Powered Development**: ~80% of this codebase was generated using GitHub Co- **API Documentation** - Available via Swagger UI at `http://localhost:5156/swagger` when running the backendilot with Claude Sonnet 4 and GPT 5 mini (preview) in Beast Mode for rapid development.

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/bkalafat/barberly)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download)
[![React](https://img.shields.io/badge/React-18-blue.svg)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-blue.svg)](https://www.typescriptlang.org/)

## Table of Contents

- [About](#about)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Testing Strategy](#testing-strategy)
- [Development Status](#development-status)
- [Documentation & Resources](#documentation--resources)
- [Contributing](#contributing)
- [License](#license)

## About

Barberly is a production-ready, full-stack barber shop management platform that streamlines appointment booking and customer management. Built with **Domain-Driven Design (DDD) + Clean Architecture + CQRS**, it provides enterprise-grade scalability and maintainability.

### Key Features

- ✅ **Complete Appointment System** - Booking workflow with availability checking, conflict detection, and idempotency
- ✅ **Directory Management** - Full CRUD for shops, barbers, and services with advanced search
- ✅ **JWT Authentication** - Role-based authorization (Customer, Barber, ShopOwner, Admin)
- ✅ **Responsive Frontend** - Modern React SPA with optimized UX
- ✅ **Comprehensive Testing** - Full integration and unit test coverage
- ✅ **Enterprise Security** - Rate limiting, CORS, input validation, security headers
- ✅ **Production Database** - PostgreSQL with automated migrations and seeding

## Tech Stack

**Architecture**: Domain-Driven Design (DDD) + Clean Architecture + CQRS + Minimal APIs

:::mermaid
graph LR
    A[React 18 + TypeScript] -->|HTTP| B[.NET 8 Minimal APIs]
    B --> C[EF Core 8]
    C --> D[PostgreSQL 16]
    B --> E[OpenTelemetry]
    B --> F[Azure Service Bus]
:::

### Backend
- **.NET 8** - Minimal APIs with OpenAPI documentation
- **PostgreSQL 16** - Primary database with EF Core 8 ORM
- **MediatR** - CQRS pattern implementation
- **OpenTelemetry** - Observability and structured logging
- **Azure Service Bus** - Messaging and notifications

### Frontend
- **React 18** - Modern UI framework with TypeScript
- **Vite** - Fast build tool and dev server
- **TanStack Query** - Server state management
- **React Hook Form + Zod** - Form handling and validation
- **Tailwind CSS + shadcn/ui** - Utility-first styling with component library
- **Vitest + Playwright** - Unit and E2E testing

## 🚀 Getting Started

### Prerequisites

Ensure you have these tools installed:

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | `8.0+` | Backend development & runtime |
| [Node.js](https://nodejs.org/) | `18+` | Frontend tooling & development |
| [PostgreSQL](https://postgresql.org/) | `15+` | Primary database (or use Docker) |
| [Git](https://git-scm.com/) | `latest` | Version control |

### Installation

```bash
# 1. Clone repository
git clone https://github.com/yourusername/barberly.git
cd barberly

# 2. Database Setup (choose one option)
# Create database 'BarberlyDb' with connection string in appsettings.Development.json

# 3. Backend Setup
npm run backend:build     # Build solution
npm run backend:run       # Start API (http://localhost:5156)

# 4. Frontend Setup (new terminal)
npm install               # Install all dependencies
npm run dev               # Start dev server (http://localhost:5173)
```

### Quick Verification

| Service | URL | Status |
|---------|-----|---------|
| Frontend App | http://localhost:5173 | React dev server |
| Backend API | http://localhost:5156 | .NET API |
| API Docs | http://localhost:5156/swagger | OpenAPI documentation |
| Health Check | http://localhost:5156/health/ready | System status |

## 📋 API Documentation

### Core Endpoints

#### Directory Management

- `GET /api/v1/shops` - List all barber shops with filtering
- `GET /api/v1/shops/{id}` - Get shop details
- `POST /api/v1/shops` - Create new shop (auth required)
- `GET /api/v1/barbers` - List barbers with search functionality
- `GET /api/v1/barbers/{id}` - Get barber profile details
- `GET /api/v1/services` - List available services

#### Appointment Scheduling

- `GET /api/v1/barbers/{id}/availability` - Check available time slots
- `POST /api/v1/appointments` - Book appointment (**requires Idempotency-Key header**)
- `GET /api/v1/appointments/{id}` - Get appointment details
- `DELETE /api/v1/appointments/{id}` - Cancel appointment

#### Authentication & User Management

- `POST /auth/register` - User registration with role assignment
- `POST /auth/login` - User authentication (returns JWT)
- `GET /me` - Current authenticated user information

#### Health & Monitoring

- `GET /health/live` - Liveness probe for monitoring
- `GET /health/ready` - Readiness probe for load balancers

## 🧪 Testing Strategy - ALL PASSING ✅

### Backend Testing

```bash
# Run all .NET tests
npm run backend:test

# Specific test suites
dotnet test backend/tests/Barberly.IntegrationTests/    # Integration tests
dotnet test backend/tests/Barberly.Application.Tests/  # Unit tests
dotnet test backend/tests/Barberly.Domain.Tests/       # Domain tests
```

### Frontend Testing

```bash
# Unit tests with Vitest
npm run test

# Test with coverage report
npm run test:coverage

# End-to-end tests with Playwright
npm run e2e
```

### Full Development Verification

```bash
# Complete quality check pipeline
npm run lint          # ESLint + Prettier
npm run type-check     # TypeScript validation
npm run test           # Unit tests
npm run build          # Production build test
```

## 📊 Development Status

✅ **Authentication & Authorization** - Complete JWT implementation with role-based policies  
✅ **Directory Management** - Full CRUD for shops, barbers, and services with advanced search  
✅ **Appointment Scheduling** - Complete booking system with conflict detection and idempotency  
✅ **Database Architecture** - PostgreSQL with EF Core, migrations, and comprehensive seeding  
✅ **Frontend Application** - Responsive React SPA with modern UX patterns  
✅ **Testing Coverage** - Comprehensive integration and unit test suites  
✅ **Security Implementation** - Rate limiting, CORS, security headers, and input validation  
✅ **API Documentation** - OpenAPI/Swagger integration with detailed endpoint documentation  
🔄 **Performance Optimization** - Query optimization and response caching (in progress)  
🚧 **Customer Hair Profiles** - Advanced customer preference system (planned)  
🚧 **Real-time Notifications** - Push notification system (planned)

## 📚 Documentation & Resources

### Essential Documentation

- **[Developer Guide](.github/copilot-instructions.md)** - Comprehensive development patterns and conventions
- **[High-Level Plan](.github/plan/01-high-level-plan.md)** - Strategic development roadmap and architecture decisions
- **[Detailed Planning](.github/plan/plan.md)** - Implementation steps and development milestones
- **API Documentation** - Available via Swagger UI at `http://localhost:5156/swagger` when running the backend

### Key Development Notes

- **Idempotency**: All appointment operations require `Idempotency-Key` header for safe retries
- **Time Zones**: All appointment times stored and processed in UTC
- **Database Migrations**: Use descriptive names with timestamp prefixes
- **Query Optimization**: Follow pattern `['entity', ...params]` for TanStack Query cache keys
- **Error Handling**: All APIs return RFC 7807 ProblemDetails format
- **CORS Configuration**: Frontend dev server (localhost:5173) is pre-configured

## 🤝 Contributing

### Development Workflow

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** using conventional commits (`git commit -m 'feat: add amazing feature'`)
4. **Test** thoroughly (`npm run lint && npm run test && npm run backend:test`)
5. **Push** to your branch (`git push origin feature/amazing-feature`)
6. **Open** a Pull Request with detailed description

### Code Standards

- Follow the established **DDD + Clean Architecture** patterns
- Maintain **comprehensive test coverage** (unit + integration)
- Use **TypeScript** strictly with proper type definitions
- Follow **conventional commit** format for all commits
- Ensure all **CI/CD pipeline** checks pass before PR submission

## 📝 License

MIT License - see [LICENSE](LICENSE) file for details.

---

### 🎯 Project Highlights

- **🚀 Rapid Development**: Built using AI-assisted development with GitHub Copilot
- **🏗️ Enterprise Architecture**: Following DDD + Clean Architecture + CQRS best practices
- **✅ Production Ready**: Comprehensive testing, security, and performance optimization
- **🔧 Developer Experience**: Modern tooling with excellent DX and comprehensive documentation
- **📱 Modern Stack**: Latest .NET 8, React 18, and TypeScript with cutting-edge libraries

> 💡 **Development Note**: This project demonstrates the power of AI-assisted development while maintaining high code quality, comprehensive testing, and enterprise-grade architecture patterns. The majority of the codebase was generated and refined through intelligent collaboration between human expertise and AI capabilities.
