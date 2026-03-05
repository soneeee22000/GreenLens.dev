import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { ApiService } from './api.service';
import { EstimateRequest } from '../models/api.models';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should POST to /estimates on createEstimate', () => {
    const request: EstimateRequest = {
      resources: [
        {
          resourceType: 'Standard_D4s_v3',
          quantity: 2,
          hours: 720,
          region: 'westeurope',
        },
      ],
    };

    service.createEstimate(request).subscribe((res) => {
      expect(res.data).toBeTruthy();
    });

    const req = httpMock.expectOne('/api/v1/estimates');
    expect(req.request.method).toBe('POST');
    expect(req.request.headers.get('X-Api-Key')).toBeTruthy();
    req.flush({ data: { estimateId: '123', totalCo2eKg: 50.4 }, error: null });
  });

  it('should GET /estimates with pagination on listEstimates', () => {
    service.listEstimates(2, 10).subscribe();

    const req = httpMock.expectOne('/api/v1/estimates?page=2&pageSize=10');
    expect(req.request.method).toBe('GET');
    req.flush({ data: [], error: null, meta: { page: 2, total: 0 } });
  });

  it('should GET /estimates/:id on getEstimate', () => {
    service.getEstimate('abc-123').subscribe();

    const req = httpMock.expectOne('/api/v1/estimates/abc-123');
    expect(req.request.method).toBe('GET');
    req.flush({ data: { estimateId: 'abc-123' }, error: null });
  });

  it('should GET /estimates/:id/recommendations on getRecommendations', () => {
    service.getRecommendations('abc-123').subscribe();

    const req = httpMock.expectOne('/api/v1/estimates/abc-123/recommendations');
    expect(req.request.method).toBe('GET');
    req.flush({ data: [], error: null });
  });

  it('should GET /emission-factors/search with query params on searchEmissionFactors', () => {
    service.searchEmissionFactors('D4s VM', 5).subscribe();

    const req = httpMock.expectOne(
      '/api/v1/emission-factors/search?q=D4s%20VM&top=5',
    );
    expect(req.request.method).toBe('GET');
    req.flush({ data: [], error: null });
  });

  it('should GET /regions and extract data on getRegions', () => {
    const mockRegions = [
      {
        name: 'westeurope',
        displayName: 'West Europe',
        gridCarbonIntensityGCo2ePerKwh: 300,
      },
    ];

    service.getRegions().subscribe((regions) => {
      expect(regions.length).toBe(1);
      expect(regions[0].name).toBe('westeurope');
    });

    const req = httpMock.expectOne('/api/v1/regions');
    expect(req.request.method).toBe('GET');
    req.flush({ data: mockRegions, error: null });
  });
});
