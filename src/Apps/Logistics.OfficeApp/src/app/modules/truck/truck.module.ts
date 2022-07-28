import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { EditTruckComponent } from './pages/edit-truck/edit-truck.component';
import { ListTruckComponent } from './pages/list-truck/list-truck.component';
import { TruckRoutingModule } from './truck-routing.module';
import { ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [
    EditTruckComponent,
    ListTruckComponent
  ],
  imports: [
    CommonModule,
    TruckRoutingModule,
    SharedModule,
    ReactiveFormsModule
  ]
})
export class TruckModule { }
