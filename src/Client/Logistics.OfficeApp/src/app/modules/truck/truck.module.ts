import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {SharedModule} from '@shared/shared.module';
import {TruckRoutingModule} from './truck-routing.module';
import {PrimengModule} from './primeng.module';
import {EditTruckComponent} from './edit-truck/edit-truck.component';
import {ListTruckComponent} from './list-truck/list-truck.component';

@NgModule({
  declarations: [
    EditTruckComponent,
    ListTruckComponent,
  ],
  imports: [
    CommonModule,
    TruckRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    PrimengModule,
  ],
})
export class TruckModule { }
