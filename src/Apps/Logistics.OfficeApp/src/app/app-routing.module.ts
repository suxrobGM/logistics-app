import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'dashboard', 
    loadChildren: () => import('./modules/dashboard/dashboard.module').then(m => m.DashboardModule)
  },
  {
    path: 'employees', 
    loadChildren: () => import('./modules/employee/employee.module').then(m => m.EmployeeModule)
  },
  {
    path: 'loads', 
    loadChildren: () => import('./modules/load/load.module').then(m => m.LoadModule)
  },
  {
    path: 'trucks', 
    loadChildren: () => import('./modules/truck/truck.module').then(m => m.TruckModule)
  },
  {
    path: 'stats', 
    loadChildren: () => import('./modules/stats/stats.module').then(m => m.StatsModule)
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
