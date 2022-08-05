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
  private users: User[];
  private selectedUser?: User;

  public suggestedUsers: string[];
  public form: FormGroup;
  public roles: string[];

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private oidcSecurityService: OidcSecurityService) 
  {
    this.users = [];
    this.suggestedUsers = [];
    this.roles = [];

    this.form = new FormGroup({
      'name': new FormControl('', Validators.required),
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
        this.users = result.items;
        this.suggestedUsers = [];

        result.items.forEach(user => {
          let username = `${user.userName} `;

          if (user.firstName) {
            username = `${user.userName} - ${user.firstName}`;
          }
          if (user.lastName) {
            username = `${user.userName} - ${user.firstName} ${user.lastName}`;
          }

          this.suggestedUsers.push(username!);
        });
      }
    });
  }

  public onSelectUser(value: string) {
    const userName = value.substring(0, value.indexOf(' '));
    this.selectedUser = this.users.find(i => userName === i.userName);
  }

  public onSubmit() {
    if (!this.selectedUser) {
      this.messageService.add({key: 'errorMsg', severity: 'error', summary: 'Error', detail: 'Select user from the list'});
      return;
    }
    
    const newEmployee: Employee = {
      userName: this.selectedUser?.userName!,
      externalId: this.selectedUser?.id!,
      firstName: this.selectedUser?.firstName,
      lastName: this.selectedUser?.lastName,
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
