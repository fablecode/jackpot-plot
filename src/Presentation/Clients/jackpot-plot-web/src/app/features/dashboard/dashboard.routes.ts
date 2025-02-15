import {Routes} from '@angular/router';
import {DashboardComponent} from './dashboard.component';
import {OverviewComponent} from './pages/overview/overview.component';
import {TrendsComponent} from './pages/trends/trends.component';
import {HotColdComponent} from './pages/hot-cold/hot-cold.component';
import {PatternsComponent} from './pages/patterns/patterns.component';
import {ROUTE_PATHS} from '../../core/constants/routes.constants';

export const dashboardRoutes: Routes = [
  {
    path: ROUTE_PATHS.DASHBOARD,
    component: DashboardComponent,
    children: [
      { path: ROUTE_PATHS.OVERVIEW, component: OverviewComponent },
      { path: ROUTE_PATHS.TRENDS, component: TrendsComponent },
      { path: ROUTE_PATHS.HOT_COLD_NUMBERS, component: HotColdComponent },
      { path: ROUTE_PATHS.WINNING_PATTERNS, component: PatternsComponent },
      { path: '', redirectTo: ROUTE_PATHS.OVERVIEW, pathMatch: 'full' }
    ]
  }
];
