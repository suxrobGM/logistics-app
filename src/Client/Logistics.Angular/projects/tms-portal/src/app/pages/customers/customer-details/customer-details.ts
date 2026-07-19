import { CommonModule, DatePipe } from "@angular/common";
import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared/services";
import {
  Card,
  DetailSection,
  EmptyState,
  Grid,
  Icon,
  InfoRow,
  Spinner,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiTabsImports,
} from "@logistics/shared/ui";
import { PageHeader } from "@/shared/components";
import { CustomerStatusTag } from "@/shared/components/tags";
import { CustomerAvatar } from "../components";
import { CustomerEditDialog } from "../components/customer-edit-dialog/customer-edit-dialog";
import { CustomerInvoicesList } from "../components/customer-invoices-list/customer-invoices-list";
import { CustomerLoadsList } from "../components/customer-loads-list/customer-loads-list";
import { CustomerDetailsStore } from "../store";

@Component({
  selector: "app-customer-details",
  templateUrl: "./customer-details.html",
  providers: [CustomerDetailsStore],
  imports: [
    Card,
    CommonModule,
    CustomerAvatar,
    CustomerEditDialog,
    CustomerInvoicesList,
    CustomerLoadsList,
    CustomerStatusTag,
    DatePipe,
    DetailSection,
    EmptyState,
    Grid,
    Icon,
    InfoRow,
    PageHeader,
    RouterLink,
    Spinner,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiTabsImports,
  ],
})
export class CustomerDetails implements OnInit {
  protected readonly store = inject(CustomerDetailsStore);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly id = input<string>();
  protected readonly activeTab = signal(0);

  ngOnInit(): void {
    const customerId = this.id();
    if (customerId) {
      this.store.loadCustomer(customerId);
    }
  }

  onTabChange(index: unknown): void {
    this.activeTab.set(index as number);
  }

  openEditDialog(): void {
    this.store.openEditDialog();
  }

  onCustomerSaved(): void {
    this.store.refreshCustomer();
    this.store.closeEditDialog();
    this.toast.showSuccess("Customer updated successfully");
  }

  onDeleteCustomer(): void {
    this.toast.confirm({
      header: "Delete Customer",
      message: "Are you sure you want to delete this customer? This action cannot be undone.",
      acceptLabel: "Delete",
      rejectLabel: "Cancel",
      severity: "danger",
      accept: async () => {
        const success = await this.store.deleteCustomer();
        if (success) {
          this.toast.showSuccess("Customer deleted successfully");
          this.router.navigate(["/customers"]);
        } else {
          this.toast.showError("Failed to delete customer");
        }
      },
    });
  }
}
