import {Component, OnInit} from '@angular/core';
import {DatePipe} from '@angular/common';
import {RouterLink} from '@angular/router';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {InputTextModule} from 'primeng/inputtext';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {TooltipModule} from 'primeng/tooltip';
import {ButtonModule} from 'primeng/button';
import {Employee} from '@core/models';
import {ApiService} from '@core/services';


@Component({
  selector: 'app-list-employees',
  templateUrl: './list-employees.component.html',
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
    DatePipe,
  ],
})
export class ListEmployeeComponent implements OnInit {
  employees: Employee[];
  isBusy: boolean;
  totalRecords: number;
  first: number;

  constructor(private apiService: ApiService) {
    this.employees = [];
    this.isBusy = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  ngOnInit(): void {
    this.isBusy = true;
  }

  search(event: any) {
    const query = event.target.value;

    this.apiService.getEmployees(query, '', 1).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.employees = result.data;
        this.totalRecords = result.totalItems!;
      }
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isBusy = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getEmployees('', sortField, page, event.rows!).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.employees = result.data;
        this.totalRecords = result.totalItems!;
      }

      this.isBusy = false;
    });
  }
}
