import { Routes } from '@angular/router';
import {IndexComponent} from './pages/index/index.component';
import {dashboardRoutes} from './features/dashboard/dashboard.routes';

export const routes: Routes = [
  ...dashboardRoutes,
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
];
