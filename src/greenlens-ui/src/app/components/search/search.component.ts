import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import {
  Subject,
  debounceTime,
  distinctUntilChanged,
  switchMap,
  of,
} from 'rxjs';
import { ApiService } from '../../services/api.service';
import { EmissionFactorResponse } from '../../models/api.models';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  template: `
    <h1>Search Emission Factors</h1>

    <mat-card>
      <mat-card-content>
        <mat-form-field appearance="outline" class="search-field">
          <mat-label
            >Search (e.g., "carbon cost of D4s VM in East US")</mat-label
          >
          <input
            matInput
            [(ngModel)]="query"
            (ngModelChange)="onQueryChange($event)"
            placeholder="Type to search..."
          />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
      </mat-card-content>
    </mat-card>

    <!-- Loading -->
    @if (loading) {
      <div class="center">
        <mat-spinner diameter="32"></mat-spinner>
      </div>
    }

    <!-- Results -->
    @if (!loading && results.length > 0) {
      <mat-card>
        <mat-card-header>
          <mat-card-title>{{ results.length }} results found</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="results" class="full-width">
            <ng-container matColumnDef="resourceType">
              <th mat-header-cell *matHeaderCellDef>Resource Type</th>
              <td mat-cell *matCellDef="let f">{{ f.resourceType }}</td>
            </ng-container>

            <ng-container matColumnDef="provider">
              <th mat-header-cell *matHeaderCellDef>Provider</th>
              <td mat-cell *matCellDef="let f">{{ f.provider }}</td>
            </ng-container>

            <ng-container matColumnDef="region">
              <th mat-header-cell *matHeaderCellDef>Region</th>
              <td mat-cell *matCellDef="let f">{{ f.region }}</td>
            </ng-container>

            <ng-container matColumnDef="co2ePerUnit">
              <th mat-header-cell *matHeaderCellDef>CO2e/Unit</th>
              <td mat-cell *matCellDef="let f">
                {{ f.co2ePerUnit | number: '1.4-6' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="unit">
              <th mat-header-cell *matHeaderCellDef>Unit</th>
              <td mat-cell *matCellDef="let f">{{ f.unit }}</td>
            </ng-container>

            <ng-container matColumnDef="source">
              <th mat-header-cell *matHeaderCellDef>Source</th>
              <td mat-cell *matCellDef="let f">{{ f.source }}</td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    }

    <!-- Empty -->
    @if (!loading && searched && results.length === 0) {
      <mat-card class="empty-card">
        <mat-card-content>
          <mat-icon class="empty-icon">search_off</mat-icon>
          <p>No emission factors found for "{{ query }}"</p>
        </mat-card-content>
      </mat-card>
    }

    <!-- Error -->
    @if (errorMessage) {
      <mat-card class="error-card">
        <mat-card-content>
          <p>{{ errorMessage }}</p>
        </mat-card-content>
      </mat-card>
    }
  `,
  styles: [
    `
      .search-field {
        width: 100%;
      }
      .center {
        display: flex;
        justify-content: center;
        padding: 32px 0;
      }
      .full-width {
        width: 100%;
      }
      .empty-card,
      .error-card {
        text-align: center;
        padding: 32px;
        margin-top: 16px;
      }
      .empty-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
        color: rgba(0, 0, 0, 0.3);
      }
      h1 {
        margin-bottom: 24px;
      }
      mat-card {
        margin-bottom: 16px;
      }
    `,
  ],
})
export class SearchComponent {
  query = '';
  results: EmissionFactorResponse[] = [];
  loading = false;
  searched = false;
  errorMessage = '';
  displayedColumns = [
    'resourceType',
    'provider',
    'region',
    'co2ePerUnit',
    'unit',
    'source',
  ];

  private searchSubject = new Subject<string>();

  constructor(private api: ApiService) {
    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((query) => {
          if (query.trim().length < 2) {
            this.results = [];
            this.searched = false;
            return of(null);
          }
          this.loading = true;
          this.errorMessage = '';
          return this.api.searchEmissionFactors(query);
        }),
      )
      .subscribe({
        next: (response) => {
          if (response) {
            this.results = response.data ?? [];
            this.searched = true;
          }
          this.loading = false;
        },
        error: () => {
          this.errorMessage = 'Search failed. Please try again.';
          this.loading = false;
        },
      });
  }

  onQueryChange(value: string): void {
    this.searchSubject.next(value);
  }
}
