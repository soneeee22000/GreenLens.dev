import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { ApiService } from '../../services/api.service';
import {
  EstimateResponse,
  RecommendationResponse,
} from '../../models/api.models';

@Component({
  selector: 'app-estimate-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatChipsModule,
    MatIconModule,
  ],
  template: `
    <!-- Loading -->
    @if (loading) {
      <div class="center">
        <mat-spinner diameter="48"></mat-spinner>
      </div>
    }

    <!-- Error -->
    @if (errorMessage) {
      <mat-card class="error-card">
        <mat-card-content>
          <p>{{ errorMessage }}</p>
          <a mat-raised-button color="primary" routerLink="/"
            >Back to Dashboard</a
          >
        </mat-card-content>
      </mat-card>
    }

    <!-- Estimate Detail -->
    @if (!loading && !errorMessage && estimate) {
      <div class="header-row">
        <h1>Estimate Results</h1>
        <a mat-button routerLink="/">Back to Dashboard</a>
      </div>

      <!-- Summary -->
      <mat-card class="summary-card">
        <mat-card-content>
          <div class="total-co2e">
            {{ estimate.totalCo2eKg | number: '1.2-2' }} kg CO2e
          </div>
          <div class="meta">
            Created {{ estimate.createdAt | date: 'medium' }}
          </div>
        </mat-card-content>
      </mat-card>

      <!-- Resource Breakdown -->
      <mat-card>
        <mat-card-header>
          <mat-card-title>Resource Breakdown</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="estimate.breakdown" class="full-width">
            <ng-container matColumnDef="resourceType">
              <th mat-header-cell *matHeaderCellDef>Resource</th>
              <td mat-cell *matCellDef="let r">{{ r.resourceType }}</td>
            </ng-container>

            <ng-container matColumnDef="region">
              <th mat-header-cell *matHeaderCellDef>Region</th>
              <td mat-cell *matCellDef="let r">{{ r.region }}</td>
            </ng-container>

            <ng-container matColumnDef="quantity">
              <th mat-header-cell *matHeaderCellDef>Qty</th>
              <td mat-cell *matCellDef="let r">{{ r.quantity }}</td>
            </ng-container>

            <ng-container matColumnDef="hours">
              <th mat-header-cell *matHeaderCellDef>Hours</th>
              <td mat-cell *matCellDef="let r">{{ r.hours }}</td>
            </ng-container>

            <ng-container matColumnDef="co2e">
              <th mat-header-cell *matHeaderCellDef>CO2e (kg)</th>
              <td mat-cell *matCellDef="let r">
                {{ r.co2eKg | number: '1.4-4' }}
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="breakdownColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: breakdownColumns"></tr>
          </table>
        </mat-card-content>
      </mat-card>

      <!-- Recommendations -->
      @if (recommendations.length > 0) {
        <h2 class="section-title">AI Recommendations</h2>
        @for (rec of recommendations; track rec.title) {
          <mat-card class="rec-card">
            <mat-card-header>
              <mat-card-title>{{ rec.title }}</mat-card-title>
              <mat-chip-set class="effort-chip">
                <mat-chip [class]="'effort-' + rec.effort.toLowerCase()">
                  {{ rec.effort }} effort
                </mat-chip>
              </mat-chip-set>
            </mat-card-header>
            <mat-card-content>
              <p>{{ rec.description }}</p>
              <div class="reduction">
                <mat-icon>trending_down</mat-icon>
                ~{{ rec.estimatedReductionPercent }}% reduction
              </div>
            </mat-card-content>
          </mat-card>
        }
      }

      @if (recommendationsLoading) {
        <div class="center">
          <mat-spinner diameter="32"></mat-spinner>
          <span class="loading-text">Generating AI recommendations...</span>
        </div>
      }
    }
  `,
  styles: [
    `
      .center {
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 32px 0;
        gap: 12px;
      }
      .error-card {
        text-align: center;
        padding: 32px;
      }
      .header-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 16px;
      }
      .summary-card {
        margin-bottom: 24px;
        text-align: center;
      }
      .total-co2e {
        font-size: 2.5rem;
        font-weight: 700;
        color: #4caf50;
      }
      .meta {
        color: rgba(0, 0, 0, 0.6);
        margin-top: 4px;
      }
      .full-width {
        width: 100%;
      }
      .section-title {
        margin: 24px 0 16px;
      }
      .rec-card {
        margin-bottom: 12px;
      }
      .effort-chip {
        margin-left: auto;
      }
      .effort-low {
        background-color: #c8e6c9 !important;
      }
      .effort-medium {
        background-color: #fff9c4 !important;
      }
      .effort-high {
        background-color: #ffcdd2 !important;
      }
      .reduction {
        display: flex;
        align-items: center;
        gap: 4px;
        color: #4caf50;
        font-weight: 500;
        margin-top: 8px;
      }
      .loading-text {
        color: rgba(0, 0, 0, 0.6);
      }
      mat-card {
        margin-bottom: 16px;
      }
    `,
  ],
})
export class EstimateDetailComponent implements OnInit {
  estimate: EstimateResponse | null = null;
  recommendations: RecommendationResponse[] = [];
  loading = true;
  recommendationsLoading = false;
  errorMessage = '';
  breakdownColumns = ['resourceType', 'region', 'quantity', 'hours', 'co2e'];

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage = 'Invalid estimate ID.';
      this.loading = false;
      return;
    }

    this.api.getEstimate(id).subscribe({
      next: (response) => {
        this.estimate = response.data;
        this.loading = false;
        this.loadRecommendations(id);
      },
      error: () => {
        this.errorMessage = 'Failed to load estimate.';
        this.loading = false;
      },
    });
  }

  private loadRecommendations(id: string): void {
    this.recommendationsLoading = true;
    this.api.getRecommendations(id).subscribe({
      next: (response) => {
        this.recommendations = response.data ?? [];
        this.recommendationsLoading = false;
      },
      error: () => {
        this.recommendationsLoading = false;
      },
    });
  }
}
