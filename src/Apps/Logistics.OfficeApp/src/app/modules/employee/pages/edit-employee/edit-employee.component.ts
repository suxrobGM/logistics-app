import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Employee } from '@shared/models/employee';
import { EmployeeRole } from '@shared/models/employee-role';
import { ApiClientService } from '@shared/services/api-client.service';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-edit-employee',
  templateUrl: './edit-employee.component.html',
  styleUrls: ['./edit-employee.component.scss']
})
export class EditEmployeeComponent implements OnInit {
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
      'role': new FormControl(EmployeeRole.Guest, Validators.required),
    });
  }

  ngOnInit(): void {
    this.id = history.state.id;
    
    if (!this.id) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'ID is an empty'});
      return;
    }

    this.apiService.getEmployee(this.id).subscribe(result => {
      if (result.success && result.value) {
        const employee = result.value;

        this.form.patchValue({
          userName: employee.userName,
          firstName: employee.firstName,
          lastName: employee.lastName,
          role: employee.role
        });
      }
    });
  }

  onSubmit() {
    
  }
}
