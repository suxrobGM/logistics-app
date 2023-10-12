import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {NgIf} from '@angular/common';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ConfirmationService} from 'primeng/api';
import {ButtonModule} from 'primeng/button';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {CardModule} from 'primeng/card';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ToastModule} from 'primeng/toast';
import {Employee, UserRoles} from '@core/models';
import {ApiService} from '@core/services';
import {AuthService} from '@core/auth';
import {ChangeRoleDialogComponent} from '../components';


@Component({
  selector: 'app-edit-employee',
  templateUrl: './edit-employee.component.html',
  styleUrls: ['./edit-employee.component.scss'],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    ToastModule,
    ConfirmDialogModule,
    ChangeRoleDialogComponent,
    CardModule,
    NgIf,
    ProgressSpinnerModule,
    ButtonModule,
    RouterLink,
  ],
  providers: [
    ConfirmationService,
  ],
})
export class EditEmployeeComponent implements OnInit {
  public id!: string;
  public isBusy: boolean;
  public showUpdateDialog: boolean;
  public canChangeRole: boolean;
  public employee?: Employee;

  constructor(
    private authService: AuthService,
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private route: ActivatedRoute)
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

    this.apiService.getEmployee(this.id).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.employee = result.data;
        const employeeRoles = this.employee.roles?.map((i) => i.name);
        const user = this.authService.getUserData();
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

    if (userRoles.includes(UserRoles.AppSuperAdmin) || userRoles.includes(UserRoles.AppAdmin)) {
      this.canChangeRole = true;
    }
    else if (userRoles.includes(UserRoles.Owner) && employeeRole !== UserRoles.Owner) {
      this.canChangeRole = true;
    }
    else if (userRoles.includes(UserRoles.Manager) && employeeRole !== UserRoles.Owner && employeeRole !== UserRoles.Manager) {
      this.canChangeRole = true;
    }
  }
}
