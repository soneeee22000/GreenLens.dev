import {
  ComponentFixture,
  TestBed,
  fakeAsync,
  tick,
} from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { EstimateFormComponent } from './estimate-form.component';
import { ApiService } from '../../services/api.service';

describe('EstimateFormComponent', () => {
  let component: EstimateFormComponent;
  let fixture: ComponentFixture<EstimateFormComponent>;
  let apiServiceSpy: jasmine.SpyObj<ApiService>;
  let router: Router;

  beforeEach(async () => {
    apiServiceSpy = jasmine.createSpyObj('ApiService', [
      'getRegions',
      'createEstimate',
    ]);
    apiServiceSpy.getRegions.and.returnValue(
      of([
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
      ]),
    );

    await TestBed.configureTestingModule({
      imports: [EstimateFormComponent],
      providers: [
        { provide: ApiService, useValue: apiServiceSpy },
        provideRouter([
          { path: 'estimate/:id', component: EstimateFormComponent },
        ]),
        provideNoopAnimations(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EstimateFormComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with one resource group', () => {
    expect(component.resources.length).toBe(1);
  });

  it('should load regions on init', fakeAsync(() => {
    tick();
    expect(component.regions.length).toBe(2);
    expect(component.regions[0].name).toBe('westeurope');
  }));

  it('should add a resource group', () => {
    component.addResource();
    expect(component.resources.length).toBe(2);
  });

  it('should remove a resource group', () => {
    component.addResource();
    expect(component.resources.length).toBe(2);
    component.removeResource(1);
    expect(component.resources.length).toBe(1);
  });

  it('should have invalid form when resource type is empty', () => {
    expect(component.form.invalid).toBeTrue();
  });

  it('should have valid form when all fields are filled', () => {
    const resource = component.resources.at(0);
    resource.patchValue({
      resourceType: 'Standard_D4s_v3',
      region: 'westeurope',
      quantity: 2,
      hours: 720,
    });
    expect(component.form.valid).toBeTrue();
  });

  it('should not submit when form is invalid', () => {
    component.onSubmit();
    expect(apiServiceSpy.createEstimate).not.toHaveBeenCalled();
  });

  it('should navigate to detail on successful submit', fakeAsync(() => {
    const navigateSpy = spyOn(router, 'navigate');
    apiServiceSpy.createEstimate.and.returnValue(
      of({
        data: {
          estimateId: 'new-id',
          totalCo2eKg: 50.4,
          createdAt: '2026-03-05',
          breakdown: [],
        },
        error: null,
      }),
    );

    component.resources.at(0).patchValue({
      resourceType: 'Standard_D4s_v3',
      region: 'westeurope',
      quantity: 2,
      hours: 720,
    });

    component.onSubmit();
    tick();

    expect(apiServiceSpy.createEstimate).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/estimate', 'new-id']);
    expect(component.submitting).toBeFalse();
  }));

  it('should show error on failed submit', fakeAsync(() => {
    apiServiceSpy.createEstimate.and.returnValue(
      throwError(() => ({ error: { error: { message: 'Bad request' } } })),
    );

    component.resources.at(0).patchValue({
      resourceType: 'Standard_D4s_v3',
      region: 'westeurope',
      quantity: 2,
      hours: 720,
    });

    component.onSubmit();
    tick(5000);

    expect(component.submitting).toBeFalse();
  }));

  it('should validate quantity minimum is 1', () => {
    const resource = component.resources.at(0);
    resource.patchValue({ quantity: 0 });
    expect(resource.get('quantity')?.hasError('min')).toBeTrue();
  });

  it('should validate hours minimum is 0', () => {
    const resource = component.resources.at(0);
    resource.patchValue({ hours: -1 });
    expect(resource.get('hours')?.hasError('min')).toBeTrue();
  });
});
