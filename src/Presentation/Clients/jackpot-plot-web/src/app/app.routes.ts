import { Routes } from '@angular/router';
import {dashboardRoutes} from './features/dashboard/dashboard.routes';
import {ROUTE_PATHS} from './core/constants/routes.constants';
import {predictionsRoutes} from './features/predictions/predictions.routes';

export const routes: Routes = [
  ...dashboardRoutes,
  ...predictionsRoutes,
  { path: '', redirectTo: ROUTE_PATHS.DASHBOARD, pathMatch: 'full' },
];
