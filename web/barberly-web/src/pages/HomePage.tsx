import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useShops } from '@/lib/api/hooks';
import { Link } from 'react-router-dom';

export function HomePage() {
  const { data: shops, isLoading, error } = useShops();

  if (isLoading) {
    return (
      <div className="container mx-auto p-4">
        <div className="text-center">Loading shops...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto p-4">
        <div className="text-center text-red-500">Error loading shops. Please try again later.</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-4xl font-bold text-center mb-8">Welcome to Barberly</h1>
      <p className="text-center text-muted-foreground mb-8">
        Find the perfect barber for your next appointment
      </p>

      <div className="text-center mb-8">
        <Link to="/barbers">
          <Button size="lg">Browse All Barbers</Button>
        </Link>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {shops?.map((shop) => (
          <Card key={shop.id} className="hover:shadow-lg transition-shadow" data-testid="shop-card">
            <CardHeader>
              <CardTitle>{shop.name}</CardTitle>
              <CardDescription>{shop.description}</CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground mb-4">
                {shop.address.street}, {shop.address.city}
              </p>
              <Link to={`/shops/${shop.id}`}>
                <Button className="w-full">View Details</Button>
              </Link>
            </CardContent>
          </Card>
        ))}
      </div>

      {shops?.length === 0 && (
        <div className="text-center">
          <p className="text-muted-foreground">No shops available at the moment.</p>
        </div>
      )}
    </div>
  );
}
