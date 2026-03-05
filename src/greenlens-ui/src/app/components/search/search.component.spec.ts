import {
  ComponentFixture,
  TestBed,
  fakeAsync,
  tick,
} from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { SearchComponent } from './search.component';
import { ApiService } from '../../services/api.service';
import { ApiResponse, EmissionFactorResponse } from '../../models/api.models';

describe('SearchComponent', () => {
  let component: SearchComponent;
  let fixture: ComponentFixture<SearchComponent>;
  let apiServiceSpy: jasmine.SpyObj<ApiService>;

  const mockResults: EmissionFactorResponse[] = [
    {
      resourceType: 'Standard_D4s_v3',
      provider: 'Azure',
      region: 'westeurope',
      co2ePerUnit: 0.035,
      unit: 'kgCO2e/hour',
      source: 'EPA',
      lastUpdated: '2026-01-01',
    },
  ];

  const mockResponse: ApiResponse<EmissionFactorResponse[]> = {
    data: mockResults,
    error: null,
  };

  beforeEach(async () => {
    apiServiceSpy = jasmine.createSpyObj('ApiService', [
      'searchEmissionFactors',
    ]);
    apiServiceSpy.searchEmissionFactors.and.returnValue(of(mockResponse));

    await TestBed.configureTestingModule({
      imports: [SearchComponent],
      providers: [
        { provide: ApiService, useValue: apiServiceSpy },
        provideNoopAnimations(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with empty results', () => {
    expect(component.results.length).toBe(0);
    expect(component.loading).toBeFalse();
    expect(component.searched).toBeFalse();
  });

  it('should not search when query is less than 2 characters', fakeAsync(() => {
    component.onQueryChange('a');
    tick(400);

    expect(apiServiceSpy.searchEmissionFactors).not.toHaveBeenCalled();
  }));

  it('should search after debounce when query is 2+ characters', fakeAsync(() => {
    component.onQueryChange('D4s VM');
    tick(400);

    expect(apiServiceSpy.searchEmissionFactors).toHaveBeenCalledWith('D4s VM');
    expect(component.results.length).toBe(1);
    expect(component.searched).toBeTrue();
  }));

  it('should display results in table', fakeAsync(() => {
    component.onQueryChange('D4s');
    tick(400);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Standard_D4s_v3');
    expect(compiled.textContent).toContain('1 results found');
  }));

  it('should show empty state when no results found', fakeAsync(() => {
    apiServiceSpy.searchEmissionFactors.and.returnValue(
      of({ data: [], error: null }),
    );

    component.onQueryChange('nonexistent');
    tick(400);
    fixture.detectChanges();

    expect(component.results.length).toBe(0);
    expect(component.searched).toBeTrue();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No emission factors found');
  }));

  it('should handle search error', fakeAsync(() => {
    apiServiceSpy.searchEmissionFactors.and.returnValue(
      throwError(() => new Error('fail')),
    );

    component.onQueryChange('error query');
    tick(400);
    fixture.detectChanges();

    expect(component.errorMessage).toBe('Search failed. Please try again.');
  }));

  it('should clear results when query becomes too short', fakeAsync(() => {
    component.onQueryChange('D4s VM');
    tick(400);
    expect(component.results.length).toBe(1);

    component.onQueryChange('a');
    tick(400);
    expect(component.results.length).toBe(0);
    expect(component.searched).toBeFalse();
  }));
});
