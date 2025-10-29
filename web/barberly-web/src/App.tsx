import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { BrowserRouter as Router, Link, Route, Routes } from 'react-router-dom';
import './index.css';
import { AuthPage } from './pages/AuthPage';
import { BarberDetailsPage } from './pages/BarberDetailsPage';
import { BarbersPage } from './pages/BarbersPage';
import { HomePage } from './pages/HomePage';
import { NotFoundPage } from './pages/NotFoundPage';
import { ShopDetailsPage } from './pages/ShopDetailsPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60_000, // 1 minute
      retry: 2,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <div className="min-h-screen bg-background text-foreground">
          <nav className="border-b">
            <div className="container mx-auto px-4 py-3 flex justify-between items-center">
              <h1 className="text-2xl font-bold">
                <Link to="/" className="hover:text-primary">
                  Barberly
                </Link>
              </h1>
              <div className="flex items-center gap-3">
                <Link
                  to="/barbers"
                  className="hidden text-sm font-medium text-muted-foreground transition-colors hover:text-primary md:block"
                >
                  Explore barbers
                </Link>
                <Link
                  to="/auth"
                  className="inline-flex items-center justify-center whitespace-nowrap rounded-md border border-input bg-background px-4 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50"
                >
                  Sign In
                </Link>
              </div>
            </div>
          </nav>

          <main>
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/shops/:shopId" element={<ShopDetailsPage />} />
              <Route path="/barbers" element={<BarbersPage />} />
              <Route path="/barbers/:barberId" element={<BarberDetailsPage />} />
              <Route path="/auth" element={<AuthPage />} />
              <Route path="*" element={<NotFoundPage />} />
            </Routes>
          </main>
        </div>
      </Router>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

export default App;
