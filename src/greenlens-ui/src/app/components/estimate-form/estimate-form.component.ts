import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
} from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../services/api.service';
import { RegionResponse } from '../../models/api.models';

@Component({
  selector: 'app-estimate-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  template: `
    <h1>New Carbon Estimate</h1>

    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <div formArrayName="resources">
        @for (resource of resources.controls; track resource; let i = $index) {
          <mat-card class="resource-card">
            <mat-card-header>
              <mat-card-title>Resource {{ i + 1 }}</mat-card-title>
              @if (resources.length > 1) {
                <button
                  mat-icon-button
                  type="button"
                  (click)="removeResource(i)"
                  class="remove-btn"
                >
                  <mat-icon>close</mat-icon>
                </button>
              }
            </mat-card-header>
            <mat-card-content [formGroupName]="i">
              <div class="form-row">
                <mat-form-field appearance="outline">
                  <mat-label>Resource Type</mat-label>
                  <mat-select formControlName="resourceType">
                    @for (type of resourceTypes; track type) {
                      <mat-option [value]="type">{{ type }}</mat-option>
                    }
                  </mat-select>
                  <mat-error>Required</mat-error>
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Region</mat-label>
                  <mat-select formControlName="region">
                    @for (region of regions; track region.name) {
                      <mat-option [value]="region.name">{{
                        region.displayName
                      }}</mat-option>
                    }
                  </mat-select>
                  <mat-error>Required</mat-error>
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline">
                  <mat-label>Quantity</mat-label>
                  <input
                    matInput
                    type="number"
                    formControlName="quantity"
                    min="1"
                  />
                  <mat-error>Must be at least 1</mat-error>
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Hours</mat-label>
                  <input
                    matInput
                    type="number"
                    formControlName="hours"
                    min="0"
                  />
                  <mat-error>Must be 0 or more</mat-error>
                </mat-form-field>
              </div>
            </mat-card-content>
          </mat-card>
        }
      </div>

      <div class="actions">
        <button mat-stroked-button type="button" (click)="addResource()">
          <mat-icon>add</mat-icon> Add Resource
        </button>
        <button
          mat-raised-button
          color="primary"
          type="submit"
          [disabled]="submitting || form.invalid"
        >
          @if (submitting) {
            <mat-spinner diameter="20"></mat-spinner>
          } @else {
            Calculate CO2e
          }
        </button>
      </div>
    </form>
  `,
  styles: [
    `
      .resource-card {
        margin-bottom: 16px;
        position: relative;
      }
      .remove-btn {
        position: absolute;
        top: 8px;
        right: 8px;
      }
      .form-row {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 16px;
      }
      .actions {
        display: flex;
        justify-content: space-between;
        margin-top: 16px;
      }
      h1 {
        margin-bottom: 24px;
      }
      mat-form-field {
        width: 100%;
      }
    `,
  ],
})
export class EstimateFormComponent implements OnInit {
  form: FormGroup;
  regions: RegionResponse[] = [];
  submitting = false;

  resourceTypes = [
    'Standard_B1s',
    'Standard_B2s',
    'Standard_D2s_v3',
    'Standard_D4s_v3',
    'Standard_D8s_v3',
    'Standard_E2s_v3',
    'Standard_E4s_v3',
    'Standard_F2s_v2',
    'Standard_F4s_v2',
    'BlobStorage',
    'ManagedDisk_StandardSSD',
    'ManagedDisk_PremiumSSD',
    'AzureSQL_Basic',
    'AzureSQL_Standard',
  ];

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private router: Router,
    private snackBar: MatSnackBar,
  ) {
    this.form = this.fb.group({
      resources: this.fb.array([this.createResourceGroup()]),
    });
  }

  get resources(): FormArray {
    return this.form.get('resources') as FormArray;
  }

  ngOnInit(): void {
    this.api.getRegions().subscribe({
      next: (regions) => (this.regions = regions),
      error: () =>
        this.snackBar.open('Failed to load regions', 'Dismiss', {
          duration: 5000,
        }),
    });
  }

  createResourceGroup(): FormGroup {
    return this.fb.group({
      resourceType: ['', Validators.required],
      region: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      hours: [720, [Validators.required, Validators.min(0)]],
    });
  }

  addResource(): void {
    this.resources.push(this.createResourceGroup());
  }

  removeResource(index: number): void {
    this.resources.removeAt(index);
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.submitting = true;
    const request = { resources: this.form.value.resources };

    this.api.createEstimate(request).subscribe({
      next: (response) => {
        this.submitting = false;
        if (response.data) {
          this.router.navigate(['/estimate', response.data.estimateId]);
        }
      },
      error: (err) => {
        this.submitting = false;
        const message =
          err.error?.error?.message ?? 'Failed to create estimate';
        this.snackBar.open(message, 'Dismiss', { duration: 5000 });
      },
    });
  }
}
