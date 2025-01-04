import {RouterModule, Routes} from '@angular/router';
import {NgModule} from '@angular/core';
import {LayoutComponent} from './layout/layout.component';
import {HomeComponent} from './pages/home/home.component';

export const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: '',
        component: LayoutComponent,
        children: [
          { path: '', component: HomeComponent }
        ]
      },
      { path: 'home', component: HomeComponent },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
