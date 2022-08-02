import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Employee } from '@shared/models/employee';
import { EmployeeRole } from '@shared/models/employee-role';
import { User } from '@shared/models/user';
import { ApiClientService } from '@shared/services/api-client.service';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-edit-employee',
  templateUrl: './edit-employee.component.html',
  styleUrls: ['./edit-employee.component.scss']
})
export class EditEmployeeComponent implements OnInit {
  private employee?: Employee;

  public isBusy = false;
  public form: FormGroup;
  public roles: string[];
  public id?: string;
  
  constructor(
    private apiService: ApiClientService,
    private messageService: MessageService,
    private oidcSecurityService: OidcSecurityService) 
  {
    this.roles = [];
    this.form = new FormGroup({
      'userName': new FormControl({value: '', disabled: true}, Validators.required),
      'firstName': new FormControl({value: '', disabled: true}),
      'lastName': new FormControl({value: '', disabled: true}),
      'role': new FormControl('guest', Validators.required),
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
    this.id = history.state.id;
    
    if (!this.id) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'ID is an empty'});
      return;
    }

    this.isBusy = true;
    this.apiService.getEmployee(this.id).subscribe(result => {
      if (result.success && result.value) {
        this.employee = result.value;
        
        this.form.patchValue({
          userName: this.employee.userName,
          firstName: this.employee.firstName,
          lastName: this.employee.lastName,
          role: this.capitalize(this.employee.role)
        });
      }

      this.isBusy = false;
    });
  }

  public onSubmit() {
    const employee: Employee = {
      id: this.employee?.id,
      externalId: this.employee?.externalId,
      userName: this.employee?.userName,
      role: this.form.value.role.toLowerCase()
    }
    
    this.isBusy = true;
    this.apiService.updateEmployee(employee).subscribe(result => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'User has been updated successfully'});
      }

      this.isBusy = false;
    });
  }

  private capitalize(str?: string): string | undefined {
    if (str) {
      return str.charAt(0).toUpperCase() + str.slice(1);
    }
    
    return str;
  }
}
