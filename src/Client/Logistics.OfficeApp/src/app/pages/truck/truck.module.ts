import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {TruckRoutingModule} from './truck-routing.module';
import {PrimengModule} from './primeng.module';
import {EditTruckComponent} from './edit-truck/edit-truck.component';
import {ListTruckComponent} from './list-truck/list-truck.component';


@NgModule({
  imports: [
    CommonModule,
    TruckRoutingModule,
    ReactiveFormsModule,
    PrimengModule,
    EditTruckComponent,
    ListTruckComponent,
  ],
})
export class TruckModule { }
