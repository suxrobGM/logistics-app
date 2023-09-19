import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {Load} from '@core/models';
import {ApiService} from '@core/services';
import {LoadStatus, LoadStatuses} from '@core/models';
import {DistanceUnitPipe} from '@shared/pipes';
import {CurrencyPipe, DatePipe} from '@angular/common';
import {InputTextModule} from 'primeng/inputtext';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {RouterLink} from '@angular/router';
import {TooltipModule} from 'primeng/tooltip';
import {ButtonModule} from 'primeng/button';


@Component({
  selector: 'app-list-load',
  templateUrl: './list-load.component.html',
  styleUrls: ['./list-load.component.scss'],
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
export class ListLoadComponent implements OnInit {
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

  ngOnInit(): void {
    this.isBusy = true;
  }

  search(event: any) {
    const query = event.target.value;

    this.apiService.getLoads(query, false).subscribe((result) => {
      if (result.success && result.items) {
        this.loads = result.items;
        this.totalRecords = result.itemsCount!;
      }
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isBusy = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getLoads('', false, sortField, page, event.rows!).subscribe((result) => {
      if (result.success && result.items) {
        this.loads = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.isBusy = false;
    });
  }

  getLoadStatusName(status: LoadStatus): string {
    return LoadStatuses[status - 1].displayName;
  }
}
