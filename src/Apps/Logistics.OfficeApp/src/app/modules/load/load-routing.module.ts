import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/auth.guard';
import { EditLoadComponent } from './pages/edit-load/edit-load.component';
import { ListLoadComponent } from './pages/list-load/list-load.component';

const rootRoutes: Routes = [
  { 
    path: '', 
    component: ListLoadComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['main.admin', 'tenant.owner', 'tenant.dispatcher']
    }
  },
  { 
    path: 'loads/edit', 
    component: EditLoadComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['main.admin', 'tenant.owner', 'tenant.dispatcher']
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class LoadRoutingModule {}