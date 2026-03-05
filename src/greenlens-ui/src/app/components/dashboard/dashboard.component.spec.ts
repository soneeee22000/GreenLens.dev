import {
  ComponentFixture,
  TestBed,
  fakeAsync,
  tick,
} from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { ApiService } from '../../services/api.service';
import { ApiResponse, EstimateResponse } from '../../models/api.models';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let apiServiceSpy: jasmine.SpyObj<ApiService>;

  const mockEstimates: EstimateResponse[] = [
    {
      estimateId: 'id-1',
      totalCo2eKg: 50.4,
      createdAt: '2026-03-05T10:00:00Z',
      breakdown: [
        {
          resourceType: 'Standard_D4s_v3',
          quantity: 2,
          hours: 720,
          region: 'westeurope',
          co2eKg: 50.4,
          co2ePerUnit: 0.035,
          unit: 'kgCO2e/hour',
        },
      ],
    },
    {
      estimateId: 'id-2',
      totalCo2eKg: 3.7,
      createdAt: '2026-03-04T10:00:00Z',
      breakdown: [
        {
          resourceType: 'BlobStorage',
          quantity: 200,
          hours: 0,
          region: 'eastus',
          co2eKg: 0.2,
          co2ePerUnit: 0.001,
          unit: 'kgCO2e/GB/month',
        },
        {
          resourceType: 'Standard_D4s_v3',
          quantity: 1,
          hours: 100,
          region: 'westeurope',
          co2eKg: 3.5,
          co2ePerUnit: 0.035,
          unit: 'kgCO2e/hour',
        },
      ],
    },
  ];

  const mockResponse: ApiResponse<EstimateResponse[]> = {
    data: mockEstimates,
    error: null,
    meta: { page: 1, total: 2 },
  };

  beforeEach(async () => {
    apiServiceSpy = jasmine.createSpyObj('ApiService', ['listEstimates']);
    apiServiceSpy.listEstimates.and.returnValue(of(mockResponse));

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        { provide: ApiService, useValue: apiServiceSpy },
        provideRouter([]),
        provideNoopAnimations(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show loading spinner initially', () => {
    expect(component.loading).toBeTrue();
  });

  it('should load estimates on init and calculate totals', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    expect(component.loading).toBeFalse();
    expect(component.estimates.length).toBe(2);
    expect(component.totalEstimates).toBe(2);
    expect(component.totalCo2e).toBeCloseTo(54.1, 1);
  }));

  it('should display empty state when no estimates exist', fakeAsync(() => {
    apiServiceSpy.listEstimates.and.returnValue(
      of({ data: [], error: null, meta: { total: 0 } }),
    );

    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No estimates yet');
  }));

  it('should display error state on API failure', fakeAsync(() => {
    apiServiceSpy.listEstimates.and.returnValue(
      throwError(() => new Error('Network error')),
    );

    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(component.errorMessage).toBeTruthy();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Failed to load estimates');
  }));

  it('should build chart data from estimates', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    expect(component.chartData.datasets.length).toBe(1);
    expect(component.chartData.labels?.length).toBe(2);
  }));

  it('should retry loading on retry button click', fakeAsync(() => {
    apiServiceSpy.listEstimates.and.returnValue(
      throwError(() => new Error('fail')),
    );
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    apiServiceSpy.listEstimates.and.returnValue(of(mockResponse));
    component.loadEstimates();
    tick();

    expect(component.estimates.length).toBe(2);
    expect(component.errorMessage).toBe('');
  }));
});
