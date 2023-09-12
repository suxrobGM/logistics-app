import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {ConfirmationService} from 'primeng/api';
import {Employee} from '@core/models';
import {ApiService, UserDataService} from '@core/services';
import {UserRole} from '@core/types';


@Component({
  selector: 'app-edit-employee',
  templateUrl: './edit-employee.component.html',
  styleUrls: ['./edit-employee.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class EditEmployeeComponent implements OnInit {
  public id!: string;
  public isBusy: boolean;
  public showUpdateDialog: boolean;
  public canChangeRole: boolean;
  public employee?: Employee;

  constructor(
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private route: ActivatedRoute,
    private userDataService: UserDataService)
  {
    this.isBusy = false;
    this.showUpdateDialog = false;
    this.canChangeRole = false;
  }

  public ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params['id'];
    });

    this.fetchEmployee();
  }

  public confirmToDelete() {
    this.confirmationService.confirm({
      message: 'Are you sure that you want to delete this employee from the company?',
      // accept: () => this.deleteLoad()
    });
  }

  public getEmployeeRoleNames(): string {
    const roleNames = this.employee?.roles?.map((i) => i.displayName).join(',');
    return roleNames ? roleNames : '';
  }

  public openUpdateDialog() {
    this.showUpdateDialog = true;
  }

  private fetchEmployee() {
    this.isBusy = true;

    this.apiService.getEmployee(this.id!).subscribe((result) => {
      if (result.success && result.value) {
        this.employee = result.value;
        const employeeRoles = this.employee.roles?.map((i) => i.name);
        const user = this.userDataService.getUser();
        this.evaluateCanChangeRole(user?.roles, employeeRoles);
      }

      this.isBusy = false;
    });
  }

  private evaluateCanChangeRole(userRoles?: string[], employeeRoles?: string[]) {
    if (!userRoles) {
      this.canChangeRole = false;
      return;
    }

    if (!employeeRoles || employeeRoles.length < 1) {
      this.canChangeRole = true;
      return;
    }

    const employeeRole = employeeRoles[0];

    if (userRoles.includes(UserRole.AppSuperAdmin) || userRoles.includes(UserRole.AppAdmin)) {
      this.canChangeRole = true;
    }
    else if (userRoles.includes(UserRole.Owner) && employeeRole !== UserRole.Owner) {
      this.canChangeRole = true;
    }
    else if (userRoles.includes(UserRole.Manager) && employeeRole !== UserRole.Owner && employeeRole !== UserRole.Manager) {
      this.canChangeRole = true;
    }
  }
}
