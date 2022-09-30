import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ConfirmationService } from 'primeng/api';
import { AppConfig } from '@configs';
import { Employee, Role } from '@shared/models';
import { ApiService, StorageService } from '@shared/services';
import { UserRole } from '@shared/types';

@Component({
  selector: 'app-edit-employee',
  templateUrl: './edit-employee.component.html',
  styleUrls: ['./edit-employee.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class EditEmployeeComponent implements OnInit {
  private userRoles?: string[];

  public id!: string;
  public roles: Role[];
  public loading: boolean;
  public showUpdateDialog: boolean;
  public employee?: Employee;
  public canChangeRole: boolean;
  
  constructor(
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private route: ActivatedRoute,
    storage: StorageService) 
  {
    this.roles = [];
    this.loading = false;
    this.showUpdateDialog = false;
    this.canChangeRole = false;

    this.userRoles = storage.get<Role[]>(AppConfig.storage.keys.userRoles)?.map(i => i.name);
  }

  public ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.id = params['id'];
    });

    this.fetchEmployee();
  }

  public confirmToDelete() {
    this.confirmationService.confirm({
      message: 'Are you sure that you want to delete this employee from the company?',
      //accept: () => this.deleteLoad()
    });
  }

  public getEmployeeRoleNames(): string {
    const roleNames = this.employee?.roles?.map(i => i.displayName).join(',');
    return roleNames ? roleNames : '';
  }

  public openUpdateDialog() {
    this.showUpdateDialog = true;
  }

  private fetchEmployee() {
    this.loading = true;

    this.apiService.getEmployee(this.id!).subscribe(result => {
      if (result.success && result.value) {
        const employee = result.value;
        this.employee = employee;
        
        if (this.userRoles && employee.roles && employee.roles.length > 0) {      
          const employeeRole = employee.roles[0].name;
          this.canChangeRole = !this.userRoles.includes(employeeRole) || this.userRoles.includes(UserRole.AppAdmin);
        }
      }

      this.loading = false;
    });
  }
}
