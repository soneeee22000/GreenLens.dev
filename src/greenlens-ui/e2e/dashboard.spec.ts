import { test, expect } from '@playwright/test';
import { mockAllApiRoutes, MOCK_ESTIMATE, MOCK_ESTIMATE_2 } from './fixtures';

test.describe('Dashboard', () => {
  test('shows empty state when no estimates exist', async ({ page }) => {
    await page.route('**/api/v1/estimates?*', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [],
          error: null,
          meta: { page: 1, total: 0 },
        }),
      });
    });

    await page.goto('/');
    await expect(page.getByText('No estimates yet')).toBeVisible();
    await expect(page.getByText('Create Estimate')).toBeVisible();
  });

  test('displays estimate list with summary metrics', async ({ page }) => {
    await mockAllApiRoutes(page);
    await page.goto('/');

    // Summary cards
    await expect(page.getByText('Total Estimates')).toBeVisible();
    await expect(
      page.locator('.metric-value', { hasText: '2' }).first(),
    ).toBeVisible();
    await expect(page.getByText('Latest CO2e')).toBeVisible();

    // Estimates table
    const table = page.locator('table');
    await expect(table).toBeVisible();
    await expect(table.locator('tr')).toHaveCount(3); // header + 2 rows
  });

  test('displays CO2e over time chart', async ({ page }) => {
    await mockAllApiRoutes(page);
    await page.goto('/');

    await expect(page.getByText('CO2e Over Time')).toBeVisible();
    await expect(page.locator('canvas')).toBeVisible();
  });

  test('navigates to estimate detail when clicking View', async ({ page }) => {
    await mockAllApiRoutes(page);
    await page.goto('/');

    await page.getByText('View').first().click();
    await expect(page).toHaveURL(/\/estimate\/e2e-test-001/);
  });

  test('shows error state when API fails', async ({ page }) => {
    await page.route('**/api/v1/estimates?*', (route) => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: '{}',
      });
    });

    await page.goto('/');
    await expect(page.getByText('Failed to load estimates')).toBeVisible();
    await expect(page.getByText('Retry')).toBeVisible();
  });
});
