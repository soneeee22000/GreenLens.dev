import { Page } from '@playwright/test';

/** Mock data matching the API response shapes. */

export const MOCK_ESTIMATE = {
  estimateId: 'e2e-test-001',
  totalCo2eKg: 12.3456,
  createdAt: '2026-03-01T10:00:00Z',
  breakdown: [
    {
      resourceType: 'Standard_D4s_v3',
      quantity: 2,
      hours: 720,
      region: 'westeurope',
      co2eKg: 8.1234,
      co2ePerUnit: 0.00564,
      unit: 'kgCO2e/hour',
    },
    {
      resourceType: 'BlobStorage',
      quantity: 1,
      hours: 720,
      region: 'westeurope',
      co2eKg: 4.2222,
      co2ePerUnit: 0.00586,
      unit: 'kgCO2e/GB-month',
    },
  ],
};

export const MOCK_ESTIMATE_2 = {
  estimateId: 'e2e-test-002',
  totalCo2eKg: 5.678,
  createdAt: '2026-02-15T14:30:00Z',
  breakdown: [
    {
      resourceType: 'Standard_B2s',
      quantity: 1,
      hours: 360,
      region: 'eastus',
      co2eKg: 5.678,
      co2ePerUnit: 0.01577,
      unit: 'kgCO2e/hour',
    },
  ],
};

export const MOCK_RECOMMENDATIONS = [
  {
    title: 'Switch to B-series burstable VMs',
    description:
      'B-series VMs use less energy during idle periods, reducing carbon emissions by up to 40%.',
    estimatedReductionPercent: 40,
    effort: 'Low',
  },
  {
    title: 'Move workloads to Sweden Central',
    description:
      'Sweden Central runs on 95%+ renewable energy, significantly lowering carbon intensity.',
    estimatedReductionPercent: 25,
    effort: 'Medium',
  },
];

export const MOCK_REGIONS = [
  {
    name: 'westeurope',
    displayName: 'West Europe',
    gridCarbonIntensityGCo2ePerKwh: 300,
  },
  {
    name: 'eastus',
    displayName: 'East US',
    gridCarbonIntensityGCo2ePerKwh: 400,
  },
  {
    name: 'swedencentral',
    displayName: 'Sweden Central',
    gridCarbonIntensityGCo2ePerKwh: 20,
  },
];

export const MOCK_EMISSION_FACTORS = [
  {
    resourceType: 'Standard_D4s_v3',
    provider: 'Azure',
    region: 'westeurope',
    co2ePerUnit: 0.00564,
    unit: 'kgCO2e/hour',
    source: 'EPA 2024',
    lastUpdated: '2026-01-01T00:00:00Z',
  },
  {
    resourceType: 'Standard_D4s_v3',
    provider: 'Azure',
    region: 'eastus',
    co2ePerUnit: 0.00712,
    unit: 'kgCO2e/hour',
    source: 'EPA 2024',
    lastUpdated: '2026-01-01T00:00:00Z',
  },
];

/**
 * Intercepts all GreenLens API routes with mock responses.
 * Call this at the start of each test to avoid hitting a real backend.
 */
export async function mockAllApiRoutes(page: Page): Promise<void> {
  // List estimates
  await page.route('**/api/v1/estimates?*', (route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        data: [MOCK_ESTIMATE, MOCK_ESTIMATE_2],
        error: null,
        meta: { page: 1, total: 2 },
      }),
    });
  });

  // Get single estimate
  await page.route('**/api/v1/estimates/e2e-test-*', (route) => {
    if (route.request().method() === 'GET') {
      const url = route.request().url();
      const id = url.split('/estimates/')[1]?.split('?')[0];
      const estimate = id === 'e2e-test-002' ? MOCK_ESTIMATE_2 : MOCK_ESTIMATE;
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ data: estimate, error: null }),
      });
    } else {
      route.continue();
    }
  });

  // Create estimate
  await page.route('**/api/v1/estimates', (route) => {
    if (route.request().method() === 'POST') {
      route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({ data: MOCK_ESTIMATE, error: null }),
      });
    } else {
      route.continue();
    }
  });

  // Recommendations
  await page.route('**/api/v1/estimates/*/recommendations', (route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ data: MOCK_RECOMMENDATIONS, error: null }),
    });
  });

  // Regions
  await page.route('**/api/v1/regions', (route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ data: MOCK_REGIONS, error: null }),
    });
  });

  // Search emission factors
  await page.route('**/api/v1/emission-factors/search*', (route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ data: MOCK_EMISSION_FACTORS, error: null }),
    });
  });
}
