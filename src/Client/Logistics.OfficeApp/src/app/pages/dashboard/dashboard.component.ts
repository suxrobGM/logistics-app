import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {
  GrossesChartComponent,
  OverallStatsComponent,
  TruckStatsTableComponent,
} from './components';


@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: [],
  standalone: true,
  imports: [
    CommonModule,
    TruckStatsTableComponent,
    GrossesChartComponent,
    OverallStatsComponent,
  ],
})
export class DashboardComponent {
  constructor() {}
}
