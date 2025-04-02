import {Routes} from '@angular/router';
import {ROUTE_PATHS} from '../../core/constants/routes.constants';
import {UserToolsComponent} from './user-tools.component';
import {KanbanBoardComponent} from './pages/kanban-board/kanban-board.component';
import {canActivateAuthRole} from '../../core/guards/auth-role.guard';
import {authGuard} from '../../core/guards/auth.guard';

export const userToolsRoutes: Routes = [
  {
    path: ROUTE_PATHS.USER_TOOLS,
    component: UserToolsComponent,
    canActivate: [authGuard],
    children: [
      { path: ROUTE_PATHS.KANBAN_BOARD, component: KanbanBoardComponent},
    ]
  }
]
