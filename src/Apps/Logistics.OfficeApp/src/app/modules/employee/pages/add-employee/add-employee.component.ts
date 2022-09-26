import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { MessageService } from 'primeng/api';
import { Employee, Role, User } from '@shared/models';
import { ApiService } from '@shared/services';

@Component({
  selector: 'app-add-employee',
  templateUrl: './add-employee.component.html',
  styleUrls: ['./add-employee.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AddEmployeeComponent implements OnInit {
  public suggestedUsers: User[];
  public form: FormGroup;
  public roles: Role[];
  public loading: boolean;

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private oidcService: OidcSecurityService) 
  {
    this.suggestedUsers = [];
    this.roles = [];
    this.loading = false;

    this.form = new FormGroup({
      user: new FormControl('', Validators.required),
      role: new FormControl('', Validators.required),
    });

    // let currentUserRole = EmployeeRole.Owner as string;
    // oidcSecurityService.getUserData().subscribe((userData: User) => currentUserRole = userData.role!);
  }

  public ngOnInit(): void {
    this.fetchRoles();
  }

  public searchUser(event: any) {
    this.apiService.getUsers(event.query).subscribe(result => {
      if (result.success && result.items) {
        this.suggestedUsers = result.items;
      }
    });
  }

  public submit() {
    const user = this.form.value.user as User;
    
    if (!user) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select user'});
      return;
    }

    const newEmployee: Employee = {
      id: user.id,
      role: this.form.value.role
    };

    this.loading = true;
    this.apiService.createEmployee(newEmployee).subscribe(result => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'New employee has been added successfully'});
        this.form.reset();
      }

      this.loading = false;
    });
  }

  private fetchRoles() {
    this.apiService.getRoles().subscribe(result => {
      if (result.success && result.items) {
        this.roles.push(...result.items);
      }
    })
  }
}
