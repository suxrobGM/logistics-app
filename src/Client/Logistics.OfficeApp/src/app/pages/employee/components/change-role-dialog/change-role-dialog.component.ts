import { Component, OnInit, input, model, signal, inject } from "@angular/core";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {DialogModule} from "primeng/dialog";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {SelectModule} from "primeng/select";
import {ApiService} from "@/core/api";
import {RemoveEmployeeRoleCommand, RoleDto, UpdateEmployeeCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {UserService} from "../../services";

@Component({
  selector: "app-change-role-dialog",
  templateUrl: "./change-role-dialog.component.html",
  imports: [
    DialogModule,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    SelectModule,
  ],
  providers: [UserService],
})
export class ChangeRoleDialogComponent implements OnInit {
  private readonly apiService = inject(ApiService);
  private readonly userService = inject(UserService);
  private readonly toastService = inject(ToastService);

  readonly roles = signal<RoleDto[]>([]);
  readonly loading = signal(false);
  readonly userId = input.required<string>();
  readonly currentRoles = input<RoleDto[] | undefined>([]);
  readonly visible = model<boolean>(false);
  readonly form: FormGroup;

  constructor() {
    this.form = new FormGroup({
      role: new FormControl("", Validators.required),
    });
  }

  ngOnInit(): void {
    this.fetchRoles();
  }

  submit() {
    const role = this.form.value.role;

    if (role === "") {
      this.toastService.showError("Select a role from the list");
      return;
    }

    const updateEmployee: UpdateEmployeeCommand = {
      userId: this.userId(),
      role: role,
    };

    this.loading.set(true);
    this.apiService.updateEmployee(updateEmployee).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess(`Successfully changed employee's role`);
      }

      this.loading.set(false);
    });
  }

  close() {
    this.visible.set(false);
    //this.visibleChange.emit(false);
    this.clearSelctedRole();
  }

  clearSelctedRole() {
    this.form.patchValue({
      role: {name: "", displayName: " "},
    });
  }

  removeRoles() {
    this.currentRoles()?.forEach((role) => {
      this.removeRole(role.name);
    });
  }

  private removeRole(roleName: string) {
    const removeRole: RemoveEmployeeRoleCommand = {
      userId: this.userId(),
      role: roleName,
    };

    this.loading.set(true);
    this.apiService.removeRoleFromEmployee(removeRole).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess(`Removed ${roleName} role from the employee`);
      }

      this.loading.set(false);
    });
  }

  private fetchRoles() {
    this.userService.fetchRoles().subscribe((roles) => {
      if (roles) {
        this.roles.set(roles);
      }
    });
  }
}
