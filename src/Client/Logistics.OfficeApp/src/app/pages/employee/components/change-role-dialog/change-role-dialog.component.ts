import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {NgIf} from '@angular/common';
import {FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MessageService} from 'primeng/api';
import {ButtonModule} from 'primeng/button';
import {DropdownModule} from 'primeng/dropdown';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {DialogModule} from 'primeng/dialog';
import {RemoveEmployeeRole, Role, UpdateEmployee} from '@core/models';
import {ApiService} from '@core/services';
import {UserService} from '../../services';

@Component({
  selector: 'change-role-dialog',
  templateUrl: './change-role-dialog.component.html',
  styleUrls: [],
  standalone: true,
  imports: [
    DialogModule,
    NgIf,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    DropdownModule,
    ButtonModule,
  ],
  providers: [
    UserService,
  ],
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
      userId: this.userId,
      role: role,
    };

    this.loading = true;
    this.apiService.updateEmployee(updateEmployee).subscribe((result) => {
      if (result.isSuccess) {
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
      if (result.isSuccess) {
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
