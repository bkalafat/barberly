import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { useAvailability, useCreateAppointment, useShop } from '@/lib/api/hooks';
import { addDays, format } from 'date-fns';
import { useState } from 'react';
import { useParams } from 'react-router-dom';

export function ShopDetailsPage() {
  const { shopId } = useParams<{ shopId: string }>();
  const [selectedDate, setSelectedDate] = useState(format(new Date(), 'yyyy-MM-dd'));
  const [selectedBarber] = useState('example-barber-id'); // TODO: Get from shop barbers

  const { data: shop, isLoading: shopLoading } = useShop(shopId!);
  const { data: availability, isLoading: availabilityLoading } = useAvailability(
    selectedBarber,
    selectedDate
  );
  const createAppointment = useCreateAppointment();

  const handleBookAppointment = async (slot: { start: string; end: string }) => {
    try {
      await createAppointment.mutateAsync({
        userId: 'example-user-id', // TODO: Get from auth context
        barberId: selectedBarber,
        serviceId: 'example-service-id', // TODO: Service selection
        start: slot.start,
        end: slot.end,
        idempotencyKey: `${Date.now()}-${Math.random()}`,
      });
      alert('Appointment booked successfully!');
    } catch (error) {
      console.error(error);
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
              {availabilityLoading ? (
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
