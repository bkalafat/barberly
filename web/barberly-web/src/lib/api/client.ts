import axios, { type AxiosResponse } from 'axios';

// API types - will be generated later with openapi-typescript
export interface CreateAppointmentRequest {
  userId: string;
  barberId: string;
  serviceId: string;
  start: string;
  end: string;
}

export interface AvailabilitySlot {
  start: string;
  end: string;
}

export interface Barber {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  barberShopId: string;
  yearsOfExperience: number;
  bio?: string;
  profileImageUrl?: string;
  isActive: boolean;
  averageRating: number;
  totalReviews: number;
  createdAt: string;
  updatedAt?: string;
}

export interface BarberShop {
  id: string;
  name: string;
  description: string;
  address: {
    street: string;
    city: string;
    state: string;
    postalCode: string;
    country: string;
  };
  phone: string;
  email: string;
  website?: string;
  isActive: boolean;
  openTime: string;
  closeTime: string;
  workingDays: string[];
  latitude: number;
  longitude: number;
  averageRating: number;
  totalReviews: number;
  createdAt: string;
  updatedAt?: string;
}

const api = axios.create({
  baseURL: (import.meta.env.VITE_API_URL || 'http://localhost:5156') + '/api/v1',
  timeout: 10000,
});

// Request interceptor for auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth-token');
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
      localStorage.removeItem('auth-token');
      window.location.href = '/auth';
    }
    return Promise.reject(error);
  }
);

export { api };

// API functions
export const barbersApi = {
  getAvailability: (
    barberId: string,
    date: string,
    serviceId?: string
  ): Promise<AxiosResponse<AvailabilitySlot[]>> =>
    api.get(`/barbers/${barberId}/availability`, { params: { date, serviceId } }),

  getById: (id: string): Promise<AxiosResponse<Barber>> => api.get(`/barbers/${id}`),

  getAll: (params?: {
    barberShopId?: string;
    serviceName?: string;
  }): Promise<AxiosResponse<Barber[]>> => api.get('/barbers', { params }),
};

export const appointmentsApi = {
  create: (
    data: CreateAppointmentRequest,
    idempotencyKey: string
  ): Promise<AxiosResponse<{ id: string }>> =>
    api.post('/appointments', data, { headers: { 'Idempotency-Key': idempotencyKey } }),
};

export const shopsApi = {
  getAll: (params?: {
    latitude?: number;
    longitude?: number;
    radiusKm?: number;
  }): Promise<AxiosResponse<BarberShop[]>> => api.get('/shops', { params }),

  getById: (id: string): Promise<AxiosResponse<BarberShop>> => api.get(`/shops/${id}`),
};
