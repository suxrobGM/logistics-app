import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SharedModule } from './shared/shared.module';
import { DashboardModule } from './modules/dashboard/dashboard.module';
import { RootModule } from './modules/root/root.module';
import { AuthModule } from './modules/auth/auth.module';
import { LoadModule } from './modules/load/load.module';
import { CoreModule } from './modules/core/core.module';
import { AppConfig, APP_CONFIG } from './configs/app.config';
import { EmployeeModule } from './modules/employee/employee.module';
import { TruckModule } from './modules/truck/truck.module';
import { StatsModule } from '@modules/stats/stats.module';


@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    CoreModule,
    DashboardModule,
    AuthModule,
    RootModule,
    SharedModule,
  ],
  providers: [
    { provide: APP_CONFIG, useValue: AppConfig }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
