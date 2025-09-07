import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import './index.css';
import { AuthPage } from './pages/AuthPage';
import { BarbersPage } from './pages/BarbersPage';
import { HomePage } from './pages/HomePage';
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
                <a href="/" className="hover:text-primary">
                  Barberly
                </a>
              </h1>
              <a 
                href="/auth" 
                className="inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 border border-input bg-background hover:bg-accent hover:text-accent-foreground h-10 px-4 py-2"
              >
                Sign In
              </a>
            </div>
          </nav>

          <main>
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/shops/:shopId" element={<ShopDetailsPage />} />
              <Route path="/barbers" element={<BarbersPage />} />
              <Route path="/barbers/:barberId" element={<ShopDetailsPage />} />
              <Route path="/auth" element={<AuthPage />} />
            </Routes>
          </main>
        </div>
      </Router>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

export default App;
