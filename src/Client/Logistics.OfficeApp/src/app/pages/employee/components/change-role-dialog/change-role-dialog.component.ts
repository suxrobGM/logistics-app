import {Component, EventEmitter, Input, OnInit, Output} from "@angular/core";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {DialogModule} from "primeng/dialog";
import {DropdownModule} from "primeng/dropdown";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ApiService} from "@/core/api";
import {RemoveEmployeeRoleCommand, RoleDto, UpdateEmployeeCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {UserService} from "../../services";

@Component({
  selector: "app-change-role-dialog",
  templateUrl: "./change-role-dialog.component.html",
  styleUrls: [],
  standalone: true,
  imports: [
    DialogModule,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    DropdownModule,
    ButtonModule,
  ],
  providers: [UserService],
})
export class ChangeRoleDialogComponent implements OnInit {
  public roles: RoleDto[];
  public form: FormGroup;
  public loading: boolean;

  @Input() userId: string;
  @Input() currentRoles?: RoleDto[];
  @Input() visible: boolean;
  @Output() visibleChange: EventEmitter<boolean>;

  constructor(
    private readonly apiService: ApiService,
    private readonly userService: UserService,
    private readonly toastService: ToastService
  ) {
    this.currentRoles = [];
    this.roles = [];
    this.visible = false;
    this.loading = false;
    this.userId = "";
    this.visibleChange = new EventEmitter<boolean>();

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
      userId: this.userId,
      role: role,
    };

    this.loading = true;
    this.apiService.updateEmployee(updateEmployee).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess(`Successfully changed employee's role`);
      }

      this.loading = false;
    });
  }

  close() {
    this.visible = false;
    this.visibleChange.emit(false);
    this.clearSelctedRole();
  }

  clearSelctedRole() {
    this.form.patchValue({
      role: {name: "", displayName: " "},
    });
  }

  removeRoles() {
    this.currentRoles?.forEach((role) => {
      this.removeRole(role.name);
    });
  }

  private removeRole(roleName: string) {
    const removeRole: RemoveEmployeeRoleCommand = {
      userId: this.userId,
      role: roleName,
    };

    this.loading = true;
    this.apiService.removeRoleFromEmployee(removeRole).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess(`Removed ${roleName} role from the employee`);
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
