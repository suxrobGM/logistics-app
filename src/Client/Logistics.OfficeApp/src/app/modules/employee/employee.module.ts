import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {SharedModule} from '@shared/shared.module';
import {EmployeeRoutingModule} from './employee-routing.module';
import {PrimengModule} from './primeng.module';
import {UserService} from './shared/user.service';
import {AddEmployeeComponent} from './add-employee/add-employee.component';
import {ChangeRoleDialogComponent} from './change-role-dialog/change-role-dialog.component';
import {EditEmployeeComponent} from './edit-employee/edit-employee.component';
import {ListEmployeeComponent} from './list-employee/list-employee.component';


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
