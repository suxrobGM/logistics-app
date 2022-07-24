import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/auth.guard';
import { EditTruckComponent } from './pages/edit-truck/edit-truck.component';
import { ListTruckComponent } from './pages/list-truck/list-truck.component';

const rootRoutes: Routes = [
  { 
    path: '', 
    component: ListTruckComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['admin', 'owner', 'dispatcher']
    }
  },
  { 
    path: 'trucks/edit', 
    component: EditTruckComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['admin', 'owner', 'dispatcher']
    }
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class TruckRoutingModule {}