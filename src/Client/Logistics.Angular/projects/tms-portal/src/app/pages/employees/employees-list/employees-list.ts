import { CurrencyPipe, DatePipe, PercentPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { PermissionGuard } from "@logistics/shared";
import { type SalaryType, salaryTypeOptions } from "@logistics/shared/api/models";
import { Permission } from "@logistics/shared/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
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
