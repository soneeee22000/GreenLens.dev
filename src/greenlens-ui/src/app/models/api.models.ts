export interface ApiResponse<T> {
  data: T | null;
  error: ApiError | null;
  meta?: ApiMeta;
}

export interface ApiError {
  code: string;
  message: string;
  details?: string[];
}

export interface ApiMeta {
  page?: number;
  total?: number;
}

export interface ResourceUsageRequest {
  resourceType: string;
  quantity: number;
  hours: number;
  region: string;
}

export interface EstimateRequest {
  resources: ResourceUsageRequest[];
}

export interface EstimateResponse {
  estimateId: string;
  totalCo2eKg: number;
  createdAt: string;
  breakdown: ResourceEstimateResponse[];
}

export interface ResourceEstimateResponse {
  resourceType: string;
  quantity: number;
  hours: number;
  region: string;
  co2eKg: number;
  co2ePerUnit: number;
  unit: string;
}

export interface RecommendationResponse {
  title: string;
  description: string;
  estimatedReductionPercent: number;
  effort: 'Low' | 'Medium' | 'High';
}

export interface EmissionFactorResponse {
  resourceType: string;
  provider: string;
  region: string;
  co2ePerUnit: number;
  unit: string;
  source: string;
  lastUpdated: string;
}

export interface RegionResponse {
  name: string;
  displayName: string;
  gridCarbonIntensityGCo2ePerKwh: number;
}
