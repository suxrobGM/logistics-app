import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';
import {MessageService} from 'primeng/api';
import {RemoveEmployeeRole, Role, UpdateEmployee} from '@shared/models';
import {ApiService} from '@shared/services';
import {UserService} from '../../shared';

@Component({
  selector: 'change-role-dialog',
  templateUrl: './change-role-dialog.component.html',
  styleUrls: ['./change-role-dialog.component.scss'],
})
export class ChangeRoleDialogComponent implements OnInit {
  public roles: Role[];
  public form: FormGroup;
  public loading: boolean;

  @Input() userId: string;
  @Input() currentRoles?: Role[];
  @Input() visible: boolean;
  @Output() visibleChange: EventEmitter<boolean>;

  constructor(
    private apiService: ApiService,
    private userService: UserService,
    private messageService: MessageService)
  {
    this.currentRoles = [];
    this.roles = [];
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
  }

  public submit() {
    const role = this.form.value.role;

    if (role === '') {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select a role from the list'});
      return;
    }

    const updateEmployee: UpdateEmployee = {
      id: this.userId,
      role: role,
    };

    this.loading = true;
    this.apiService.updateEmployee(updateEmployee).subscribe((result) => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'Successfully changed employee\'s role'});
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
      role: {name: '', displayName: ' '},
    });
  }

  public removeRoles() {
    this.currentRoles?.forEach((role) => {
      this.removeRole(role.name);
    });
  }

  private removeRole(roleName: string) {
    const removeRole: RemoveEmployeeRole = {
      userId: this.userId,
      role: roleName,
    };

    this.loading = true;
    this.apiService.removeRoleFromEmployee(removeRole).subscribe((result) => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: `Removed ${roleName} role from employee`});
      }

      this.loading = false;
    });
  }

  private fetchRoles() {
    this.userService.fetchRoles().subscribe((roles) => {
      if (roles) {
        this.roles = roles;
      }
    });
  }
}
