import {
  ComponentFixture,
  TestBed,
  fakeAsync,
  tick,
} from '@angular/core/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { EstimateDetailComponent } from './estimate-detail.component';
import { ApiService } from '../../services/api.service';
import {
  ApiResponse,
  EstimateResponse,
  RecommendationResponse,
} from '../../models/api.models';

describe('EstimateDetailComponent', () => {
  let component: EstimateDetailComponent;
  let fixture: ComponentFixture<EstimateDetailComponent>;
  let apiServiceSpy: jasmine.SpyObj<ApiService>;

  const mockEstimate: EstimateResponse = {
    estimateId: 'test-id',
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
  };

  const mockRecommendations: RecommendationResponse[] = [
    {
      title: 'Use B-series VMs',
      description: 'Reduce idle energy.',
      estimatedReductionPercent: 30,
      effort: 'Low',
    },
    {
      title: 'Move to greener region',
      description: 'Sweden Central is greener.',
      estimatedReductionPercent: 25,
      effort: 'Medium',
    },
    {
      title: 'Right-size resources',
      description: 'Downsize over-provisioned VMs.',
      estimatedReductionPercent: 20,
      effort: 'Medium',
    },
  ];

  beforeEach(async () => {
    apiServiceSpy = jasmine.createSpyObj('ApiService', [
      'getEstimate',
      'getRecommendations',
    ]);
    apiServiceSpy.getEstimate.and.returnValue(
      of({ data: mockEstimate, error: null }),
    );
    apiServiceSpy.getRecommendations.and.returnValue(
      of({ data: mockRecommendations, error: null }),
    );

    await TestBed.configureTestingModule({
      imports: [EstimateDetailComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: ApiService, useValue: apiServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'test-id' } } },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EstimateDetailComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load estimate and recommendations on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    expect(component.estimate).toBeTruthy();
    expect(component.estimate?.totalCo2eKg).toBe(50.4);
    expect(component.recommendations.length).toBe(3);
    expect(component.loading).toBeFalse();
    expect(component.recommendationsLoading).toBeFalse();
  }));

  it('should display estimate total CO2e', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('50.40');
    expect(compiled.textContent).toContain('kg CO2e');
  }));

  it('should display resource breakdown table', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Standard_D4s_v3');
    expect(compiled.textContent).toContain('westeurope');
  }));

  it('should display recommendations with effort levels', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Use B-series VMs');
    expect(compiled.textContent).toContain('Low effort');
    expect(compiled.textContent).toContain('30%');
  }));

  it('should show error when estimate not found', fakeAsync(() => {
    apiServiceSpy.getEstimate.and.returnValue(
      throwError(() => new Error('404')),
    );

    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(component.errorMessage).toBe('Failed to load estimate.');
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Failed to load estimate');
  }));

  it('should handle missing route param', fakeAsync(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [EstimateDetailComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: ApiService, useValue: apiServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => null } } },
        },
      ],
    }).compileComponents();

    const newFixture = TestBed.createComponent(EstimateDetailComponent);
    const newComponent = newFixture.componentInstance;
    newFixture.detectChanges();
    tick();

    expect(newComponent.errorMessage).toBe('Invalid estimate ID.');
    expect(newComponent.loading).toBeFalse();
  }));

  it('should gracefully handle recommendation failure', fakeAsync(() => {
    apiServiceSpy.getRecommendations.and.returnValue(
      throwError(() => new Error('503')),
    );

    fixture.detectChanges();
    tick();

    expect(component.estimate).toBeTruthy();
    expect(component.recommendations.length).toBe(0);
    expect(component.recommendationsLoading).toBeFalse();
  }));
});
