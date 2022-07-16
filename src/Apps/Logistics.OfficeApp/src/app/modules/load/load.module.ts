import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadRoutingModule } from './load-routing.module';
import { ListLoadComponent } from './pages/list-load/list-load.component';
import { EditLoadComponent } from './pages/edit-load/edit-load.component';
import { SharedModule } from '@app/shared/shared.module';
import { LoadService } from './shared/load.service';


@NgModule({
  declarations: [
    ListLoadComponent,
    EditLoadComponent
  ],
  imports: [
    CommonModule,
    LoadRoutingModule,
    SharedModule
  ],
  providers: [
    LoadService
  ]
})
export class LoadModule { }
