import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {SharedModule} from '@shared/index';
import {EmployeeRoutingModule} from './employee-routing.module';
import {PrimengModule} from './primeng.module';
import {AddEmployeeComponent, EditEmployeeComponent, ListEmployeeComponent} from './pages';
import {ChangeRoleDialogComponent} from './components';
import {UserService} from './shared/user.service';


@NgModule({
  declarations: [
    ListEmployeeComponent,
    EditEmployeeComponent,
    AddEmployeeComponent,
    ChangeRoleDialogComponent,
  ],
  imports: [
    CommonModule,
    EmployeeRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    PrimengModule,
  ],
  providers: [
    UserService,
  ],
})
export class EmployeeModule { }
