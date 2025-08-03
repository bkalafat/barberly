# React UI Geliştirme Prompt'ları (Copilot için)

Bu dosya, Barberly frontend geliştirme sürecinde Copilot'un tutarlı ve modern React kodu üretmesi için şablonlar içerir.

## RHF + Zod Form Kalıbı

```tsx
// components/forms/appointment-form.tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation } from '@tanstack/react-query';

const appointmentSchema = z.object({
  barberId: z.string().uuid('Geçerli bir berber seçin'),
  serviceId: z.string().uuid('Hizmet seçimi gerekli'),
  date: z.string().min(1, 'Tarih seçimi gerekli'),
  time: z.string().min(1, 'Saat seçimi gerekli'),
  notes: z.string().max(500, 'Notlar en fazla 500 karakter olabilir').optional(),
});

type AppointmentFormData = z.infer<typeof appointmentSchema>;

export function AppointmentForm() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<AppointmentFormData>({
    resolver: zodResolver(appointmentSchema),
  });

  const createAppointment = useMutation({
    mutationFn: (data: AppointmentFormData) =>
      api.post('/v1/appointments', data),
    onSuccess: () => {
      reset();
      // Toast success message
    },
    onError: (error) => {
      // Handle error
    },
  });

  const onSubmit = (data: AppointmentFormData) => {
    createAppointment.mutate(data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <label htmlFor="barberId" className="block text-sm font-medium">
          Berber Seçin
        </label>
        <select
          {...register('barberId')}
          className="mt-1 block w-full rounded-md border-gray-300"
          aria-invalid={errors.barberId ? 'true' : 'false'}
          aria-describedby={errors.barberId ? 'barberId-error' : undefined}
        >
          <option value="">Berber seçin...</option>
          {/* Options will be populated */}
        </select>
        {errors.barberId && (
          <p id="barberId-error" role="alert" className="mt-1 text-sm text-red-600">
            {errors.barberId.message}
          </p>
        )}
      </div>

      <button
        type="submit"
        disabled={isSubmitting || createAppointment.isPending}
        className="w-full py-2 px-4 bg-blue-600 text-white rounded-md disabled:opacity-50"
      >
        {isSubmitting ? 'Kaydediliyor...' : 'Randevu Oluştur'}
      </button>

      {createAppointment.isError && (
        <div role="alert" className="p-3 bg-red-50 border border-red-200 rounded-md">
          <p className="text-sm text-red-800">
            Randevu oluşturulamadı. Lütfen tekrar deneyin.
          </p>
        </div>
      )}
    </form>
  );
}
```

## TanStack Query Kullanımı

```ts
// hooks/use-barber-availability.ts
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';

interface UseBarberAvailabilityProps {
  barberId: string;
  date: string;
  serviceId?: string;
}

export function useBarberAvailability({
  barberId,
  date,
  serviceId
}: UseBarberAvailabilityProps) {
  return useQuery({
    queryKey: ['barbers', barberId, 'availability', date, serviceId],
    queryFn: () =>
      api.get(`/v1/barbers/${barberId}/availability`, {
        params: { date, serviceId },
      }).then(response => response.data),
    enabled: Boolean(barberId && date),
    staleTime: 60_000, // 1 minute
    retry: 2,
  });
}

// hooks/use-shops.ts
export function useShops(filters?: ShopFilters) {
  return useQuery({
    queryKey: ['shops', filters],
    queryFn: () =>
      api.get('/v1/shops', { params: filters }).then(r => r.data),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

// hooks/use-appointment-mutation.ts
export function useCreateAppointment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateAppointmentRequest) =>
      api.post('/v1/appointments', data, {
        headers: {
          'Idempotency-Key': crypto.randomUUID(),
        },
      }),
    onSuccess: (data, variables) => {
      // Invalidate and refetch
      queryClient.invalidateQueries({
        queryKey: ['barbers', variables.barberId, 'availability'],
      });
      queryClient.invalidateQueries({
        queryKey: ['appointments', 'my'],
      });
    },
  });
}
```

## Component Pattern (Shadcn/ui uyumlu)

```tsx
// components/hair-profile/hair-profile-wizard.tsx
import { useState } from 'react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

interface HairProfileWizardProps {
  onComplete: (profile: HairProfile) => void;
  className?: string;
}

export function HairProfileWizard({ onComplete, className }: HairProfileWizardProps) {
  const [currentStep, setCurrentStep] = useState(0);
  const [profile, setProfile] = useState<Partial<HairProfile>>({});

  const steps = [
    { title: 'Saç Tipi', component: HairTypeStep },
    { title: 'Doku & Yoğunluk', component: TextureDensityStep },
    { title: 'Tercihler', component: PreferencesStep },
    { title: 'Açıklama & Fotoğraf', component: DescriptionStep },
  ];

  const CurrentStepComponent = steps[currentStep].component;

  const handleNext = () => {
    if (currentStep < steps.length - 1) {
      setCurrentStep(prev => prev + 1);
    } else {
      onComplete(profile as HairProfile);
    }
  };

  const handleBack = () => {
    if (currentStep > 0) {
      setCurrentStep(prev => prev - 1);
    }
  };

  return (
    <Card className={cn('w-full max-w-2xl mx-auto', className)}>
      <CardHeader>
        <CardTitle>Saç Profili Oluşturma</CardTitle>
        <div className="flex space-x-2">
          {steps.map((_, index) => (
            <div
              key={index}
              className={cn(
                'h-2 flex-1 rounded',
                index <= currentStep ? 'bg-blue-600' : 'bg-gray-200'
              )}
              aria-label={`Adım ${index + 1}${index <= currentStep ? ' tamamlandı' : ''}`}
            />
          ))}
        </div>
      </CardHeader>

      <CardContent className="space-y-6">
        <CurrentStepComponent
          value={profile}
          onChange={setProfile}
        />

        <div className="flex justify-between">
          <Button
            variant="outline"
            onClick={handleBack}
            disabled={currentStep === 0}
          >
            Geri
          </Button>
          <Button onClick={handleNext}>
            {currentStep === steps.length - 1 ? 'Tamamla' : 'İleri'}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
```

## Error Boundary & Loading States

```tsx
// components/error-boundary.tsx
import { Component, ReactNode } from 'react';
import { Button } from '@/components/ui/button';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback || (
        <div className="flex flex-col items-center justify-center p-8 text-center">
          <h2 className="text-lg font-semibold mb-2">Bir hata oluştu</h2>
          <p className="text-gray-600 mb-4">
            Beklenmeyen bir hata oluştu. Lütfen sayfayı yenileyin.
          </p>
          <Button onClick={() => window.location.reload()}>
            Sayfayı Yenile
          </Button>
        </div>
      );
    }

    return this.props.children;
  }
}

// components/loading-spinner.tsx
export function LoadingSpinner({ size = 'md' }: { size?: 'sm' | 'md' | 'lg' }) {
  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-8 h-8',
    lg: 'w-12 h-12',
  };

  return (
    <div className="flex justify-center items-center">
      <div
        className={cn(
          'animate-spin rounded-full border-2 border-gray-300 border-t-blue-600',
          sizeClasses[size]
        )}
        role="status"
        aria-label="Yükleniyor"
      >
        <span className="sr-only">Yükleniyor...</span>
      </div>
    </div>
  );
}
```

## Custom Hooks Pattern

```ts
// hooks/use-local-storage.ts
import { useState, useEffect } from 'react';

export function useLocalStorage<T>(key: string, initialValue: T) {
  const [storedValue, setStoredValue] = useState<T>(() => {
    try {
      const item = window.localStorage.getItem(key);
      return item ? JSON.parse(item) : initialValue;
    } catch (error) {
      console.error(`Error reading localStorage key "${key}":`, error);
      return initialValue;
    }
  });

  const setValue = (value: T | ((val: T) => T)) => {
    try {
      const valueToStore = value instanceof Function ? value(storedValue) : value;
      setStoredValue(valueToStore);
      window.localStorage.setItem(key, JSON.stringify(valueToStore));
    } catch (error) {
      console.error(`Error setting localStorage key "${key}":`, error);
    }
  };

  return [storedValue, setValue] as const;
}

// hooks/use-debounce.ts
import { useState, useEffect } from 'react';

export function useDebounce<T>(value: T, delay: number) {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}
```

## Erişilebilirlik & Best Practices

```tsx
// components/accessible-button.tsx
import { forwardRef } from 'react';
import { cn } from '@/lib/utils';

interface AccessibleButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
}

export const AccessibleButton = forwardRef<HTMLButtonElement, AccessibleButtonProps>(
  ({ className, variant = 'primary', size = 'md', loading, children, ...props }, ref) => {
    return (
      <button
        ref={ref}
        className={cn(
          'inline-flex items-center justify-center rounded-md font-medium transition-colors',
          'focus:outline-none focus:ring-2 focus:ring-offset-2',
          'disabled:opacity-50 disabled:pointer-events-none',
          {
            'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500': variant === 'primary',
            'bg-gray-200 text-gray-900 hover:bg-gray-300 focus:ring-gray-500': variant === 'secondary',
            'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500': variant === 'danger',
          },
          {
            'px-2 py-1 text-sm': size === 'sm',
            'px-4 py-2': size === 'md',
            'px-6 py-3 text-lg': size === 'lg',
          },
          className
        )}
        disabled={loading || props.disabled}
        aria-busy={loading}
        {...props}
      >
        {loading && (
          <LoadingSpinner size="sm" />
        )}
        <span className={loading ? 'ml-2' : ''}>{children}</span>
      </button>
    );
  }
);

AccessibleButton.displayName = 'AccessibleButton';
```

## Environment & Configuration

```ts
// lib/env.ts
import { z } from 'zod';

const envSchema = z.object({
  VITE_API_URL: z.string().url('API URL gerekli'),
  VITE_AUTH_DOMAIN: z.string().min(1, 'Auth domain gerekli'),
  VITE_APP_ENV: z.enum(['development', 'staging', 'production']).default('development'),
});

export const env = envSchema.parse(import.meta.env);

// lib/api.ts
import axios from 'axios';
import { env } from './env';

export const api = axios.create({
  baseURL: env.VITE_API_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for auth
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```’ları (Copilot için)

## RHF + Zod Form Kalıbı

```tsx
const schema = z.object({ ... });
const { register, handleSubmit } = useForm({ resolver: zodResolver(schema) });
```

## TanStack Query Kullanımı

```ts
const { data } = useQuery({ queryKey: [...], queryFn: ... });
```

## Erişilebilirlik & Hata Durumları
- WAI-ARIA, role="alert", odak yönetimi
