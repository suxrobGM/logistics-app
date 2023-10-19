import {Component} from '@angular/core';
import {CommonModule, CurrencyPipe, DatePipe, PercentPipe} from '@angular/common';
import {RouterLink} from '@angular/router';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {InputTextModule} from 'primeng/inputtext';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {TooltipModule} from 'primeng/tooltip';
import {ButtonModule} from 'primeng/button';
import {Employee} from '@core/models';
import {ApiService} from '@core/services';
import {SalaryType, SalaryTypeEnum, getEnumDescription} from '@core/enums';


@Component({
  selector: 'app-list-employees',
  templateUrl: './list-employees.component.html',
  styleUrls: [],
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
    DatePipe,
    PercentPipe,
    CurrencyPipe,
  ],
})
export class ListEmployeeComponent {
  public employees: Employee[];
  public isLoading: boolean;
  public totalRecords: number;
  public first: number;

  constructor(private apiService: ApiService) {
    this.employees = [];
    this.isLoading = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  search(event: Event) {
    this.isLoading = true;
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getEmployees({search: searchValue}).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.employees = result.data;
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

    this.apiService.getEmployees({orderBy: sortField, page: page, pageSize: rows}).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.employees = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  getSalaryTypeName(enumValue: SalaryType): string {
    return getEnumDescription(SalaryTypeEnum, enumValue);
  }
}
