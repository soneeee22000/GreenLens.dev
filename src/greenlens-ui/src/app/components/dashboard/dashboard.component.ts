import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { NgChartsModule } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { ApiService } from '../../services/api.service';
import { EstimateResponse } from '../../models/api.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatIconModule,
    NgChartsModule,
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
          <button mat-raised-button color="primary" (click)="loadEstimates()">
            Retry
          </button>
        </mat-card-content>
      </mat-card>
    }

    <!-- Empty State -->
    @if (!loading && !errorMessage && estimates.length === 0) {
      <mat-card class="empty-card">
        <mat-card-content>
          <mat-icon class="empty-icon">eco</mat-icon>
          <h2>No estimates yet</h2>
          <p>Create your first carbon footprint estimate to start tracking.</p>
          <a mat-raised-button color="primary" routerLink="/estimate/new"
            >Create Estimate</a
          >
        </mat-card-content>
      </mat-card>
    }

    <!-- Dashboard Content -->
    @if (!loading && !errorMessage && estimates.length > 0) {
      <h1>Carbon Footprint Dashboard</h1>

      <!-- Summary Cards -->
      <div class="summary-row">
        <mat-card>
          <mat-card-content>
            <div class="metric-label">Total Estimates</div>
            <div class="metric-value">{{ totalEstimates }}</div>
          </mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-content>
            <div class="metric-label">Latest CO2e</div>
            <div class="metric-value">
              {{ estimates[0].totalCo2eKg | number: '1.2-2' }} kg
            </div>
          </mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-content>
            <div class="metric-label">Total CO2e (all time)</div>
            <div class="metric-value">{{ totalCo2e | number: '1.2-2' }} kg</div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Chart -->
      <mat-card class="chart-card">
        <mat-card-header>
          <mat-card-title>CO2e Over Time</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <canvas
            baseChart
            [datasets]="chartData.datasets"
            [labels]="chartData.labels"
            [options]="chartOptions"
            type="line"
          >
          </canvas>
        </mat-card-content>
      </mat-card>

      <!-- Estimates Table -->
      <mat-card>
        <mat-card-header>
          <mat-card-title>Estimation History</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="estimates" class="full-width">
            <ng-container matColumnDef="date">
              <th mat-header-cell *matHeaderCellDef>Date</th>
              <td mat-cell *matCellDef="let e">
                {{ e.createdAt | date: 'short' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="resources">
              <th mat-header-cell *matHeaderCellDef>Resources</th>
              <td mat-cell *matCellDef="let e">{{ e.breakdown.length }}</td>
            </ng-container>

            <ng-container matColumnDef="co2e">
              <th mat-header-cell *matHeaderCellDef>CO2e (kg)</th>
              <td mat-cell *matCellDef="let e">
                {{ e.totalCo2eKg | number: '1.2-2' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let e">
                <a
                  mat-button
                  color="primary"
                  [routerLink]="['/estimate', e.estimateId]"
                  >View</a
                >
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    }
  `,
  styles: [
    `
      .center {
        display: flex;
        justify-content: center;
        padding: 64px 0;
      }
      .error-card {
        text-align: center;
        padding: 32px;
      }
      .empty-card {
        text-align: center;
        padding: 48px;
      }
      .empty-icon {
        font-size: 64px;
        width: 64px;
        height: 64px;
        color: #4caf50;
      }
      .summary-row {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 16px;
        margin-bottom: 24px;
      }
      .metric-label {
        font-size: 0.875rem;
        color: rgba(0, 0, 0, 0.6);
      }
      .metric-value {
        font-size: 1.5rem;
        font-weight: 700;
        margin-top: 4px;
      }
      .chart-card {
        margin-bottom: 24px;
      }
      .full-width {
        width: 100%;
      }
      h1 {
        margin-bottom: 24px;
      }
    `,
  ],
})
export class DashboardComponent implements OnInit {
  estimates: EstimateResponse[] = [];
  loading = true;
  errorMessage = '';
  totalEstimates = 0;
  totalCo2e = 0;
  displayedColumns = ['date', 'resources', 'co2e', 'actions'];

  chartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  chartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    plugins: { legend: { display: false } },
    scales: {
      y: { title: { display: true, text: 'CO2e (kg)' } },
    },
  };

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadEstimates();
  }

  loadEstimates(): void {
    this.loading = true;
    this.errorMessage = '';

    this.api.listEstimates(1, 50).subscribe({
      next: (response) => {
        this.estimates = response.data ?? [];
        this.totalEstimates = response.meta?.total ?? this.estimates.length;
        this.totalCo2e = this.estimates.reduce(
          (sum, e) => sum + e.totalCo2eKg,
          0,
        );
        this.buildChart();
        this.loading = false;
      },
      error: () => {
        this.errorMessage =
          'Failed to load estimates. Please check your connection and try again.';
        this.loading = false;
      },
    });
  }

  private buildChart(): void {
    const sorted = [...this.estimates].reverse();
    this.chartData = {
      labels: sorted.map((e) => new Date(e.createdAt).toLocaleDateString()),
      datasets: [
        {
          data: sorted.map((e) => e.totalCo2eKg),
          label: 'CO2e (kg)',
          borderColor: '#4caf50',
          backgroundColor: 'rgba(76, 175, 80, 0.1)',
          fill: true,
          tension: 0.3,
        },
      ],
    };
  }
}
