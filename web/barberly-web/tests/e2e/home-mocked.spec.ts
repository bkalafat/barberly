import { expect, test } from '@playwright/test';
import { TestHelpers } from './helpers/test-helpers';

test.describe('Home Page (with mocked data)', () => {
  let helpers: TestHelpers;

  test.beforeEach(async ({ page }) => {
    helpers = new TestHelpers(page);
    await helpers.mockShopsApi();
    await page.goto('/');
  });

  test('should display shops with mocked data', async ({ page }) => {
    // Wait for mocked data to load
    await expect(page.getByText('Test Barber Shop')).toBeVisible();
    await expect(page.getByText('123 Test Street, Test City')).toBeVisible();
    
    // Check if the shop card has correct test ID
    const shopCard = page.locator('[data-testid="shop-card"]');
    await expect(shopCard).toBeVisible();
  });

  test('should navigate to shop details', async ({ page }) => {
    // Click on view details button
    await page.getByRole('button', { name: 'View Details' }).click();
    
    // Should navigate to shop details page
    await expect(page).toHaveURL(/\/shops\/shop-1$/);
  });
});
