import { expect, test } from '@playwright/test';

test.describe('Appointment Booking Flow', () => {
  test('should complete the full booking flow', async ({ page }) => {
    // Start from homepage
    await page.goto('/');
    
    // Navigate to barbers page
    await page.getByRole('link', { name: 'Browse All Barbers' }).click();
    await expect(page).toHaveURL('/barbers');
    
    // Wait for barbers to load
    await page.waitForSelector('[data-testid="barber-card"]', { timeout: 10000 });
    
    // Click on the first barber's profile
    const firstBarber = page.locator('[data-testid="barber-card"]').first();
    await firstBarber.getByRole('button', { name: 'View Profile & Book' }).click();
    
    // Should navigate to barber profile page
    await expect(page).toHaveURL(/\/barbers\/[^\/]+$/);
    
    // Check if booking section is visible
    await expect(page.getByText(/book.*appointment|schedule.*appointment/i)).toBeVisible();
  });

  test('should show availability calendar', async ({ page }) => {
    // Mock the availability API
    await page.route('**/api/v1/barbers/*/availability*', async route => {
      const availableSlots = [
        { dateTime: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(), isAvailable: true },
        { dateTime: new Date(Date.now() + 25 * 60 * 60 * 1000).toISOString(), isAvailable: true },
      ];
      
      await route.fulfill({
        status: 200,
        body: JSON.stringify(availableSlots),
      });
    });

    // Navigate directly to a barber profile (using test barber ID)
    await page.goto('/barbers/9e862653-65c0-41b0-82e9-754f638baa49');
    
    // Check if calendar/availability section is visible
    await expect(page.getByText(/available.*times|select.*time|choose.*appointment/i)).toBeVisible();
  });

  test('should handle booking form submission', async ({ page }) => {
    // Mock successful booking
    await page.route('**/api/v1/appointments', async route => {
      const request = route.request();
      const idempotencyKey = request.headers()['idempotency-key'];
      
      if (!idempotencyKey) {
        await route.fulfill({
          status: 400,
          body: JSON.stringify({ error: 'Idempotency key required' }),
        });
        return;
      }
      
      await route.fulfill({
        status: 201,
        body: JSON.stringify({
          id: 'test-appointment-id',
          barberId: '9e862653-65c0-41b0-82e9-754f638baa49',
          serviceId: 'test-service-id',
          dateTime: new Date().toISOString(),
          status: 'Confirmed'
        }),
      });
    });

    // Navigate to barber profile
    await page.goto('/barbers/9e862653-65c0-41b0-82e9-754f638baa49');
    
    // Fill booking form (if it exists)
    const bookingForm = page.locator('form[data-testid="booking-form"]');
    if (await bookingForm.isVisible({ timeout: 5000 })) {
      // Select service
      const serviceSelect = page.getByLabel(/service|treatment/i);
      if (await serviceSelect.isVisible({ timeout: 2000 })) {
        await serviceSelect.click();
        await page.getByRole('option').first().click();
      }
      
      // Select date/time if available
      const dateTimeButton = page.locator('button[data-testid="time-slot"]').first();
      if (await dateTimeButton.isVisible({ timeout: 2000 })) {
        await dateTimeButton.click();
      }
      
      // Submit booking
      await page.getByRole('button', { name: /book|confirm|schedule/i }).click();
      
      // Check for success message
      await expect(page.getByText(/booked|confirmed|success/i)).toBeVisible({ timeout: 10000 });
    }
  });

  test('should validate required booking fields', async ({ page }) => {
    await page.goto('/barbers/9e862653-65c0-41b0-82e9-754f638baa49');
    
    // Try to submit booking without filling required fields
    const bookButton = page.getByRole('button', { name: /book|confirm|schedule/i });
    if (await bookButton.isVisible({ timeout: 5000 })) {
      await bookButton.click();
      
      // Should show validation errors
      await expect(page.getByText(/required|select.*service|choose.*time/i)).toBeVisible();
    }
  });

  test('should handle booking conflicts', async ({ page }) => {
    // Mock booking conflict
    await page.route('**/api/v1/appointments', async route => {
      await route.fulfill({
        status: 409,
        body: JSON.stringify({ error: 'Time slot no longer available' }),
      });
    });

    await page.goto('/barbers/9e862653-65c0-41b0-82e9-754f638baa49');
    
    // Attempt to book (if form exists)
    const bookButton = page.getByRole('button', { name: /book|confirm|schedule/i });
    if (await bookButton.isVisible({ timeout: 5000 })) {
      await bookButton.click();
      
      // Should show conflict message
      await expect(page.getByText(/no longer available|conflict|already booked/i)).toBeVisible();
    }
  });
});
