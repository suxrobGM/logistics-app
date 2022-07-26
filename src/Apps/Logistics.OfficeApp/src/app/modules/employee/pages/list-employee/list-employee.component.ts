import { AfterViewInit, Component, OnInit } from '@angular/core';
import { Employee } from '@shared/models/employee';
import { ApiClientService } from '@shared/services/api-client.service';

@Component({
  selector: 'app-list-employee',
  templateUrl: './list-employee.component.html',
  styleUrls: ['./list-employee.component.scss']
})
export class ListEmployeeComponent implements AfterViewInit {
  public employees: Employee[];
  public isBusy = false;

  constructor(private apiService: ApiClientService) {
    this.employees = [];
  }

  ngAfterViewInit(): void {
    this.apiService.getEmployees().subscribe(result => {
      if (result.success && result.items) {
        this.employees = result.items;
      }
    });
  }
}
