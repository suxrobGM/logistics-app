import { DatePipe, DecimalPipe, SlicePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute } from "@angular/router";
import { Permission, PermissionGuard } from "@logistics/shared";
import { Api, deleteTimeEntry, getEmployeeById } from "@logistics/shared/api";
import { timeEntryTypeOptions } from "@logistics/shared/api/enums";
import type { EmployeeDto, TimeEntryDto, TimeEntryType } from "@logistics/shared/api/models";
import { ConfirmationService } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { DatePickerModule } from "primeng/datepicker";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { DataContainer, PageHeader } from "@/shared/components";
import { TimeEntryFormDialog } from "../components/time-entry-form-dialog/time-entry-form-dialog";
import { TimeEntriesListStore } from "../store/time-entries-list.store";

@Component({
  selector: "app-time-entries-list",
  templateUrl: "./time-entries-list.html",
  providers: [TimeEntriesListStore, ConfirmationService],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    TagModule,
    SelectModule,
    DatePickerModule,
    FormsModule,
    DatePipe,
    DecimalPipe,
    SlicePipe,
    DataContainer,
    PageHeader,
    TimeEntryFormDialog,
    PermissionGuard,
    ConfirmDialogModule,
  ],
})
export class TimeEntriesList {
  private readonly api = inject(Api);
  private readonly route = inject(ActivatedRoute);
  private readonly toastService = inject(ToastService);
  private readonly confirmationService = inject(ConfirmationService);

  protected readonly store = inject(TimeEntriesListStore);
  protected readonly Permission = Permission;
  protected readonly typeOptions = timeEntryTypeOptions;

  protected readonly formDialogVisible = signal(false);
  protected readonly selectedTimeEntry = signal<TimeEntryDto | null>(null);
  protected readonly employee = signal<EmployeeDto | null>(null);

  // Filters
  protected readonly filterType = signal<TimeEntryType | null>(null);
  protected readonly filterStartDate = signal<Date | null>(null);
  protected readonly filterEndDate = signal<Date | null>(null);

  private employeeId: string | null = null;

  constructor() {
    // Check if we have an employee ID in the route
    this.route.paramMap.subscribe((params) => {
      this.employeeId = params.get("employeeId");
      if (this.employeeId) {
        this.store.setFilters({ EmployeeId: this.employeeId });
        this.fetchEmployee(this.employeeId);
      }
    });
  }

  protected openAddDialog(): void {
    this.selectedTimeEntry.set(null);
    this.formDialogVisible.set(true);
  }

  protected openEditDialog(entry: TimeEntryDto): void {
    this.selectedTimeEntry.set(entry);
    this.formDialogVisible.set(true);
  }

  protected onSaved(): void {
    this.store.retry();
  }

  protected confirmDelete(entry: TimeEntryDto): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete this time entry for ${entry.employeeName} on ${new Date(entry.date!).toLocaleDateString()}?`,
      header: "Delete Time Entry",
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => this.deleteEntry(entry),
    });
  }

  protected async deleteEntry(entry: TimeEntryDto): Promise<void> {
    try {
      await this.api.invoke(deleteTimeEntry, { id: entry.id! });
      this.toastService.showSuccess("Time entry deleted successfully");
      this.store.retry();
    } catch {
      this.toastService.showError("Failed to delete time entry");
    }
  }

  protected applyTypeFilter(type: TimeEntryType | null): void {
    this.filterType.set(type);
    this.store.setFilters({ Type: type ?? undefined });
  }

  protected applyDateFilter(): void {
    const startDate = this.filterStartDate();
    const endDate = this.filterEndDate();

    this.store.setFilters({
      StartDate: startDate ? startDate.toISOString().split("T")[0] : undefined,
      EndDate: endDate ? endDate.toISOString().split("T")[0] : undefined,
    });
  }

  protected clearFilters(): void {
    this.filterType.set(null);
    this.filterStartDate.set(null);
    this.filterEndDate.set(null);
    this.store.setFilters({
      Type: null,
      StartDate: null,
      EndDate: null,
    });
  }

  protected getTypeLabel(type: string | null | undefined): string {
    if (!type) return "N/A";
    return timeEntryTypeOptions.find((o) => o.value === type)?.label ?? type;
  }

  protected getTypeSeverity(
    type: string | null | undefined,
  ): "success" | "info" | "warn" | "danger" | "secondary" {
    switch (type) {
      case "regular":
        return "info";
      case "overtime":
        return "warn";
      case "double_time":
        return "danger";
      case "paid_time_off":
        return "success";
      case "holiday":
        return "secondary";
      default:
        return "info";
    }
  }

  private async fetchEmployee(employeeId: string): Promise<void> {
    try {
      const result = await this.api.invoke(getEmployeeById, { userId: employeeId });
      this.employee.set(result);
    } catch {
      // Silently fail - employee name will still show in entries
    }
  }

  protected get pageTitle(): string {
    const emp = this.employee();
    return emp ? `Time Entries - ${emp.fullName}` : "Time Entries";
  }

  protected get preselectedEmployeeId(): string | null {
    return this.employeeId;
  }
}
