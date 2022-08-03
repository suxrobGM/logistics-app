import { AfterViewInit, Component, OnInit } from '@angular/core';
import { Employee } from '@shared/models/employee';
import { ApiClientService } from '@shared/services/api-client.service';
import { LazyLoadEvent } from 'primeng/api';

@Component({
  selector: 'app-list-employee',
  templateUrl: './list-employee.component.html',
  styleUrls: ['./list-employee.component.scss']
})
export class ListEmployeeComponent implements OnInit {
  public employees: Employee[];
  public isBusy: boolean;
  public totalRecords: number;
  public first: number

  constructor(private apiService: ApiClientService) {
    this.employees = [];
    this.isBusy = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  public ngOnInit(): void {
    this.isBusy = true;
  }

  public search(event: any) {
    const query = event.target.value;
    //this.isBusy = true;
    
    this.apiService.getEmployees(query, 1).subscribe(result => {
      if (result.success && result.items) {
        this.employees = result.items;
        this.totalRecords = result.itemsCount!;
      }

      //this.isBusy = false;
    });
  }

  public loadEmployees(event: LazyLoadEvent) {
    this.isBusy = true;
    const page = event.first! / event.rows! + 1;
    
    this.apiService.getEmployees(undefined, page).subscribe(result => {
      if (result.success && result.items) {
        this.employees = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.isBusy = false;
    });
  }
}
