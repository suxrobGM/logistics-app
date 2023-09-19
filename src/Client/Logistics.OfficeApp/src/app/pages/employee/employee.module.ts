import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {EmployeeRoutingModule} from './employee-routing.module';
import {PrimengModule} from './primeng.module';
import {UserService} from './services/user.service';
import {ChangeRoleDialogComponent} from './components';
import {AddEmployeeComponent} from './add-employee/add-employee.component';
import {EditEmployeeComponent} from './edit-employee/edit-employee.component';
import {ListEmployeeComponent} from './list-employee/list-employee.component';


@NgModule({
  imports: [
    CommonModule,
    EmployeeRoutingModule,
    ReactiveFormsModule,
    PrimengModule,
    ListEmployeeComponent,
    EditEmployeeComponent,
    AddEmployeeComponent,
    ChangeRoleDialogComponent,
  ],
  providers: [
    UserService,
  ],
})
export class EmployeeModule { }
