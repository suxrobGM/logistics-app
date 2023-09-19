import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {SharedModule} from '@shared/shared.module';
import {LoadRoutingModule} from './load-routing.module';
import {PrimengModule} from './primeng.module';
import {EditLoadComponent, ListLoadComponent} from './pages';


@NgModule({
  imports: [
    CommonModule,
    LoadRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    PrimengModule,
    ListLoadComponent,
    EditLoadComponent,
  ],
})
export class LoadModule { }
