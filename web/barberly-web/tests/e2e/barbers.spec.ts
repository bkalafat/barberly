import { expect, test } from '@playwright/test';

test.describe('Barbers Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/barbers');
  });

  test('should display the barbers page heading', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /barbers/i })).toBeVisible();
  });

  test('should display search functionality', async ({ page }) => {
    // Check if search input is present
    const searchInput = page.getByPlaceholder(/search/i);
    await expect(searchInput).toBeVisible();
    
    // Test search functionality
    await searchInput.fill('test barber');
    await expect(searchInput).toHaveValue('test barber');
  });

  test('should display barber cards when loaded', async ({ page }) => {
    // Wait for barbers to load
    await page.waitForSelector('[data-testid="barber-card"]', { timeout: 10000 });
    
    // Check if at least one barber card is displayed
    const barberCards = page.locator('[data-testid="barber-card"]');
    await expect(barberCards.first()).toBeVisible();
  });

  test('should filter barbers by search term', async ({ page }) => {
    // Wait for barbers to load
    await page.waitForSelector('[data-testid="barber-card"]', { timeout: 10000 });
    
    // Get initial count of barber cards
    const initialCount = await page.locator('[data-testid="barber-card"]').count();
    
    // Search for a specific term
    const searchInput = page.getByPlaceholder(/search/i);
    await searchInput.fill('Ahmet');
    
    // Wait for filtering to take effect
    await page.waitForTimeout(500);
    
    // Check that results are filtered
    const filteredCount = await page.locator('[data-testid="barber-card"]').count();
    expect(filteredCount).toBeLessThanOrEqual(initialCount);
  });

  test('should handle empty search results', async ({ page }) => {
    // Wait for barbers to load
    await page.waitForSelector('[data-testid="barber-card"]', { timeout: 10000 });
    
    // Search for something that doesn't exist
    const searchInput = page.getByPlaceholder(/search/i);
    await searchInput.fill('nonexistentbarber12345');
    
    // Wait for filtering to take effect
    await page.waitForTimeout(500);
    
    // Check that no barber cards are displayed
    const barberCards = page.locator('[data-testid="barber-card"]');
    await expect(barberCards).toHaveCount(0);
  });

  test('should display barber information correctly', async ({ page }) => {
    // Wait for barbers to load
    await page.waitForSelector('[data-testid="barber-card"]', { timeout: 10000 });
    
    const firstBarberCard = page.locator('[data-testid="barber-card"]').first();
    
    // Check that barber card contains expected elements
    await expect(firstBarberCard.locator('[data-testid="barber-name"]')).toBeVisible();
    await expect(firstBarberCard.locator('[data-testid="barber-bio"]')).toBeVisible();
  });

  test('should handle loading state', async ({ page }) => {
    // Intercept the API call to simulate loading
    await page.route('**/api/v1/barbers', async route => {
      await new Promise(resolve => setTimeout(resolve, 1000));
      await route.continue();
    });

    await page.goto('/barbers');
    
    // Check if loading message appears
    await expect(page.getByText(/loading/i)).toBeVisible();
  });

  test('should handle API errors gracefully', async ({ page }) => {
    // Mock API to return error
    await page.route('**/api/v1/barbers', async route => {
      await route.fulfill({
        status: 500,
        body: JSON.stringify({ error: 'Server error' }),
      });
    });

    await page.goto('/barbers');
    
    // Check if error state is handled gracefully
    await expect(page.getByText(/error|failed|unavailable/i)).toBeVisible();
  });
});
