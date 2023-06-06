import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {SharedModule} from '@shared/index';
import {LoadRoutingModule} from './load-routing.module';
import {PrimengModule} from './primeng.module';
import {EditLoadComponent, ListLoadComponent} from './pages';

@NgModule({
  declarations: [
    ListLoadComponent,
    EditLoadComponent,
  ],
  imports: [
    CommonModule,
    LoadRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    PrimengModule,
  ],
})
export class LoadModule { }
