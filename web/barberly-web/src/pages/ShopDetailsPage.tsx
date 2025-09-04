import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { useAvailability, useBarbers, useCreateAppointment, useShop } from '@/lib/api/hooks';
import { addDays, format } from 'date-fns';
import { useState } from 'react';
import { useParams } from 'react-router-dom';

export function ShopDetailsPage() {
  const { shopId } = useParams<{ shopId: string }>();
  const [selectedDate, setSelectedDate] = useState(format(new Date(), 'yyyy-MM-dd'));
  const [selectedBarber, setSelectedBarber] = useState<string>('');

  const { data: shop, isLoading: shopLoading } = useShop(shopId!);
  const { data: barbers, isLoading: barbersLoading } = useBarbers({ barberShopId: shopId });
  const { data: availability, isLoading: availabilityLoading } = useAvailability(
    selectedBarber,
    selectedDate
  );
  const createAppointment = useCreateAppointment();

  const handleBookAppointment = async (slot: { start: string; end: string }) => {
    if (!selectedBarber) {
      alert('Please select a barber first.');
      return;
    }

    try {
      // For now, use a default service (Classic Haircut - 30min)
      const defaultServiceId = '9fe96b28-8f2b-4625-ab4d-9f336ae5bf5d';
      
      await createAppointment.mutateAsync({
        userId: 'temp-user-id', // TODO: Get from auth context when authentication is implemented
        barberId: selectedBarber,
        serviceId: defaultServiceId,
        start: slot.start,
        end: slot.end,
        idempotencyKey: `${Date.now()}-${Math.random()}`,
      });
      alert('Appointment booked successfully!');
    } catch (error) {
      console.error('Booking error:', error);
      alert('Failed to book appointment. Please try again.');
    }
  };

  if (shopLoading) {
    return <div className="container mx-auto p-4">Loading shop details...</div>;
  }

  if (!shop) {
    return <div className="container mx-auto p-4">Shop not found.</div>;
  }

  return (
    <div className="container mx-auto p-4">
      <Card className="mb-8">
        <CardHeader>
          <CardTitle className="text-3xl">{shop.name}</CardTitle>
          <CardDescription className="text-lg">{shop.description}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <h3 className="font-semibold mb-2">Address</h3>
              <p className="text-muted-foreground">
                {shop.address.street}
                <br />
                {shop.address.city}, {shop.address.state} {shop.address.postalCode}
              </p>
            </div>
            <div>
              <h3 className="font-semibold mb-2">Contact</h3>
              <p className="text-muted-foreground">
                Phone: {shop.phone}
                <br />
                Email: {shop.email}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Book an Appointment</CardTitle>
          <CardDescription>Select a date and available time slot</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div>
              <label htmlFor="barber" className="text-sm font-medium">
                Select Barber
              </label>
              <select
                id="barber"
                value={selectedBarber}
                onChange={(e) => setSelectedBarber(e.target.value)}
                className="mt-1 w-full border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:border-ring focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 rounded-md"
                disabled={barbersLoading}
              >
                <option value="">Choose a barber</option>
                {barbers
                  ?.filter((barber) => barber.isActive)
                  .map((barber) => (
                    <option key={barber.id} value={barber.id}>
                      {barber.fullName} - {barber.yearsOfExperience} years exp.
                    </option>
                  ))}
              </select>
              {barbersLoading && (
                <p className="mt-1 text-xs text-muted-foreground">Loading barbers...</p>
              )}
              {!barbersLoading && (!barbers || barbers.length === 0) && (
                <p className="mt-1 text-xs text-muted-foreground">No barbers available</p>
              )}
            </div>

            <div>
              <label htmlFor="date" className="text-sm font-medium">
                Select Date
              </label>
              <Input
                id="date"
                type="date"
                value={selectedDate}
                min={format(new Date(), 'yyyy-MM-dd')}
                max={format(addDays(new Date(), 30), 'yyyy-MM-dd')}
                onChange={(e) => setSelectedDate(e.target.value)}
                className="mt-1"
              />
            </div>

            <div>
              <h3 className="text-sm font-medium mb-2">Available Time Slots</h3>
              {!selectedBarber ? (
                <p className="text-muted-foreground">Please select a barber first.</p>
              ) : availabilityLoading ? (
                <p className="text-muted-foreground">Loading availability...</p>
              ) : availability && availability.length > 0 ? (
                <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
                  {availability.map((slot, index) => (
                    <Button
                      key={index}
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
                <p className="text-muted-foreground">No availability for this date.</p>
              )}
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
