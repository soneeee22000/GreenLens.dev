import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  ApiResponse,
  EstimateRequest,
  EstimateResponse,
  RecommendationResponse,
  EmissionFactorResponse,
  RegionResponse,
} from '../models/api.models';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly baseUrl = environment.apiUrl;
  private apiKey = environment.apiKey;

  private get headers(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'X-Api-Key': this.apiKey,
    });
  }

  constructor(private http: HttpClient) {}

  /**
   * Fetches the API key from the server config endpoint.
   * Called during app initialization in production.
   */
  async loadConfig(): Promise<void> {
    if (environment.production) {
      const response = await firstValueFrom(
        this.http.get<ApiResponse<{ apiKey: string }>>(
          `${this.baseUrl}/config`,
        ),
      );
      this.apiKey = response.data?.apiKey ?? '';
    }
  }

  createEstimate(
    request: EstimateRequest,
  ): Observable<ApiResponse<EstimateResponse>> {
    return this.http.post<ApiResponse<EstimateResponse>>(
      `${this.baseUrl}/estimates`,
      request,
      { headers: this.headers },
    );
  }

  getEstimate(id: string): Observable<ApiResponse<EstimateResponse>> {
    return this.http.get<ApiResponse<EstimateResponse>>(
      `${this.baseUrl}/estimates/${id}`,
      { headers: this.headers },
    );
  }

  listEstimates(
    page = 1,
    pageSize = 20,
  ): Observable<ApiResponse<EstimateResponse[]>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApiResponse<EstimateResponse[]>>(
      `${this.baseUrl}/estimates`,
      { headers: this.headers, params },
    );
  }

  getRecommendations(
    estimateId: string,
  ): Observable<ApiResponse<RecommendationResponse[]>> {
    return this.http.get<ApiResponse<RecommendationResponse[]>>(
      `${this.baseUrl}/estimates/${estimateId}/recommendations`,
      { headers: this.headers },
    );
  }

  searchEmissionFactors(
    query: string,
    top = 10,
  ): Observable<ApiResponse<EmissionFactorResponse[]>> {
    const params = new HttpParams().set('q', query).set('top', top.toString());

    return this.http.get<ApiResponse<EmissionFactorResponse[]>>(
      `${this.baseUrl}/emission-factors/search`,
      { headers: this.headers, params },
    );
  }

  getRegions(): Observable<RegionResponse[]> {
    return this.http
      .get<ApiResponse<RegionResponse[]>>(`${this.baseUrl}/regions`)
      .pipe(map((response) => response.data ?? []));
  }
}
