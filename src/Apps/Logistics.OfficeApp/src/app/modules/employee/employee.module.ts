import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ListEmployeeComponent } from './pages/list-employee/list-employee.component';
import { EditEmployeeComponent } from './pages/edit-employee/edit-employee.component';
import { EmployeeRoutingModule } from './employee-routing.module';
import { AddEmployeeComponent } from './pages/add-employee/add-employee.component';

@NgModule({
  declarations: [
    ListEmployeeComponent,
    EditEmployeeComponent,
    AddEmployeeComponent
  ],
  imports: [
    CommonModule,
    EmployeeRoutingModule,
    SharedModule,
    ReactiveFormsModule
  ]
})
export class EmployeeModule { }
