import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { MessageService } from 'primeng/api';
import { Employee, EmployeeRole, User } from '@shared/models';
import { ApiService } from '@shared/services';

@Component({
  selector: 'app-add-employee',
  templateUrl: './add-employee.component.html',
  styleUrls: ['./add-employee.component.scss']
})
export class AddEmployeeComponent implements OnInit {
  public suggestedUsers: User[];
  public form: FormGroup;
  public roles: string[];

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private oidcSecurityService: OidcSecurityService) 
  {
    this.suggestedUsers = [];
    this.roles = [];

    this.form = new FormGroup({
      'user': new FormControl('', Validators.required),
      'role': new FormControl(EmployeeRole.Guest, Validators.required),
    });

    let currentUserRole = EmployeeRole.Owner as string;
    oidcSecurityService.getUserData().subscribe((userData: User) => currentUserRole = userData.role!);

    for (const role in EmployeeRole) {
      if (currentUserRole !== 'admin' && role === EmployeeRole.Owner) {
        continue;
      }
      else {
        this.roles.push(role);
      }
    }
  }

  public ngOnInit(): void {
  }

  public searchUser(event: any) {
    this.apiService.getUsers(event.query).subscribe(result => {
      if (result.success && result.items) {
        this.suggestedUsers = result.items;
      }
    });
  }

  public onSubmit() {
    const user = this.form.value.user as User;

    if (!user) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select user'});
      return;
    }

    const newEmployee: Employee = {
      id: user.id,
      role: this.form.value.role
    };

    this.apiService.createEmployee(newEmployee).subscribe(result => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'New employee has been added successfully'});
        this.form.reset();
      }
    });
  }
}
