import { Button } from '@/components/ui/button';
import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="container mx-auto flex min-h-[70vh] flex-col items-center justify-center gap-6 text-center">
      <div>
        <p className="text-sm uppercase tracking-wide text-muted-foreground">404</p>
        <h1 className="mt-2 text-4xl font-bold">Page not found</h1>
        <p className="mt-2 max-w-md text-muted-foreground">
          The page you are looking for might have been removed, had its name changed, or is
          temporarily unavailable.
        </p>
      </div>

      <div className="flex flex-wrap items-center justify-center gap-3">
        <Link to="/">
          <Button>Go to homepage</Button>
        </Link>
        <Link to="/barbers">
          <Button variant="outline">Browse barbers</Button>
        </Link>
      </div>
    </div>
  );
}

export default NotFoundPage;
