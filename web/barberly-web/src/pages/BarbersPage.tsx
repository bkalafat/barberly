import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { useBarbers, useShops } from '@/lib/api/hooks';
import type { BarberShop } from '@/lib/api/client';
import { MapPin, Search, Star } from 'lucide-react';
import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';

export function BarbersPage() {
  const [searchTerm, setSearchTerm] = useState('');

  const { data: barbers, isLoading, error } = useBarbers();
  const { data: shops } = useShops();

  const shopsById = useMemo<Record<string, BarberShop>>(() => {
    if (!shops) {
      return {};
    }

    return shops.reduce<Record<string, BarberShop>>((accumulator, shop) => {
      accumulator[shop.id] = shop;
      return accumulator;
    }, {});
  }, [shops]);

  const filteredBarbers = barbers?.filter((barber) => {
    return (
      barber.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      barber.bio?.toLowerCase().includes(searchTerm.toLowerCase())
    );
  });

  if (isLoading) {
    return (
      <div className="container mx-auto p-4">
        <div className="text-center">Loading barbers...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto p-4">
        <div className="text-center text-red-500">
          Error loading barbers. Please try again later.
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-4">
      <div className="mb-8">
        <h1 className="text-4xl font-bold text-center mb-4">Find Your Barber</h1>
        <p className="text-center text-muted-foreground mb-6">
          Discover skilled barbers in your area
        </p>

        {/* Search Section */}
        <div className="max-w-2xl mx-auto space-y-4">
          <div className="relative">
            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search barbers by name or specialty..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>
      </div>

      {/* Barbers Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredBarbers?.map((barber) => (
          <Card
            key={barber.id}
            className="hover:shadow-lg transition-shadow"
            data-testid="barber-card"
          >
            <CardHeader className="text-center">
              <div className="w-20 h-20 bg-primary/10 rounded-full mx-auto mb-4 flex items-center justify-center">
                <span className="text-2xl font-bold text-primary">{barber.fullName.charAt(0)}</span>
              </div>
              <CardTitle className="text-xl" data-testid="barber-name">
                {barber.fullName}
              </CardTitle>
              <CardDescription>{barber.yearsOfExperience} years of experience</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {barber.bio && (
                  <p
                    className="text-sm text-muted-foreground line-clamp-2"
                    data-testid="barber-bio"
                  >
                    {barber.bio}
                  </p>
                )}

                <div className="flex items-center justify-between text-sm">
                  <div className="flex items-center gap-1">
                    <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                    <span>{barber.averageRating.toFixed(1)}</span>
                    <span className="text-muted-foreground">({barber.totalReviews} reviews)</span>
                  </div>
                </div>

                <div className="flex items-start gap-2 text-sm text-muted-foreground">
                  <MapPin className="mt-0.5 h-4 w-4" />
                  <div>
                    <span className="font-medium text-foreground">
                      {shopsById[barber.barberShopId]?.name ?? 'Unknown shop'}
                    </span>
                    <p className="text-xs text-muted-foreground">
                      {shopsById[barber.barberShopId]
                        ? `${shopsById[barber.barberShopId].address.city}, ${shopsById[barber.barberShopId].address.state}`
                        : 'Location unavailable'}
                    </p>
                  </div>
                </div>

                <div className="pt-2">
                  <Link to={`/barbers/${barber.id}`}>
                    <Button className="w-full">View Profile & Book</Button>
                  </Link>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {filteredBarbers?.length === 0 && (
        <div className="text-center py-12">
          <p className="text-muted-foreground text-lg">No barbers found matching your criteria.</p>
          <p className="text-sm text-muted-foreground mt-2">
            Try adjusting your search or filters.
          </p>
        </div>
      )}
    </div>
  );
}
