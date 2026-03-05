import { Routes } from '@angular/router';
import { LayoutComponent } from './components/layout/layout.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { EstimateFormComponent } from './components/estimate-form/estimate-form.component';
import { EstimateDetailComponent } from './components/estimate-detail/estimate-detail.component';
import { SearchComponent } from './components/search/search.component';

export const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: '', component: DashboardComponent },
      { path: 'estimate/new', component: EstimateFormComponent },
      { path: 'estimate/:id', component: EstimateDetailComponent },
      { path: 'search', component: SearchComponent },
    ],
  },
];
