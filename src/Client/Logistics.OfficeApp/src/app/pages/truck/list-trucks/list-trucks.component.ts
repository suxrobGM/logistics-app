import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterLink} from '@angular/router';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {InputTextModule} from 'primeng/inputtext';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {TooltipModule} from 'primeng/tooltip';
import {ButtonModule} from 'primeng/button';
import {TruckDto} from '@core/models';
import {ApiService} from '@core/services';
import {AddressPipe} from '@shared/pipes';


@Component({
  selector: 'app-list-trucks',
  templateUrl: './list-trucks.component.html',
  styleUrls: ['./list-trucks.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    SharedModule,
    InputTextModule,
    AddressPipe,
  ],
})
export class ListTruckComponent {
  public trucks: TruckDto[];
  public isLoading: boolean;
  public totalRecords: number;

  constructor(private apiService: ApiService) {
    this.trucks = [];
    this.isLoading = false;
    this.totalRecords = 0;
  }

  search(event: Event) {
    this.isLoading = true;
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getTrucks({search: searchValue}).subscribe((result) => {
      if (result.success && result.data) {
        this.trucks = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getTrucks({orderBy: sortField, page: page, pageSize: rows}).subscribe((result) => {
      if (result.success && result.data) {
        this.trucks = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }
}
