import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Truck, TruckGrosses } from '@shared/models';
import { ApiService } from '@shared/services';
import { DateUtils } from '@shared/utils';

@Component({
  selector: 'app-truck-stats',
  templateUrl: './truck-stats.component.html',
  styleUrls: ['./truck-stats.component.scss']
})
export class TruckStatsComponent implements OnInit {
  public id!: string;
  public isBusy: boolean;
  public truck?: Truck;
  public truckGrosses?: TruckGrosses;

  constructor(
    private apiService: ApiService,
    private route: ActivatedRoute,
    private dateUtils: DateUtils) 
  {
    this.isBusy = false;
  }

  public ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.id = params['id'];
    });

    this.isBusy = true;
    this.fetchTruck();
    this.fetchTruckGrosses();
  }

  private fetchTruck() {
    this.apiService.getTruck(this.id).subscribe(result => {
      if (result.success) {
        this.truck = result.value;
      }
    });
  }

  private fetchTruckGrosses() {
    this.isBusy = true;
    const oneMonthAgo = this.dateUtils.daysAgo(30);

    this.apiService.getTruckGrossesForInterval(this.id, oneMonthAgo).subscribe(result => {
      if (result.success) {
        this.truckGrosses = result.value;
      }

      this.isBusy = false;
    });
  }
}
