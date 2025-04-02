import { Routes } from '@angular/router';
import {dashboardRoutes} from './features/dashboard/dashboard.routes';
import {ROUTE_PATHS} from './core/constants/routes.constants';
import {predictionsRoutes} from './features/predictions/predictions.routes';
import {userToolsRoutes} from './features/user-tools/user-tools.routes';

export const routes: Routes = [
  ...dashboardRoutes,
  ...predictionsRoutes,
  ...userToolsRoutes,
  { path: '', redirectTo: ROUTE_PATHS.DASHBOARD, pathMatch: 'full' },
];
