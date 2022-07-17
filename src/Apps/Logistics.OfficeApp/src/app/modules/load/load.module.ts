import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadRoutingModule } from './load-routing.module';
import { ListLoadComponent } from './pages/list-load/list-load.component';
import { EditLoadComponent } from './pages/edit-load/edit-load.component';
import { SharedModule } from '@app/shared/shared.module';


@NgModule({
  declarations: [
    ListLoadComponent,
    EditLoadComponent
  ],
  imports: [
    CommonModule,
    LoadRoutingModule,
    SharedModule
  ]
})
export class LoadModule { }
