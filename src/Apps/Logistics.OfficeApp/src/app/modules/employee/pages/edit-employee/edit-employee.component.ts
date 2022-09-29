import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Employee, Role, User } from '@shared/models';
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
  public form: FormGroup;
  public roles: Role[];
  public loading: boolean;
  
  constructor(
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private oidcService: OidcSecurityService,
    private route: ActivatedRoute) 
  {
    this.roles = [];
    this.userRoles = [];
    this.loading = false;

    this.form = new FormGroup({
      userName: new FormControl({value: '', disabled: true}, Validators.required),
      firstName: new FormControl({value: '', disabled: true}),
      lastName: new FormControl({value: '', disabled: true}),
      role: new FormControl({value: '', disabled: true}),
    });
  }

  public ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.id = params['id'];
    });

    this.fetchEmployee();
    this.oidcService.getUserData().subscribe((userData: User) => this.userRoles = userData.role!);
  }

  public submit() {
    const employee: Employee = {
      id: this.id,
      role: this.form.value.role
    }

    this.loading = true;
    this.apiService.updateEmployee(employee).subscribe(result => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'User has been updated successfully'});
      }

      this.loading = false;
    });
  }

  public confirmToDelete() {
    this.confirmationService.confirm({
      message: 'Are you sure that you want to delete this employee from the company?',
      //accept: () => this.deleteLoad()
    });
  }

  private fetchEmployee() {
    this.loading = true;

    this.apiService.getEmployee(this.id!).subscribe(result => {
      if (result.success && result.value) {
        const employee = result.value;
        
        this.form.patchValue({
          userName: employee.userName,
          firstName: employee.firstName,
          lastName: employee.lastName
        });
        
        if (employee.roles && employee.roles.length > 0) {      
          const employeeRole = employee.roles[0].name;
          const canChangeRole = !this.userRoles.includes(employeeRole);

          this.form.patchValue({
            role: {value: employeeRole, disabled: true}
          });

          
          this.fetchRoles(employeeRole);
        }
      }

      this.loading = false;
    });
  }

  private fetchRoles(employeeRole: string) {
    this.apiService.getRoles().subscribe(result => {
      if (result.success && result.items) {
        const roles = result.items;
        const roleNames = result.items.map(i => i.name);
        
        this.roles.push(...roles);
      }
    });
  }
}
