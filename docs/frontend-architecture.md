# Barberly Frontend Architecture

## Overview
Barberly frontend is a modern React 18 single-page application (SPA) built with TypeScript and Vite. The architecture follows component-based design principles, emphasizes type safety, and provides excellent developer experience with fast build times and hot module replacement.

## Technology Stack
- **React 18** - UI library with concurrent features
- **TypeScript** - Type-safe JavaScript with strict mode
- **Vite** - Fast build tool and dev server
- **TanStack Query (React Query)** - Server state management
- **React Hook Form (RHF)** - Form handling and validation
- **Zod** - Schema validation and type inference
- **Tailwind CSS** - Utility-first CSS framework
- **shadcn/ui** - Re-usable component library
- **MSAL.js** - Microsoft Authentication Library for Azure AD B2C
- **i18next** - Internationalization framework
- **Zustand** - Lightweight state management
- **Axios** - HTTP client with interceptors
- **React Router** - Client-side routing
- **Playwright** - End-to-end testing
- **Vitest** - Unit testing framework
- **ESLint + Prettier** - Code quality and formatting

## Project Structure

```
web/barberly-web/
├── public/                        # Static assets
├── src/
│   ├── components/               # Reusable UI components
│   │   ├── ui/                  # shadcn/ui base components
│   │   ├── forms/               # Form-specific components
│   │   ├── layout/              # Layout components
│   │   └── common/              # Common utility components
│   ├── pages/                   # Page components (route containers)
│   │   ├── auth/               # Authentication pages
│   │   ├── dashboard/          # Dashboard pages
│   │   ├── appointments/       # Appointment management
│   │   ├── profile/            # User profile pages
│   │   └── barbers/           # Barber-related pages
│   ├── hooks/                  # Custom React hooks
│   │   ├── api/               # API-related hooks
│   │   ├── auth/              # Authentication hooks
│   │   └── common/            # Utility hooks
│   ├── lib/                    # Core utilities and configurations
│   │   ├── api/               # API client and types
│   │   ├── auth/              # MSAL configuration
│   │   ├── validation/        # Zod schemas
│   │   └── utils/             # Helper functions
│   ├── stores/                 # Zustand state stores
│   ├── types/                  # TypeScript type definitions
│   ├── i18n/                   # Internationalization resources
│   └── styles/                 # Global styles and Tailwind config
├── tests/                      # Test files
│   ├── e2e/                   # Playwright E2E tests
│   ├── unit/                  # Unit tests
│   └── setup/                 # Test configuration
├── package.json
├── vite.config.ts
├── tailwind.config.js
├── tsconfig.json
└── playwright.config.ts
```

## Architecture Patterns

### Component Architecture
The frontend follows a hierarchical component structure with clear separation of concerns:

```
Pages (Route Containers)
    ↓
Layout Components
    ↓
Feature Components
    ↓
UI Components (shadcn/ui)
    ↓
Base HTML Elements
```

### State Management Strategy

#### Server State (TanStack Query)
For API data, caching, synchronization, and background updates:

```typescript
// Query example
const useBarbers = (filters?: BarberFilters) =>
  useQuery({
    queryKey: ['barbers', filters],
    queryFn: () => api.barbers.getAll(filters),
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
  });

// Mutation example
const useCreateAppointment = () =>
  useMutation({
    mutationFn: (data: CreateAppointmentData) => api.appointments.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries(['appointments']);
      toast.success('Randevu başarıyla oluşturuldu!');
    },
  });
```

#### Client State (Zustand)
For UI state, user preferences, and temporary data:

```typescript
interface AppStore {
  // UI state
  sidebarOpen: boolean;
  theme: 'light' | 'dark';

  // User preferences
  language: string;
  dateFormat: string;

  // Temporary state
  selectedBarber: Barber | null;

  // Actions
  toggleSidebar: () => void;
  setTheme: (theme: 'light' | 'dark') => void;
  setSelectedBarber: (barber: Barber | null) => void;
}

const useAppStore = create<AppStore>((set) => ({
  sidebarOpen: false,
  theme: 'light',
  language: 'tr',
  dateFormat: 'DD/MM/YYYY',
  selectedBarber: null,

  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
  setTheme: (theme) => set({ theme }),
  setSelectedBarber: (barber) => set({ selectedBarber: barber }),
}));
```

## Authentication & Authorization

### MSAL Configuration
Azure AD B2C integration using MSAL.js:

```typescript
// lib/auth/msal-config.ts
export const msalConfig = {
  auth: {
    clientId: env.VITE_AUTH_CLIENT_ID,
    authority: env.VITE_AUTH_AUTHORITY,
    knownAuthorities: [env.VITE_AUTH_KNOWN_AUTHORITIES],
    redirectUri: env.VITE_AUTH_REDIRECT_URI,
    postLogoutRedirectUri: env.VITE_AUTH_POST_LOGOUT_REDIRECT_URI,
  },
  cache: {
    cacheLocation: 'localStorage' as const,
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level: number, message: string, containsPii: boolean) => {
        if (containsPii) return;
        console.log(`MSAL [${level}]: ${message}`);
      },
      logLevel: env.VITE_APP_ENV === 'development' ? 3 : 1,
    },
  },
};
```

### Protected Routes
Route protection based on authentication and roles:

```typescript
// components/auth/ProtectedRoute.tsx
interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
  fallback?: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requiredRoles = [],
  fallback = <Navigate to="/auth/login" replace />
}) => {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return fallback;
  }

  if (requiredRoles.length > 0 && !hasRequiredRole(user, requiredRoles)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
};
```

## Form Handling & Validation

### React Hook Form + Zod Integration
Type-safe forms with validation:

```typescript
// Zod schema
const createAppointmentSchema = z.object({
  barberId: z.string().uuid('Berber seçiniz'),
  serviceId: z.string().uuid('Hizmet seçiniz'),
  date: z.string().min(1, 'Tarih seçiniz'),
  timeSlot: z.string().min(1, 'Saat seçiniz'),
  notes: z.string().max(500, 'Not 500 karakterden uzun olamaz').optional(),
});

type CreateAppointmentForm = z.infer<typeof createAppointmentSchema>;

// Form component
export const CreateAppointmentForm: React.FC = () => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    control,
  } = useForm<CreateAppointmentForm>({
    resolver: zodResolver(createAppointmentSchema),
  });

  const createAppointment = useCreateAppointment();

  const onSubmit = async (data: CreateAppointmentForm) => {
    try {
      await createAppointment.mutateAsync(data);
      // Handle success
    } catch (error) {
      // Handle error
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {/* Form fields */}
    </form>
  );
};
```

## API Integration

### Type-Safe API Client
OpenAPI-generated types with Axios interceptors:

```typescript
// lib/api/client.ts
const apiClient = axios.create({
  baseURL: env.VITE_API_URL,
  timeout: 10000,
});

// Request interceptor for auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      await handleTokenRefresh();
    }
    return Promise.reject(error);
  }
);

// API service example
export const barbersApi = {
  getAll: (filters?: BarberFilters): Promise<Barber[]> =>
    apiClient.get('/v1/barbers', { params: filters }).then(res => res.data),

  getById: (id: string): Promise<Barber> =>
    apiClient.get(`/v1/barbers/${id}`).then(res => res.data),

  getAvailability: (id: string, date: string, serviceId: string): Promise<TimeSlot[]> =>
    apiClient.get(`/v1/barbers/${id}/availability`, {
      params: { date, serviceId }
    }).then(res => res.data),
};
```

## UI Component Library

### shadcn/ui Integration
Customizable, accessible components:

```typescript
// components/ui/button.tsx
const buttonVariants = cva(
  "inline-flex items-center justify-center rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-50 disabled:pointer-events-none ring-offset-background",
  {
    variants: {
      variant: {
        default: "bg-primary text-primary-foreground hover:bg-primary/90",
        destructive: "bg-destructive text-destructive-foreground hover:bg-destructive/90",
        outline: "border border-input hover:bg-accent hover:text-accent-foreground",
        secondary: "bg-secondary text-secondary-foreground hover:bg-secondary/80",
        ghost: "hover:bg-accent hover:text-accent-foreground",
        link: "underline-offset-4 hover:underline text-primary",
      },
      size: {
        default: "h-10 py-2 px-4",
        sm: "h-9 px-3 rounded-md",
        lg: "h-11 px-8 rounded-md",
        icon: "h-10 w-10",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
);
```

### Tailwind CSS Configuration
Custom design system:

```javascript
// tailwind.config.js
module.exports = {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        // Custom Barberly colors
        barberly: {
          50: '#f0f9ff',
          500: '#3b82f6',
          900: '#1e3a8a',
        },
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
};
```

## Routing & Navigation

### React Router Configuration
Type-safe routing with lazy loading:

```typescript
// App.tsx
const router = createBrowserRouter([
  {
    path: '/',
    element: <RootLayout />,
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: 'auth',
        children: [
          {
            path: 'login',
            element: <LoginPage />,
          },
          {
            path: 'register',
            element: <RegisterPage />,
          },
          {
            path: 'callback',
            element: <AuthCallbackPage />,
          },
        ],
      },
      {
        path: 'dashboard',
        element: <ProtectedRoute><DashboardLayout /></ProtectedRoute>,
        children: [
          {
            index: true,
            element: <DashboardPage />,
          },
          {
            path: 'appointments',
            element: <AppointmentsPage />,
          },
          {
            path: 'profile',
            element: <ProfilePage />,
          },
        ],
      },
      {
        path: 'barbers',
        children: [
          {
            index: true,
            element: <BarbersListPage />,
          },
          {
            path: ':id',
            element: <BarberDetailPage />,
          },
        ],
      },
    ],
  },
]);
```

## Performance Optimization

### Code Splitting & Lazy Loading
Route-based and component-based splitting:

```typescript
// Lazy loaded pages
const DashboardPage = lazy(() => import('../pages/dashboard/DashboardPage'));
const AppointmentsPage = lazy(() => import('../pages/appointments/AppointmentsPage'));

// Lazy loaded components
const HairProfileWizard = lazy(() => import('../components/hair-profile/HairProfileWizard'));

// Usage with Suspense
<Suspense fallback={<LoadingSpinner />}>
  <HairProfileWizard />
</Suspense>
```

### React Query Optimization
Efficient data fetching and caching:

```typescript
// Prefetching
const prefetchBarberData = async (barberId: string) => {
  await queryClient.prefetchQuery({
    queryKey: ['barbers', barberId],
    queryFn: () => barbersApi.getById(barberId),
  });
};

// Background updates
const useBarberWithBackground = (id: string) =>
  useQuery({
    queryKey: ['barbers', id],
    queryFn: () => barbersApi.getById(id),
    staleTime: 5 * 60 * 1000,
    refetchOnWindowFocus: true,
    refetchInterval: 30 * 60 * 1000, // 30 minutes
  });
```

## Internationalization (i18n)

### i18next Configuration
Multi-language support:

```typescript
// i18n/index.ts
i18n
  .use(Backend)
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: 'tr',
    lng: 'tr',

    interpolation: {
      escapeValue: false,
    },

    resources: {
      tr: {
        common: require('./locales/tr/common.json'),
        auth: require('./locales/tr/auth.json'),
        appointments: require('./locales/tr/appointments.json'),
      },
      en: {
        common: require('./locales/en/common.json'),
        auth: require('./locales/en/auth.json'),
        appointments: require('./locales/en/appointments.json'),
      },
    },
  });

// Usage in components
const { t } = useTranslation('auth');
return <h1>{t('login.title')}</h1>;
```

## Testing Strategy

### Unit Testing (Vitest)
Component and hook testing:

```typescript
// tests/unit/components/AppointmentCard.test.tsx
describe('AppointmentCard', () => {
  it('should render appointment details correctly', () => {
    const appointment = createMockAppointment();

    render(<AppointmentCard appointment={appointment} />);

    expect(screen.getByText(appointment.barber.name)).toBeInTheDocument();
    expect(screen.getByText(appointment.service.name)).toBeInTheDocument();
  });

  it('should handle cancel appointment', async () => {
    const onCancel = vi.fn();
    const appointment = createMockAppointment();

    render(<AppointmentCard appointment={appointment} onCancel={onCancel} />);

    await user.click(screen.getByRole('button', { name: /iptal/i }));

    expect(onCancel).toHaveBeenCalledWith(appointment.id);
  });
});
```

### E2E Testing (Playwright)
User journey testing:

```typescript
// tests/e2e/appointment-flow.spec.ts
test('user can create an appointment', async ({ page }) => {
  await page.goto('/auth/login');

  // Login
  await page.fill('[data-testid="email"]', 'test@example.com');
  await page.fill('[data-testid="password"]', 'password123');
  await page.click('[data-testid="login-button"]');

  // Navigate to barbers
  await page.click('[data-testid="barbers-link"]');

  // Select barber
  await page.click('[data-testid="barber-card"]:first-child');

  // Book appointment
  await page.click('[data-testid="book-appointment"]');
  await page.selectOption('[data-testid="service-select"]', 'haircut');
  await page.fill('[data-testid="date-input"]', '2024-12-25');
  await page.click('[data-testid="time-slot"]:first-child');
  await page.click('[data-testid="confirm-booking"]');

  // Verify success
  await expect(page.getByText('Randevu başarıyla oluşturuldu')).toBeVisible();
});
```

## Build & Deployment

### Vite Configuration
Optimized build setup:

```typescript
// vite.config.ts
export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
          ui: ['@radix-ui/react-dialog', '@radix-ui/react-dropdown-menu'],
          auth: ['@azure/msal-browser', '@azure/msal-react'],
        },
      },
    },
    chunkSizeWarningLimit: 1000,
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
});
```

### Environment Configuration
Type-safe environment variables:

```typescript
// lib/config.ts
const envSchema = z.object({
  VITE_API_URL: z.string().url(),
  VITE_AUTH_CLIENT_ID: z.string().uuid(),
  VITE_AUTH_AUTHORITY: z.string().url(),
  VITE_AUTH_KNOWN_AUTHORITIES: z.string(),
  VITE_AUTH_REDIRECT_URI: z.string().url(),
  VITE_AUTH_POST_LOGOUT_REDIRECT_URI: z.string().url(),
  VITE_APP_ENV: z.enum(['development', 'staging', 'production']),
});

export const env = envSchema.parse({
  VITE_API_URL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  VITE_AUTH_CLIENT_ID: import.meta.env.VITE_AUTH_CLIENT_ID,
  VITE_AUTH_AUTHORITY: import.meta.env.VITE_AUTH_AUTHORITY,
  VITE_AUTH_KNOWN_AUTHORITIES: import.meta.env.VITE_AUTH_KNOWN_AUTHORITIES,
  VITE_AUTH_REDIRECT_URI: import.meta.env.VITE_AUTH_REDIRECT_URI,
  VITE_AUTH_POST_LOGOUT_REDIRECT_URI: import.meta.env.VITE_AUTH_POST_LOGOUT_REDIRECT_URI,
  VITE_APP_ENV: import.meta.env.VITE_APP_ENV || 'development',
});
```

## Development Guidelines

### TypeScript Best Practices
- **Strict Mode**: Enabled for maximum type safety
- **No Any**: Avoid `any` type, use proper typing
- **Utility Types**: Leverage Pick, Omit, Partial, etc.
- **Generic Constraints**: Use bounded generics where appropriate

### Component Guidelines
- **Single Responsibility**: One component, one purpose
- **Props Interface**: Always define proper prop types
- **Default Props**: Use defaultProps or default parameters
- **Error Boundaries**: Implement for error handling

### Performance Guidelines
- **React.memo**: For expensive components
- **useMemo/useCallback**: For expensive calculations
- **Virtualization**: For large lists
- **Bundle Analysis**: Regular bundle size monitoring

### Accessibility (a11y)
- **Semantic HTML**: Use proper HTML elements
- **ARIA Labels**: Provide screen reader support
- **Keyboard Navigation**: Ensure full keyboard accessibility
- **Color Contrast**: Meet WCAG guidelines

## Security Considerations

### XSS Prevention
- **Input Sanitization**: Sanitize user inputs
- **Content Security Policy**: Implement CSP headers
- **Dangerous HTML**: Avoid dangerouslySetInnerHTML

### Token Management
- **Secure Storage**: Use httpOnly cookies or secure localStorage
- **Token Refresh**: Implement automatic refresh
- **Logout Cleanup**: Clear all tokens on logout

## Monitoring & Analytics

### Error Tracking
```typescript
// lib/monitoring.ts
export const errorLogger = {
  logError: (error: Error, context?: Record<string, any>) => {
    console.error('Frontend Error:', error, context);
    // Send to monitoring service (Sentry, LogRocket, etc.)
  },

  logUserAction: (action: string, data?: Record<string, any>) => {
    console.log('User Action:', action, data);
    // Send to analytics service
  },
};
```

### Performance Monitoring
- **Core Web Vitals**: Monitor LCP, FID, CLS
- **Bundle Size**: Track bundle growth
- **Load Times**: Monitor page load performance
- **API Response Times**: Track backend performance

---

> This document serves as the single source of truth for Barberly frontend architecture. Keep it updated as the system evolves.
