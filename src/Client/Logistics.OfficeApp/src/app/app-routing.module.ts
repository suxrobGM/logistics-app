import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'dashboard', 
    loadChildren: () => import('./modules/dashboard/dashboard.module').then(m => m.DashboardModule)
  },
  {
    path: 'employees', 
    loadChildren: () => import('./modules/employee/employee.module').then(m => m.EmployeeModule),
    data: {
      breadcrumb: 'Employees',
    }
  },
  {
    path: 'loads', 
    loadChildren: () => import('./modules/load/load.module').then(m => m.LoadModule),
    data: {
      breadcrumb: 'Loads',
    }
  },
  {
    path: 'trucks', 
    loadChildren: () => import('./modules/truck/truck.module').then(m => m.TruckModule),
    data: {
      breadcrumb: 'Trucks',
    }
  },
  {
    path: 'report', 
    loadChildren: () => import('./modules/report/report.module').then(m => m.ReportModule),
    data: {
      breadcrumb: 'Report',
    }
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
