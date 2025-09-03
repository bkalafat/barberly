import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  appointmentsApi,
  barbersApi,
  shopsApi,
  type Barber,
  type CreateAppointmentRequest,
} from './client';

// Query keys factory
export const queryKeys = {
  barbers: {
    all: ['barbers'] as const,
    byId: (id: string) => ['barbers', id] as const,
    availability: (barberId: string, date: string, serviceId?: string) =>
      ['barbers', barberId, 'availability', date, serviceId] as const,
  },
  shops: {
    all: ['shops'] as const,
    byId: (id: string) => ['shops', id] as const,
    nearby: (lat?: number, lng?: number, radius?: number) =>
      ['shops', 'nearby', lat, lng, radius] as const,
  },
  appointments: {
    all: ['appointments'] as const,
    byId: (id: string) => ['appointments', id] as const,
  },
};

// Barber hooks
export const useBarber = (id: string) => {
  return useQuery({
    queryKey: queryKeys.barbers.byId(id),
    queryFn: () => barbersApi.getById(id).then((res) => res.data),
    enabled: !!id,
  });
};

export const useBarbers = (params?: { barberShopId?: string; serviceName?: string }) => {
  return useQuery({
    queryKey: [...queryKeys.barbers.all, params],
    queryFn: (): Promise<Barber[]> => barbersApi.getAll(params).then((res) => res.data),
    staleTime: 5 * 60_000, // 5 minutes
  });
};

export const useAvailability = (barberId: string, date: string, serviceId?: string) => {
  return useQuery({
    queryKey: queryKeys.barbers.availability(barberId, date, serviceId),
    queryFn: () => barbersApi.getAvailability(barberId, date, serviceId).then((res) => res.data),
    enabled: !!barberId && !!date,
    staleTime: 60_000, // 1 minute
    refetchInterval: 5 * 60_000, // Refetch every 5 minutes
  });
};

// Shop hooks
export const useShops = (params?: { latitude?: number; longitude?: number; radiusKm?: number }) => {
  return useQuery({
    queryKey: queryKeys.shops.nearby(params?.latitude, params?.longitude, params?.radiusKm),
    queryFn: () => shopsApi.getAll(params).then((res) => res.data),
    staleTime: 5 * 60_000, // 5 minutes
  });
};

export const useShop = (id: string) => {
  return useQuery({
    queryKey: queryKeys.shops.byId(id),
    queryFn: () => shopsApi.getById(id).then((res) => res.data),
    enabled: !!id,
  });
};

// Appointment hooks
export const useCreateAppointment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateAppointmentRequest & { idempotencyKey: string }) => {
      const { idempotencyKey, ...appointmentData } = data;
      return appointmentsApi.create(appointmentData, idempotencyKey).then((res) => res.data);
    },
    onSuccess: (_, variables) => {
      // Invalidate availability cache for the booked barber
      queryClient.invalidateQueries({
        queryKey: queryKeys.barbers.availability(variables.barberId, variables.start.split('T')[0]),
      });
    },
  });
};
