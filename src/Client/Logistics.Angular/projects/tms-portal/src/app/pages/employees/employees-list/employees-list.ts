import { CurrencyPipe, DatePipe, PercentPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Permission, PermissionGuard } from "@logistics/shared";
import type { EmployeeDto, SalaryType } from "@logistics/shared/api/models";
import { salaryTypeOptions } from "@logistics/shared/api/models";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { InviteEmployeeDialogComponent } from "../components/invite-employee-dialog/invite-employee-dialog";
import { EmployeesListStore } from "../store/employees-list.store";

@Component({
  selector: "app-employees-list",
  templateUrl: "./employees-list.html",
  providers: [EmployeesListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    MenuModule,
    DatePipe,
    PercentPipe,
    CurrencyPipe,
    DataContainer,
    PageHeader,
    SearchInput,
    InviteEmployeeDialogComponent,
    PermissionGuard,
  ],
})
export class EmployeeListComponent {
  private readonly router = inject(Router);
  protected readonly store = inject(EmployeesListStore);
  protected readonly Permission = Permission;

  protected readonly inviteDialogVisible = signal(false);
  protected readonly selectedRow = signal<EmployeeDto | null>(null);

  protected readonly actionMenuItems: MenuItem[] = [
    {
      label: "Edit employee",
      icon: "pi pi-pen-to-square",
      command: () => this.router.navigateByUrl(`/employees/${this.selectedRow()!.id}/edit`),
    },
    {
      label: "View payrolls",
      icon: "pi pi-file-o",
      command: () =>
        this.router.navigateByUrl(`/invoices/payroll/employee/${this.selectedRow()!.id}`),
    },
    {
      label: "Manage documents",
      icon: "pi pi-paperclip",
      command: () => this.router.navigateByUrl(`/employees/${this.selectedRow()!.id}/documents`),
    },
  ];

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
