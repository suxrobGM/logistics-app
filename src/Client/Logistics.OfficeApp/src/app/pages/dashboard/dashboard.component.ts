import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {OverallStatsComponent, TruckStatsTableComponent} from './components';
import {GrossesBarchartComponent} from '@shared/components';


@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: [],
  standalone: true,
  imports: [
    CommonModule,
    TruckStatsTableComponent,
    GrossesBarchartComponent,
    OverallStatsComponent,
  ],
})
export class DashboardComponent {
  constructor() {}
}
