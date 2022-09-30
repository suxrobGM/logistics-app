import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { MessageService } from 'primeng/api';
import { RemoveEmployeeRole, Role, UpdateEmployee, UserIdentity } from '@shared/models';
import { ApiService } from '@shared/services';
import { UserRole } from '@shared/types';

@Component({
  selector: 'change-role-dialog',
  templateUrl: './change-role-dialog.component.html',
  styleUrls: ['./change-role-dialog.component.scss']
})
export class ChangeRoleDialogComponent implements OnInit {
  private userRoles: string | string[];
  public roles: Role[];
  public form: FormGroup;
  public loading: boolean;

  @Input() userId: string;
  @Input() currentRoles?: Role[];
  @Input() visible: boolean;
  @Output() visibleChange: EventEmitter<boolean>;
  
  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private oidcService: OidcSecurityService) 
  {
    this.currentRoles = [];
    this.roles = [];
    this.userRoles = [];
    this.visible = false;
    this.loading = false;
    this.userId = '';
    this.visibleChange = new EventEmitter<boolean>();

    this.form = new FormGroup({
      role: new FormControl('', Validators.required),
    });
  }

  public ngOnInit(): void {
    this.fetchRoles();
    this.oidcService.getUserData().subscribe((userData: UserIdentity) => this.userRoles = userData.role!);
  }

  public submit() {
    const role = this.form.value.role;

    if (role === '') {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select a role from the list'});
      return;
    }
    
    const updateEmployee: UpdateEmployee = {
      id: this.userId,
      role: role
    }

    this.loading = true;
    this.apiService.updateEmployee(updateEmployee).subscribe(result => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: "Successfully changed employee's role"});
      }

      this.loading = false;
    });
  }

  public close() {
    this.visible = false;
    this.visibleChange.emit(false);
    this.clearSelctedRole();
  }

  public clearSelctedRole() {
    this.form.patchValue({
      role: {name: '', displayName: ' '}
    });
  }

  public removeRoles() {
    this.currentRoles?.forEach(role => {
      this.removeRole(role.name);
    })
  }

  private removeRole(roleName: string) {
    const removeRole: RemoveEmployeeRole = {
      userId: this.userId,
      role: roleName
    }

    this.loading = true;
    this.apiService.removeRoleFromEmployee(removeRole).subscribe(result => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: `Removed ${roleName} role from employee`});
      }

      this.loading = false;
    });
  }

  private fetchRoles() {
    this.apiService.getRoles().subscribe(result => {
      if (result.success && result.items) {
        const roles = result.items;
        const roleNames = roles.map(i => i.name);
        
        if (this.userRoles.includes(UserRole.Owner)) {
          roles.splice(roleNames.indexOf(UserRole.Owner), 1);
        }
        else if (this.userRoles.includes(UserRole.Manager)) {
          roles.splice(roleNames.indexOf(UserRole.Owner), 1);
          roles.splice(roleNames.indexOf(UserRole.Manager), 1);
        }
        
        this.roles.push({name: '', displayName: ' '});
        this.roles.push(...roles);
      }
    });
  }
}
