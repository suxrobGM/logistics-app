import {Component, OnInit} from '@angular/core';
import {CommonModule, NgFor, NgIf, PercentPipe} from '@angular/common';
import {RouterLink} from '@angular/router';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {InputTextModule} from 'primeng/inputtext';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {TooltipModule} from 'primeng/tooltip';
import {ButtonModule} from 'primeng/button';
import {Truck} from '@core/models';
import {ApiService} from '@core/services';


@Component({
  selector: 'app-list-truck',
  templateUrl: './list-truck.component.html',
  styleUrls: ['./list-truck.component.scss'],
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
    PercentPipe,
  ],
})
export class ListTruckComponent implements OnInit {
  public trucks: Truck[];
  public isLoading: boolean;
  public totalRecords: number;

  constructor(private apiService: ApiService) {
    this.trucks = [];
    this.isLoading = false;
    this.totalRecords = 0;
  }

  ngOnInit(): void {}

  search(event: any) {
    const query = event.target.value;

    this.apiService.getTrucks(query, '', 1).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.trucks = result.data;
        this.totalRecords = result.totalItems!;
      }
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getTrucks('', sortField, page, event.rows!).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.trucks = result.data;
        this.totalRecords = result.totalItems!;
      }

      this.isLoading = false;
    });
  }
}
