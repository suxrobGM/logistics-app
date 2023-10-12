import {Component, ViewEncapsulation} from '@angular/core';
import {CurrencyPipe, DatePipe} from '@angular/common';
import {RouterLink} from '@angular/router';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {InputTextModule} from 'primeng/inputtext';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {TooltipModule} from 'primeng/tooltip';
import {ButtonModule} from 'primeng/button';
import {Load} from '@core/models';
import {ApiService} from '@core/services';
import {LoadStatus, LoadStatuses} from '@core/models';
import {DistanceUnitPipe} from '@shared/pipes';


@Component({
  selector: 'app-list-load',
  templateUrl: './list-load.component.html',
  styleUrls: [],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    SharedModule,
    InputTextModule,
    CurrencyPipe,
    DatePipe,
    DistanceUnitPipe,
  ],
})
export class ListLoadComponent {
  loads: Load[];
  isBusy: boolean;
  totalRecords: number;
  first: number;

  constructor(private apiService: ApiService) {
    this.loads = [];
    this.isBusy = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  search(event: any) {
    const query = event.target.value;

    this.apiService.getLoads(query, false).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.loads = result.data;
        this.totalRecords = result.totalItems!;
      }
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isBusy = true;
    const page = event.first! / event.rows! + 1;
    let sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    if (sortField === '') { // default sort property
      sortField = '-dispatchedDate';
    }

    this.apiService.getLoads('', false, sortField, page, event.rows!).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.loads = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isBusy = false;
    });
  }

  getLoadStatusName(status: LoadStatus): string {
    return LoadStatuses[status - 1].description;
  }
}
