import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { useAvailability, useBarber, useCreateAppointment, useShop } from '@/lib/api/hooks';
import { addDays, format } from 'date-fns';
import { Mail, MapPin, Phone, Star } from 'lucide-react';
import { useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import { generateIdempotencyKey } from '@/lib/utils';

export function BarberDetailsPage() {
  const { barberId } = useParams<{ barberId: string }>();
  const [selectedDate, setSelectedDate] = useState(() => format(new Date(), 'yyyy-MM-dd'));

  const {
    data: barber,
    isLoading: barberLoading,
    error: barberError,
  } = useBarber(barberId ?? '');
  const { data: shop } = useShop(barber?.barberShopId ?? '');
  const { data: availability, isLoading: availabilityLoading } = useAvailability(
    barberId ?? '',
    selectedDate
  );
  const createAppointment = useCreateAppointment();

  const experienceLabel = useMemo(() => {
    if (!barber) {
      return '';
    }

    return barber.yearsOfExperience === 1
      ? '1 year of experience'
      : `${barber.yearsOfExperience} years of experience`;
  }, [barber]);

  const handleBookAppointment = async (slot: { start: string; end: string }) => {
    if (!barberId) {
      alert('Missing barber identifier. Please try again.');
      return;
    }

    try {
      const defaultServiceId = '9fe96b28-8f2b-4625-ab4d-9f336ae5bf5d';
      await createAppointment.mutateAsync({
        userId: 'temp-user-id', // TODO: wire up to authenticated user context
        barberId,
        serviceId: defaultServiceId,
        start: slot.start,
        end: slot.end,
        idempotencyKey: generateIdempotencyKey(),
      });

      alert('Appointment booked successfully!');
    } catch (error) {
      console.error('Booking error:', error);
      alert('Failed to book appointment. Please try again.');
    }
  };

  if (barberLoading) {
    return <div className="container mx-auto p-4">Loading barber details...</div>;
  }

  if (barberError) {
    return (
      <div className="container mx-auto p-4">
        <p className="text-red-500">Error loading barber. Please refresh and try again.</p>
      </div>
    );
  }

  if (!barber) {
    return (
      <div className="container mx-auto p-4">
        <p className="text-muted-foreground">Barber not found.</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-4 space-y-8">
      <Card>
        <CardHeader className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div className="flex items-center gap-4">
            <div className="w-20 h-20 rounded-full bg-primary/10 flex items-center justify-center text-3xl font-bold text-primary">
              {barber.profileImageUrl ? (
                <img
                  src={barber.profileImageUrl}
                  alt={barber.fullName}
                  className="h-full w-full rounded-full object-cover"
                />
              ) : (
                barber.fullName.charAt(0)
              )}
            </div>
            <div>
              <CardTitle className="text-3xl">{barber.fullName}</CardTitle>
              <CardDescription>{experienceLabel}</CardDescription>
            </div>
          </div>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Star className="h-5 w-5 text-yellow-400 fill-yellow-400" />
            <span className="font-semibold text-foreground">{barber.averageRating.toFixed(1)}</span>
            <span>({barber.totalReviews} reviews)</span>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {barber.bio && <p className="text-muted-foreground">{barber.bio}</p>}

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div className="space-y-2 text-sm text-muted-foreground">
              <h3 className="font-semibold text-foreground">Contact</h3>
              {barber.phone && (
                <div className="flex items-center gap-2">
                  <Phone className="h-4 w-4" />
                  <span>{barber.phone}</span>
                </div>
              )}
              {barber.email && (
                <div className="flex items-center gap-2">
                  <Mail className="h-4 w-4" />
                  <span>{barber.email}</span>
                </div>
              )}
            </div>

            {shop && (
              <div className="space-y-2 text-sm text-muted-foreground">
                <h3 className="font-semibold text-foreground">Works at</h3>
                <div className="flex items-start gap-2">
                  <MapPin className="h-4 w-4 mt-1" />
                  <div>
                    <p className="font-medium text-foreground">{shop.name}</p>
                    <p>
                      {shop.address.street}
                      <br />
                      {shop.address.city}, {shop.address.state} {shop.address.postalCode}
                    </p>
                  </div>
                </div>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Book an Appointment</CardTitle>
          <CardDescription>Select a date to see available time slots</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <Input
              id="date"
              type="date"
              value={selectedDate}
              min={format(new Date(), 'yyyy-MM-dd')}
              max={format(addDays(new Date(), 30), 'yyyy-MM-dd')}
              onChange={(event) => setSelectedDate(event.target.value)}
              className="max-w-xs"
            />

            <div>
              <h3 className="text-sm font-medium mb-2">Available Time Slots</h3>
              {availabilityLoading ? (
                <p className="text-muted-foreground">Loading availability...</p>
              ) : availability && availability.length > 0 ? (
                <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-4">
                  {availability.map((slot, index) => (
                    <Button
                      key={`${slot.start}-${index}`}
                      variant="outline"
                      onClick={() => handleBookAppointment(slot)}
                      disabled={createAppointment.isPending}
                      className="text-sm"
                    >
                      {format(new Date(slot.start), 'HH:mm')}
                    </Button>
                  ))}
                </div>
              ) : (
                <p className="text-muted-foreground">
                  No availability for the selected date. Please try another day.
                </p>
              )}
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default BarberDetailsPage;
