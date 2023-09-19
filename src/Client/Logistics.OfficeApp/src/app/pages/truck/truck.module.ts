import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {SharedModule} from '@shared/shared.module';
import {TruckRoutingModule} from './truck-routing.module';
import {PrimengModule} from './primeng.module';
import {EditTruckComponent, ListTruckComponent} from './pages';


@NgModule({
  imports: [
    CommonModule,
    TruckRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    PrimengModule,
    EditTruckComponent,
    ListTruckComponent,
  ],
})
export class TruckModule { }
