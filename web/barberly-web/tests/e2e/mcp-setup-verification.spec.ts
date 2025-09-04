import { expect, test } from '@playwright/test';

test.describe('Playwright MCP Setup Verification', () => {
  test('should load the homepage and basic navigation', async ({ page }) => {
    // Navigate to the homepage
    await page.goto('/');

    // Wait for the page to be fully loaded
    await page.waitForLoadState('networkidle');

    // Verify the page loaded correctly
    await expect(page).toHaveTitle(/Barberly|Vite \+ React/);

    // Check if the main content is visible (with timeout)
    await expect(page.getByText('Welcome to Barberly')).toBeVisible({ timeout: 10000 });

    // Test basic navigation
    const browseBarbersLink = page.getByRole('link', { name: 'Browse All Barbers' });
    await expect(browseBarbersLink).toBeVisible();

    // Click and verify navigation
    await browseBarbersLink.click();
    await expect(page).toHaveURL('/barbers');

    console.log('✅ Playwright MCP is working correctly with your Barberly app!');
  });

  test('should handle API interactions', async ({ page }) => {
    // Test API mocking capability
    await page.route('**/api/v1/shops', async (route) => {
      await route.fulfill({
        status: 200,
        body: JSON.stringify([
          {
            id: 'test-shop',
            name: 'MCP Test Shop',
            description: 'A shop created by MCP test',
            address: {
              street: '123 MCP Street',
              city: 'Test City',
              state: 'TC',
              zipCode: '12345',
              country: 'Testland',
            },
          },
        ]),
      });
    });

    await page.goto('/');

    // Verify mocked data appears
    await expect(page.getByText('MCP Test Shop')).toBeVisible();

    console.log('✅ API mocking is working correctly!');
  });
});
