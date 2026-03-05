import { test, expect } from '@playwright/test';
import { mockAllApiRoutes } from './fixtures';

test.describe('Create Estimate', () => {
  test.beforeEach(async ({ page }) => {
    await mockAllApiRoutes(page);
  });

  test('renders form with one resource by default', async ({ page }) => {
    await page.goto('/estimate/new');

    await expect(page.getByText('New Carbon Estimate')).toBeVisible();
    await expect(page.getByText('Resource 1')).toBeVisible();
    await expect(
      page.getByRole('button', { name: 'Calculate CO2e' }),
    ).toBeVisible();
  });

  test('can add and remove resource rows', async ({ page }) => {
    await page.goto('/estimate/new');

    // Add a second resource
    await page.getByRole('button', { name: 'Add Resource' }).click();
    await expect(page.getByText('Resource 2')).toBeVisible();

    // Remove the second resource
    const closeButtons = page.locator('button mat-icon:has-text("close")');
    await closeButtons.last().click();
    await expect(page.getByText('Resource 2')).not.toBeVisible();
  });

  test('submit button is disabled when form is invalid', async ({ page }) => {
    await page.goto('/estimate/new');

    const submitBtn = page.getByRole('button', { name: 'Calculate CO2e' });
    await expect(submitBtn).toBeDisabled();
  });

  test('submits form and navigates to estimate detail', async ({ page }) => {
    await page.goto('/estimate/new');

    // Fill out form — select resource type
    await page.locator('mat-select[formControlName="resourceType"]').click();
    await page.getByRole('option', { name: 'Standard_D4s_v3' }).click();

    // Select region
    await page.locator('mat-select[formControlName="region"]').click();
    await page.getByRole('option', { name: 'West Europe' }).click();

    // Quantity and hours already have defaults (1 and 720)
    // Submit
    await page.getByRole('button', { name: 'Calculate CO2e' }).click();

    // Should navigate to estimate detail
    await expect(page).toHaveURL(/\/estimate\/e2e-test-001/);
  });

  test('shows error snackbar when submission fails', async ({ page }) => {
    // Override the POST route to return error
    await page.route('**/api/v1/estimates', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({
            data: null,
            error: {
              code: 'VALIDATION_ERROR',
              message: 'Invalid resource type',
            },
          }),
        });
      } else {
        route.continue();
      }
    });

    await page.goto('/estimate/new');

    // Fill form
    await page.locator('mat-select[formControlName="resourceType"]').click();
    await page.getByRole('option', { name: 'Standard_D4s_v3' }).click();
    await page.locator('mat-select[formControlName="region"]').click();
    await page.getByRole('option', { name: 'West Europe' }).click();

    await page.getByRole('button', { name: 'Calculate CO2e' }).click();

    // Snackbar with error message
    await expect(page.locator('mat-snack-bar-container')).toBeVisible();
  });
});
