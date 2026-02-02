import { DatePipe } from "@angular/common";
import { Component, effect, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Api, PermissionGuard, deleteCustomer } from "@logistics/shared";
import { Permission } from "@logistics/shared";
import type { CustomerDto } from "@logistics/shared/api";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { CustomerStatusTag } from "@/shared/components/tags";
import { CustomerAvatar, InviteCustomerDialogComponent } from "../components";
import { CustomersListStore } from "../store";

@Component({
  selector: "app-customers-list",
  templateUrl: "./customers-list.html",
  providers: [CustomersListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    MenuModule,
    ConfirmDialogModule,
    DatePipe,
    DataContainer,
    PageHeader,
    SearchInput,
    CustomerAvatar,
    CustomerStatusTag,
    InviteCustomerDialogComponent,
    PermissionGuard,
  ],
})
export class CustomersList {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(CustomersListStore);
  protected readonly Permission = Permission;

  protected readonly inviteDialogVisible = signal(false);
  protected readonly selectedCustomerId = signal<string | undefined>(undefined);
  protected readonly selectedCustomerName = signal<string | undefined>(undefined);
  protected readonly selectedRow = signal<CustomerDto | null>(null);

  constructor() {
    effect(() => {
      const customers = this.store.data();
      console.log("Customers List Updated:", customers);
    });
  }

  protected readonly actionMenuItems: MenuItem[] = [
    {
      label: "View Details",
      icon: "pi pi-eye",
      command: () => this.viewCustomer(this.selectedRow()?.id),
    },
    {
      label: "Invite Portal User",
      icon: "pi pi-send",
      command: () =>
        this.openInviteDialog(this.selectedRow()?.id, this.selectedRow()?.name ?? undefined),
    },
    { separator: true },
    {
      label: "Delete",
      icon: "pi pi-trash",
      command: () => this.confirmToDelete(this.selectedRow()?.id),
    },
  ];

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected addCustomer(): void {
    this.router.navigate(["/customers/add"]);
  }

  protected viewCustomer(id?: string): void {
    if (id) {
      this.router.navigate(["/customers", id]);
    }
  }

  protected openInviteDialog(customerId?: string, customerName?: string): void {
    this.selectedCustomerId.set(customerId);
    this.selectedCustomerName.set(customerName);
    this.inviteDialogVisible.set(true);
  }

  protected onInvitationSent(): void {
    // Optionally refresh or show notification
  }

  protected confirmToDelete(id?: string): void {
    if (!id) {
      return;
    }

    this.toastService.confirmDelete("customer", () => this.deleteCustomer(id));
  }

  private async deleteCustomer(id: string): Promise<void> {
    await this.api.invoke(deleteCustomer, { id });
    this.toastService.showSuccess("The customer has been deleted successfully");
    this.store.removeItem(id);
  }
}
