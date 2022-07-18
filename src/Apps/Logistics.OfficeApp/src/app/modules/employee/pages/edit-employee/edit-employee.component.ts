import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
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
  
  constructor(
    private apiService: ApiClientService,
    private messageService: MessageService,
    private oidcSecurityService: OidcSecurityService) 
  {
    this.roles = [];
    this.form = new FormGroup({
      'userName': new FormControl('', Validators.required),
      'firstName': new FormControl(''),
      'lastName': new FormControl(''),
      'role': new FormControl(EmployeeRole.Guest, Validators.required),
    });
  }

  ngOnInit(): void {
  }

}
