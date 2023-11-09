import {Component} from '@angular/core';
import {CurrencyPipe, DatePipe} from '@angular/common';
import {RouterLink} from '@angular/router';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {InputTextModule} from 'primeng/inputtext';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {TooltipModule} from 'primeng/tooltip';
import {ButtonModule} from 'primeng/button';
import {LoadStatus, LoadStatusEnum} from '@core/enums';
import {Load} from '@core/models';
import {ApiService} from '@core/services';
import {DistanceUnitPipe} from '@shared/pipes';


@Component({
  selector: 'app-list-loads',
  templateUrl: './list-loads.component.html',
  styleUrls: [],
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
  public loads: Load[];
  public isLoading: boolean;
  public totalRecords: number;
  public first: number;

  constructor(private apiService: ApiService) {
    this.loads = [];
    this.isLoading = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  search(event: Event) {
    this.isLoading = true;
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getLoads({search: searchValue}, false).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.loads = result.data;
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
    let sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    if (sortField === '') { // default sort property
      sortField = '-dispatchedDate';
    }

    this.apiService.getLoads({orderBy: sortField, page: page, pageSize: rows}, false).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.loads = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  getLoadStatusDesc(enumValue: LoadStatus): string {
    return LoadStatusEnum.getDescription(enumValue);
  }
}
