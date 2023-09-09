import {Component, OnInit} from '@angular/core';
import {TableLazyLoadEvent} from 'primeng/table';
import {Employee} from '@shared/models';
import {ApiService} from '@shared/services';


@Component({
  selector: 'app-list-employee',
  templateUrl: './list-employee.component.html',
  styleUrls: ['./list-employee.component.scss'],
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
      if (result.success && result.items) {
        this.employees = result.items;
        this.totalRecords = result.itemsCount!;
      }
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isBusy = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getEmployees('', sortField, page, event.rows!).subscribe((result) => {
      if (result.success && result.items) {
        this.employees = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.isBusy = false;
    });
  }
}
