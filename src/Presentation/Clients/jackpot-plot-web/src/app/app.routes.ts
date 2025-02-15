import { Routes } from '@angular/router';
import {dashboardRoutes} from './features/dashboard/dashboard.routes';
import {ROUTE_PATHS} from './core/constants/routes.constants';

export const routes: Routes = [
  ...dashboardRoutes,
  { path: '', redirectTo: ROUTE_PATHS.DASHBOARD, pathMatch: 'full' },
];
