import { expect, test } from '@playwright/test';

test('Basic Playwright functionality test', async ({ page }) => {
  // Simply navigate to the running frontend
  console.log('Starting test...');
  
  try {
    await page.goto('http://localhost:5173', { waitUntil: 'networkidle' });
    console.log('Navigated to homepage');
    
    // Take a screenshot to verify
    await page.screenshot({ path: 'test-results/homepage.png' });
    console.log('Screenshot taken');
    
    // Check if page title contains something
    const title = await page.title();
    console.log('Page title:', title);
    
    // Simple assertion
    expect(title).toBeTruthy();
    
    console.log('✅ Basic test passed!');
  } catch (error) {
    console.error('Test failed:', error);
    throw error;
  }
});
