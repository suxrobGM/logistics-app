import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'loads', 
    loadChildren: () => import('./modules/load/load.module').then(m => m.LoadModule)
  },
  {
    path: 'trucks', 
    loadChildren: () => import('./modules/truck/truck.module').then(m => m.TruckModule)
  },
  {
    path: 'employees', 
    loadChildren: () => import('./modules/employee/employee.module').then(m => m.EmployeeModule)
  },
  {
    path: 'stats', 
    loadChildren: () => import('./modules/stats/stats.module').then(m => m.StatsModule)
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full'}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
