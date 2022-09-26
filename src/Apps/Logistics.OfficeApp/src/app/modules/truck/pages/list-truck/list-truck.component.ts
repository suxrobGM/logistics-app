import { Component, OnInit } from '@angular/core';
import { Truck } from '@shared/models';
import { ApiService } from '@shared/services';
import { LazyLoadEvent } from 'primeng/api';

@Component({
  selector: 'app-list-truck',
  templateUrl: './list-truck.component.html',
  styleUrls: ['./list-truck.component.scss']
})
export class ListTruckComponent implements OnInit {
  trucks: Truck[];
  loading: boolean;
  totalRecords: number;
  first: number;

  constructor(private apiService: ApiService) {
    this.trucks = [];
    this.loading = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  ngOnInit(): void {
    this.loading = true;
  }

  search(event: any) {
    const query = event.target.value;
    
    this.apiService.getTrucks(query, '', 1).subscribe(result => {
      if (result.success && result.items) {
        this.trucks = result.items;
        this.totalRecords = result.itemsCount!;
      }
    });
  }

  load(event: LazyLoadEvent) {
    this.loading = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField, event.sortOrder);
    
    this.apiService.getTrucks('', sortField, page, event.rows).subscribe(result => {
      if (result.success && result.items) {
        this.trucks = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.loading = false;
    });
  }
}
