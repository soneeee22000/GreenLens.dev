import { test, expect } from '@playwright/test';
import { mockAllApiRoutes } from './fixtures';

test.describe('Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await mockAllApiRoutes(page);
  });

  test('layout renders sidebar with navigation links', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByText('GreenLens')).toBeVisible();
    await expect(page.getByRole('link', { name: 'Dashboard' })).toBeVisible();
    await expect(
      page.getByRole('link', { name: 'New Estimate' }),
    ).toBeVisible();
    await expect(
      page.getByRole('link', { name: 'Search Factors' }),
    ).toBeVisible();
  });

  test('navigates between pages using sidebar', async ({ page }) => {
    await page.goto('/');

    // Go to New Estimate
    await page.getByText('New Estimate').click();
    await expect(page).toHaveURL('/estimate/new');
    await expect(page.getByText('New Carbon Estimate')).toBeVisible();

    // Go to Search
    await page.getByText('Search Factors').click();
    await expect(page).toHaveURL('/search');
    await expect(page.getByText('Search Emission Factors')).toBeVisible();

    // Back to Dashboard
    await page.getByRole('link', { name: 'Dashboard' }).click();
    await expect(page).toHaveURL('/');
  });
});
