import { test, expect } from '@playwright/test';
import { mockAllApiRoutes } from './fixtures';

test.describe('Search Emission Factors', () => {
  test.beforeEach(async ({ page }) => {
    await mockAllApiRoutes(page);
  });

  test('renders search page with input field', async ({ page }) => {
    await page.goto('/search');

    await expect(page.getByText('Search Emission Factors')).toBeVisible();
    await expect(page.locator('input[matInput]')).toBeVisible();
  });

  test('searches and displays results after typing', async ({ page }) => {
    await page.goto('/search');

    await page.locator('input[matInput]').fill('D4s VM');

    // Wait for debounce + results
    await expect(page.getByText('2 results found')).toBeVisible();
    await expect(page.getByText('Standard_D4s_v3').first()).toBeVisible();
    await expect(page.getByText('EPA 2024').first()).toBeVisible();
  });

  test('shows empty state for no results', async ({ page }) => {
    await page.route('**/api/v1/emission-factors/search*', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ data: [], error: null }),
      });
    });

    await page.goto('/search');
    await page.locator('input[matInput]').fill('nonexistent-resource-xyz');

    await expect(page.getByText(/No emission factors found/)).toBeVisible();
  });

  test('does not search when query is too short', async ({ page }) => {
    let searchCalled = false;
    await page.route('**/api/v1/emission-factors/search*', (route) => {
      searchCalled = true;
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ data: [], error: null }),
      });
    });

    await page.goto('/search');
    await page.locator('input[matInput]').fill('a');

    // Wait a bit longer than debounce
    await page.waitForTimeout(500);
    expect(searchCalled).toBe(false);
  });
});
