import { expect, test } from '@playwright/test';

test.describe('Home Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should display the welcome message', async ({ page }) => {
    // Check if the main heading is visible
    await expect(page.getByRole('heading', { name: 'Welcome to Barberly' })).toBeVisible();

    // Check if the description is present
    await expect(page.getByText('Find the perfect barber for your next appointment')).toBeVisible();
  });

  test('should have a "Browse All Barbers" button', async ({ page }) => {
    const browseBarbersButton = page.getByRole('link', { name: 'Browse All Barbers' });
    await expect(browseBarbersButton).toBeVisible();
    await expect(browseBarbersButton).toHaveAttribute('href', '/barbers');
  });

  test('should display barber shops when loaded', async ({ page }) => {
    // Wait for shops to load (may take time for API call)
    await page.waitForSelector('[data-testid="shop-card"]', { timeout: 10000 });

    // Check if at least one shop card is displayed
    const shopCards = page.locator('[data-testid="shop-card"]');
    await expect(shopCards.first()).toBeVisible();
  });

  test('should handle loading state', async ({ page }) => {
    // Intercept the API call to simulate loading
    await page.route('**/api/v1/shops', async (route) => {
      // Delay the response to test loading state
      await new Promise((resolve) => setTimeout(resolve, 1000));
      await route.continue();
    });

    await page.goto('/');

    // Check if loading message appears
    await expect(page.getByText('Loading shops...')).toBeVisible();
  });

  test('should handle error state', async ({ page }) => {
    // Mock API to return error
    await page.route('**/api/v1/shops', async (route) => {
      await route.fulfill({
        status: 500,
        body: JSON.stringify({ error: 'Server error' }),
      });
    });

    await page.goto('/');

    // Check if error message appears
    await expect(page.getByText('Error loading shops. Please try again later.')).toBeVisible();
  });

  test('should navigate to shop details when clicking "View Details"', async ({ page }) => {
    // Wait for shops to load
    await page.waitForSelector('[data-testid="shop-card"]', { timeout: 10000 });

    // Click on the first "View Details" button
    const firstViewDetailsButton = page.getByRole('button', { name: 'View Details' }).first();
    await firstViewDetailsButton.click();

    // Check if we navigated to the shop details page
    await expect(page).toHaveURL(/\/shops\/[^/]+$/);
  });
});
