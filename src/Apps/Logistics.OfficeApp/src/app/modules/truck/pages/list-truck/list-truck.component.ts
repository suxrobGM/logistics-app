import { Component, OnInit } from '@angular/core';
import { Truck } from '@shared/models/truck';
import { ApiService } from '@shared/services';
import { LazyLoadEvent } from 'primeng/api';

@Component({
  selector: 'app-list-truck',
  templateUrl: './list-truck.component.html',
  styleUrls: ['./list-truck.component.scss']
})
export class ListTruckComponent implements OnInit {
  public trucks: Truck[];
  public isBusy: boolean;
  public totalRecords: number;
  public first: number;

  constructor(private apiService: ApiService) {
    this.trucks = [];
    this.isBusy = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  public ngOnInit(): void {
    this.isBusy = true;
  }

  public search(event: any) {
    const query = event.target.value;
    //this.isBusy = true;
    
    this.apiService.getTrucks(query, 1).subscribe(result => {
      if (result.success && result.items) {
        this.trucks = result.items;
        this.totalRecords = result.itemsCount!;
      }

      //this.isBusy = false;
    });
  }

  public load(event: LazyLoadEvent) {
    this.isBusy = true;
    const page = event.first! / event.rows! + 1;
    
    this.apiService.getTrucks(undefined, page).subscribe(result => {
      if (result.success && result.items) {
        this.trucks = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.isBusy = false;
    });
  }
}
