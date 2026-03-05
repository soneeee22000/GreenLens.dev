import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
  ],
  template: `
    <mat-toolbar color="primary">
      <span class="logo" routerLink="/">GreenLens</span>
      <span class="spacer"></span>
      <a
        mat-button
        routerLink="/"
        routerLinkActive="active"
        [routerLinkActiveOptions]="{ exact: true }"
      >
        Dashboard
      </a>
      <a mat-button routerLink="/estimate/new" routerLinkActive="active">
        New Estimate
      </a>
      <a mat-button routerLink="/search" routerLinkActive="active">
        Search Factors
      </a>
    </mat-toolbar>
    <main class="content">
      <router-outlet />
    </main>
  `,
  styles: [
    `
      .logo {
        font-weight: 700;
        cursor: pointer;
        font-size: 1.2rem;
      }
      .spacer {
        flex: 1 1 auto;
      }
      .active {
        border-bottom: 2px solid white;
      }
      .content {
        max-width: 1200px;
        margin: 24px auto;
        padding: 0 16px;
      }
    `,
  ],
})
export class LayoutComponent {}
