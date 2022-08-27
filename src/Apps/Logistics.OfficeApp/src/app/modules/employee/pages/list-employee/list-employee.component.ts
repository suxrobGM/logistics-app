import { Component, OnInit } from '@angular/core';
import { Employee } from '@shared/models';
import { ApiService } from '@shared/services';
import { LazyLoadEvent, SortEvent } from 'primeng/api';

@Component({
  selector: 'app-list-employee',
  templateUrl: './list-employee.component.html',
  styleUrls: ['./list-employee.component.scss']
})
export class ListEmployeeComponent implements OnInit {
  public employees: Employee[];
  public isBusy: boolean;
  public totalRecords: number;
  public first: number;

  public columns: string[];

  constructor(private apiService: ApiService) {
    this.employees = [];
    this.columns = [
      'userName',
      'firstName',
      'lastName',
      'email',
      'roles',
      'joinedDate'
    ];
    this.isBusy = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  public ngOnInit(): void {
    this.isBusy = true;
  }

  public search(event: any) {
    const query = event.target.value;
    
    this.apiService.getEmployees(query, '', 1).subscribe(result => {
      if (result.success && result.items) {
        this.employees = result.items;
        this.totalRecords = result.itemsCount!;
      }
    });
  }

  public load(event: LazyLoadEvent) {
    this.isBusy = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField, event.sortOrder);
    
    this.apiService.getEmployees('', sortField, page).subscribe(result => {
      if (result.success && result.items) {        
        this.employees = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.isBusy = false;
    });
  }

  
}
