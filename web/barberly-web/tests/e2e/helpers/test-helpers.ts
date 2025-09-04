import { Page } from '@playwright/test';

/**
 * Test utilities for Playwright E2E tests
 */

export class TestHelpers {
  constructor(private page: Page) {}

  /**
   * Wait for API calls to complete
   */
  async waitForApiCalls(patterns: string[] = ['**/api/v1/**']) {
    const promises = patterns.map(pattern => 
      this.page.waitForResponse(response => 
        response.url().includes(pattern.replace('**/api/v1/**', '')) && 
        response.status() < 400
      )
    );
    
    try {
      await Promise.race(promises);
    } catch (error) {
      console.log('API calls timed out, continuing with test');
    }
  }

  /**
   * Mock successful shops API response
   */
  async mockShopsApi() {
    await this.page.route('**/api/v1/shops', async route => {
      const mockShops = [
        {
          id: 'shop-1',
          name: 'Test Barber Shop',
          description: 'A great place for haircuts',
          address: {
            street: '123 Test Street',
            city: 'Test City',
            state: 'TS',
            zipCode: '12345',
            country: 'Test Country'
          },
          phoneNumber: '+1-555-TEST',
          email: 'test@barbershop.com'
        }
      ];
      
      await route.fulfill({
        status: 200,
        body: JSON.stringify(mockShops),
      });
    });
  }

  /**
   * Mock successful barbers API response
   */
  async mockBarbersApi() {
    await this.page.route('**/api/v1/barbers', async route => {
      const mockBarbers = [
        {
          id: '9e862653-65c0-41b0-82e9-754f638baa49',
          fullName: 'Ahmet Yılmaz',
          email: 'ahmet@test.com',
          phoneNumber: '+90-555-TEST',
          barberShopId: 'shop-1',
          yearsOfExperience: 8,
          bio: 'Experienced barber specializing in modern cuts',
          averageRating: 4.8,
          totalReviews: 127
        },
        {
          id: 'barber-2',
          fullName: 'Test Barber 2',
          email: 'barber2@test.com',
          phoneNumber: '+1-555-TEST2',
          barberShopId: 'shop-1',
          yearsOfExperience: 5,
          bio: 'Creative stylist with modern techniques',
          averageRating: 4.5,
          totalReviews: 85
        }
      ];
      
      await route.fulfill({
        status: 200,
        body: JSON.stringify(mockBarbers),
      });
    });
  }

  /**
   * Mock services API response
   */
  async mockServicesApi() {
    await this.page.route('**/api/v1/services', async route => {
      const mockServices = [
        {
          id: 'service-1',
          name: 'Haircut',
          description: 'Professional haircut service',
          durationMinutes: 30,
          price: 25.00
        },
        {
          id: 'service-2',
          name: 'Beard Trim',
          description: 'Professional beard trimming',
          durationMinutes: 15,
          price: 15.00
        }
      ];
      
      await route.fulfill({
        status: 200,
        body: JSON.stringify(mockServices),
      });
    });
  }

  /**
   * Mock availability API response
   */
  async mockAvailabilityApi(barberId: string = '9e862653-65c0-41b0-82e9-754f638baa49') {
    await this.page.route(`**/api/v1/barbers/${barberId}/availability*`, async route => {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      tomorrow.setHours(10, 0, 0, 0);

      const availableSlots: Array<{ dateTime: string; isAvailable: boolean }> = [];
      for (let i = 0; i < 8; i++) {
        const slot = new Date(tomorrow);
        slot.setHours(10 + i);
        availableSlots.push({
          dateTime: slot.toISOString(),
          isAvailable: true
        });
      }
      
      await route.fulfill({
        status: 200,
        body: JSON.stringify(availableSlots),
      });
    });
  }

  /**
   * Fill out a booking form with test data
   */
  async fillBookingForm(options: {
    service?: string;
    selectFirstSlot?: boolean;
  } = {}) {
    const { service = 'Haircut', selectFirstSlot = true } = options;

    // Select service if dropdown exists
    const serviceSelect = this.page.getByLabel(/service|treatment/i);
    if (await serviceSelect.isVisible({ timeout: 2000 })) {
      await serviceSelect.click();
      await this.page.getByRole('option', { name: service }).click();
    }

    // Select first available time slot if requested
    if (selectFirstSlot) {
      const timeSlot = this.page.locator('button[data-testid="time-slot"]').first();
      if (await timeSlot.isVisible({ timeout: 2000 })) {
        await timeSlot.click();
      }
    }
  }

  /**
   * Generate a unique idempotency key for API calls
   */
  generateIdempotencyKey(): string {
    return `test-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Mock successful appointment booking
   */
  async mockSuccessfulBooking() {
    await this.page.route('**/api/v1/appointments', async route => {
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
          id: 'appointment-' + Date.now(),
          barberId: '9e862653-65c0-41b0-82e9-754f638baa49',
          serviceId: 'service-1',
          dateTime: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
          status: 'Confirmed',
          totalPrice: 25.00
        }),
      });
    });
  }

  /**
   * Mock booking conflict error
   */
  async mockBookingConflict() {
    await this.page.route('**/api/v1/appointments', async route => {
      await route.fulfill({
        status: 409,
        body: JSON.stringify({ 
          error: 'Time slot no longer available',
          details: 'Another customer has booked this time slot'
        }),
      });
    });
  }

  /**
   * Navigate to a specific barber's profile page
   */
  async navigateToBarberProfile(barberId: string = '9e862653-65c0-41b0-82e9-754f638baa49') {
    await this.page.goto(`/barbers/${barberId}`);
  }

  /**
   * Wait for page to be fully loaded
   */
  async waitForPageLoad() {
    await this.page.waitForLoadState('networkidle');
  }
}

/**
 * Test data constants
 */
export const TEST_DATA = {
  BARBER_ID: '9e862653-65c0-41b0-82e9-754f638baa49',
  SHOP_ID: 'shop-1',
  SERVICE_ID: 'service-1',
  USER_EMAIL: 'test@example.com',
  USER_PHONE: '+1-555-0123',
} as const;
