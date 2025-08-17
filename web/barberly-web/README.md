# Barberly Frontend

Modern React 18 + TypeScript + Tailwind CSS frontend for the Barberly barber appointment system.

## 🚀 Tech Stack

- **React 18** with TypeScript
- **Vite** for fast development and building
- **TanStack Query** for server state management
- **React Hook Form + Zod** for forms and validation
- **shadcn/ui + Tailwind CSS** for UI components and styling
- **React Router v6** for navigation
- **Axios** for API client with interceptors
- **Vitest + Testing Library** for unit testing
- **Storybook** for component development
- **Playwright** for E2E testing

## 🛠️ Development Setup

### Prerequisites

- Node.js 18+
- npm or yarn
- Backend API running on http://localhost:5000

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

The app will be available at http://localhost:5173

### Available Scripts

```bash
# Development
npm run dev              # Start dev server
npm run build           # Build for production
npm run preview         # Preview production build

# Testing
npm test                # Run unit tests
npm run test:ui         # Run tests with UI
npm run test:coverage   # Run tests with coverage
npm run e2e             # Run E2E tests
npm run e2e:ui          # Run E2E tests with UI

# Code Quality
npm run lint            # Lint code
npm run type-check      # TypeScript type checking

# Storybook
npm run storybook       # Start Storybook
npm run build-storybook # Build Storybook

# API Types
npm run generate-api-types  # Generate TypeScript types from OpenAPI
```

## 📁 Project Structure

```
src/
├── components/          # Reusable UI components
│   └── ui/             # shadcn/ui components
├── pages/              # Page components
├── lib/                # Utilities and configurations
│   ├── api/            # API client and hooks
│   └── utils.ts        # Shared utilities
├── test/               # Test setup and utilities
└── App.tsx             # Main app component
```

## 🎨 UI Components

Built with **shadcn/ui** and **Tailwind CSS** for:
- ✅ Consistent design system
- ✅ Accessibility built-in
- ✅ Dark mode support
- ✅ Responsive design
- ✅ TypeScript-first

### Available Components

- `Button` - Primary UI actions
- `Card` - Content containers
- `Input` - Form inputs
- More components will be added as needed

## 🔌 API Integration

The frontend connects to the .NET 8 backend API with:

- **Type-safe API calls** using generated TypeScript types
- **TanStack Query** for caching, background refetching, and optimistic updates
- **Axios interceptors** for auth tokens and error handling
- **Proper error boundaries** and loading states

### Key API Endpoints

- `GET /api/v1/shops` - List barber shops
- `GET /api/v1/shops/{id}` - Shop details
- `GET /api/v1/barbers/{id}/availability` - Available time slots
- `POST /api/v1/appointments` - Book appointment (with idempotency)

## 🧪 Testing Strategy

### Unit Tests
- Component behavior with **Testing Library**
- Business logic with **Vitest**
- API integration with **MSW** mocking

### Integration Tests
- User workflows with **Testing Library**
- API integration with real endpoints

### E2E Tests
- Critical user journeys with **Playwright**
- Cross-browser testing
- Visual regression testing

## 🚦 Development Workflow

1. **Component Development**: Use Storybook for isolated component development
2. **API Integration**: Use TanStack Query hooks with proper loading/error states
3. **Form Handling**: React Hook Form + Zod for validation
4. **Styling**: Tailwind CSS utility classes with shadcn/ui components
5. **Testing**: Write tests alongside features

## 🔐 Authentication

Integration with Azure AD B2C for:
- User registration and login
- JWT token management
- Role-based access control
- Secure API communication

## 📱 Responsive Design

Mobile-first responsive design with:
- Tailwind CSS breakpoints
- Touch-friendly interactions
- Optimized for both mobile and desktop

## 🌐 Internationalization

Ready for i18next integration for:
- Multi-language support
- Date/time localization
- Currency formatting

## 🚀 Deployment

Production build optimized with:
- Code splitting
- Tree shaking
- Asset optimization
- PWA capabilities (planned)

---

Built with ❤️ for the Barberly project using modern web technologies.
