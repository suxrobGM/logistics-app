import {Component, OnInit} from '@angular/core';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {Truck} from '@core/models';
import {ApiService} from '@core/services';
import {NgFor, NgIf} from '@angular/common';
import {InputTextModule} from 'primeng/inputtext';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {RouterLink} from '@angular/router';
import {TooltipModule} from 'primeng/tooltip';
import {ButtonModule} from 'primeng/button';


@Component({
  selector: 'app-list-truck',
  templateUrl: './list-truck.component.html',
  styleUrls: ['./list-truck.component.scss'],
  standalone: true,
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    SharedModule,
    InputTextModule,
    NgFor,
    NgIf,
  ],
})
export class ListTruckComponent implements OnInit {
  trucks: Truck[];
  isBusy: boolean;
  totalRecords: number;
  first: number;

  constructor(private apiService: ApiService) {
    this.trucks = [];
    this.isBusy = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  ngOnInit(): void {
    this.isBusy = true;
  }

  search(event: any) {
    const query = event.target.value;

    this.apiService.getTrucks(query, '', 1).subscribe((result) => {
      if (result.success && result.items) {
        this.trucks = result.items;
        this.totalRecords = result.itemsCount!;
      }
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isBusy = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getTrucks('', sortField, page, event.rows!).subscribe((result) => {
      if (result.success && result.items) {
        this.trucks = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.isBusy = false;
    });
  }
}
