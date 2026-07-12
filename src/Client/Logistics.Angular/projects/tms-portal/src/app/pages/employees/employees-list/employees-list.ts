import { DatePipe, PercentPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Permission, PermissionGuard } from "@logistics/shared";
import type { EmployeeDto, SalaryType } from "@logistics/shared/api";
import { salaryTypeOptions } from "@logistics/shared/api/enums";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import {
  Badge,
  Card,
  Stack,
  UiButton,
  UiDataTable,
  UiMenu,
  UiSortHeader,
  type UiBadgeIntent,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { DataContainer, PageHeader, SearchField } from "@/shared/components";
import { EmployeeStatusTag } from "@/shared/components/tags";
import { EmployeeAvatar, InviteEmployeeDialog } from "../components";
import { EmployeesListStore } from "../store";

@Component({
  selector: "app-employees-list",
  templateUrl: "./employees-list.html",
  providers: [EmployeesListStore],
  imports: [
    Badge,
    Card,
    CurrencyFormatPipe,
    DataContainer,
    DatePipe,
    EmployeeAvatar,
    EmployeeStatusTag,
    InviteEmployeeDialog,
    PageHeader,
    PercentPipe,
    PermissionGuard,
    RouterLink,
    SearchField,
    Stack,
    UiButton,
    UiDataTable,
    UiMenu,
    UiSortHeader,
  ],
})
export class EmployeeList {
  private readonly router = inject(Router);
  protected readonly store = inject(EmployeesListStore);
  protected readonly Permission = Permission;

  protected readonly inviteDialogVisible = signal(false);
  protected readonly selectedRow = signal<EmployeeDto | null>(null);

  protected readonly actionMenuItems: UiMenuItem[] = [
    {
      label: "View details",
      icon: "eye",
      command: () => this.router.navigateByUrl(`/employees/${this.selectedRow()!.id}`),
    },
    {
      label: "View payrolls",
      icon: "file",
      command: () => this.router.navigateByUrl(`/payroll/employee/${this.selectedRow()!.id}`),
    },
    {
      label: "View timesheets",
      icon: "clock",
      command: () => this.router.navigateByUrl(`/timesheets/employee/${this.selectedRow()!.id}`),
    },
  ];

  protected getRoleSeverity(roleName: string | undefined): UiBadgeIntent {
    switch (roleName?.toLowerCase()) {
      case "owner":
        return "warn";
      case "manager":
        return "info";
      case "dispatcher":
        return "secondary";
      case "driver":
        return "success";
      default:
        return "secondary";
    }
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected addEmployee(): void {
    this.router.navigate(["/employees/add"]);
  }

  protected openInviteDialog(): void {
    this.inviteDialogVisible.set(true);
  }

  protected onInvitationSent(): void {
    // Optionally refresh the list or navigate to pending invitations
  }

  protected getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
