import {Routes} from '@angular/router';
import {ROUTE_PATHS} from '../../core/constants/routes.constants';
import {PredictionsComponent} from './predictions.component';
import {NumberGeneratorComponent} from './pages/number-generator/number-generator.component';

export const predictionsRoutes: Routes = [
  {
    path: ROUTE_PATHS.PREDICTIONS,
    component: PredictionsComponent,
    children: [
      { path: ROUTE_PATHS.NUMBER_GENERATOR, component: NumberGeneratorComponent },
      { path: '', redirectTo: ROUTE_PATHS.OVERVIEW, pathMatch: 'full' }
    ]
  }
];
