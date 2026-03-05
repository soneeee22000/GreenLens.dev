import { test, expect } from '@playwright/test';
import { mockAllApiRoutes } from './fixtures';

test.describe('Estimate Detail', () => {
  test.beforeEach(async ({ page }) => {
    await mockAllApiRoutes(page);
  });

  test('displays CO2e total and resource breakdown', async ({ page }) => {
    await page.goto('/estimate/e2e-test-001');

    // Total CO2e
    await expect(page.getByText('12.35 kg CO2e')).toBeVisible();

    // Breakdown table
    await expect(page.getByText('Resource Breakdown')).toBeVisible();
    await expect(page.getByText('Standard_D4s_v3')).toBeVisible();
    await expect(page.getByText('BlobStorage')).toBeVisible();
  });

  test('loads and displays AI recommendations', async ({ page }) => {
    await page.goto('/estimate/e2e-test-001');

    await expect(page.getByText('AI Recommendations')).toBeVisible();
    await expect(
      page.getByText('Switch to B-series burstable VMs'),
    ).toBeVisible();
    await expect(
      page.getByText('Move workloads to Sweden Central'),
    ).toBeVisible();
    await expect(page.getByText('~40% reduction')).toBeVisible();
    await expect(page.getByText('Low effort')).toBeVisible();
  });

  test('shows back to dashboard link', async ({ page }) => {
    await page.goto('/estimate/e2e-test-001');

    const backLink = page.getByText('Back to Dashboard');
    await expect(backLink).toBeVisible();
    await backLink.click();
    await expect(page).toHaveURL('/');
  });

  test('shows error when estimate not found', async ({ page }) => {
    await page.route('**/api/v1/estimates/nonexistent', (route) => {
      route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({
          data: null,
          error: { code: 'NOT_FOUND', message: 'Not found' },
        }),
      });
    });

    await page.goto('/estimate/nonexistent');
    await expect(page.getByText('Failed to load estimate')).toBeVisible();
  });
});
