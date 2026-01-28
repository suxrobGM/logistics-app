import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import type { DocumentType } from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TabsModule } from "primeng/tabs";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DocumentManager, PageHeader } from "@/shared/components";
import { EmployeeStatusTag } from "@/shared/components/tags";
import { EmployeeAvatar } from "../components";
import { EmployeeEditDialog } from "../components/employee-edit-dialog/employee-edit-dialog";
import { EmployeeLoadsList } from "../components/employee-loads-list/employee-loads-list";
import { EmployeeDetailsStore } from "../store";

@Component({
  selector: "app-employee-details",
  templateUrl: "./employee-details.html",
  providers: [EmployeeDetailsStore, CurrencyPipe],
  imports: [
    CommonModule,
    ButtonModule,
    CardModule,
    TooltipModule,
    TabsModule,
    TagModule,
    DividerModule,
    ProgressSpinnerModule,
    RouterLink,
    DatePipe,
    PageHeader,
    EmployeeStatusTag,
    EmployeeAvatar,
    DocumentManager,
    EmployeeEditDialog,
    EmployeeLoadsList,
  ],
})
export class EmployeeDetails implements OnInit {
  protected readonly store = inject(EmployeeDetailsStore);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly currencyPipe = inject(CurrencyPipe);

  protected readonly id = input<string>();
  protected readonly activeTab = signal(0);

  protected readonly employeeDocTypes: DocumentType[] = [
    "driver_license",
    "insurance_certificate",
    "identity_document",
    "other",
  ];

  protected readonly salaryDisplay = computed(() => {
    const emp = this.store.employee();
    if (!emp) return "";

    const salary = emp.salary ?? 0;
    const type = emp.salaryType;

    const formatCurrency = (value: number) => this.currencyPipe.transform(value, "USD", "symbol", "1.2-2") ?? "";

    switch (type) {
      case "share_of_gross":
        return `${(salary * 100).toFixed(1)}%`;
      case "hourly":
        return formatCurrency(salary) + "/hr";
      case "rate_per_distance":
        return formatCurrency(salary) + "/mi";
      case "monthly":
      case "weekly":
        return formatCurrency(salary);
      default:
        return "N/A";
    }
  });

  ngOnInit(): void {
    const employeeId = this.id();
    if (employeeId) {
      this.store.loadEmployee(employeeId);
    }
  }

  onTabChange(index: unknown): void {
    this.activeTab.set(index as number);
  }

  openEditDialog(): void {
    this.store.openEditDialog();
  }

  onEmployeeSaved(): void {
    this.store.refreshEmployee();
    this.store.closeEditDialog();
    this.toast.showSuccess("Employee updated successfully");
  }

  onDeleteEmployee(): void {
    this.toast.confirm({
      header: "Delete Employee",
      message: "Are you sure you want to remove this employee? This action cannot be undone.",
      acceptLabel: "Delete",
      rejectLabel: "Cancel",
      acceptButtonStyleClass: "p-button-danger",
      accept: async () => {
        const success = await this.store.deleteEmployee();
        if (success) {
          this.toast.showSuccess("Employee deleted successfully");
          this.router.navigate(["/employees"]);
        } else {
          this.toast.showError("Failed to delete employee");
        }
      },
    });
  }
}
