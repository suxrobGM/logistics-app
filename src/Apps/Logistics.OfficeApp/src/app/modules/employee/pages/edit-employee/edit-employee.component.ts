import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Employee, Role, UserIdentity } from '@shared/models';
import { ApiService } from '@shared/services';
import { UserRole } from '@shared/types';

@Component({
  selector: 'app-edit-employee',
  templateUrl: './edit-employee.component.html',
  styleUrls: ['./edit-employee.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class EditEmployeeComponent implements OnInit {
  private userRoles: string | string[];

  public id!: string;
  public roles: Role[];
  public loading: boolean;
  public showUpdateDialog: boolean;
  public employee?: Employee;
  public canChangeRole: boolean;
  
  constructor(
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private oidcService: OidcSecurityService,
    private route: ActivatedRoute) 
  {
    this.roles = [];
    this.userRoles = [];
    this.loading = false;
    this.showUpdateDialog = false;
    this.canChangeRole = false;
  }

  public ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.id = params['id'];
    });

    this.fetchEmployee();
    this.oidcService.getUserData().subscribe((userData: UserIdentity) => this.userRoles = userData.role!);
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
        
        if (employee.roles && employee.roles.length > 0) {      
          const employeeRole = employee.roles[0].name;
          this.canChangeRole = !this.userRoles.includes(employeeRole) || this.userRoles.includes(UserRole.AppAdmin);
        }
      }

      this.loading = false;
    });
  }
}
