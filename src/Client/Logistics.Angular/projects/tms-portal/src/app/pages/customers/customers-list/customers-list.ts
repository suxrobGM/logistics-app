import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Api, deleteCustomer } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { CustomersListStore } from "../store/customers-list.store";
import { InviteCustomerDialogComponent } from "../components/invite-customer-dialog/invite-customer-dialog";

@Component({
  selector: "app-customers-list",
  templateUrl: "./customers-list.html",
  providers: [CustomersListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    ConfirmDialogModule,
    DataContainer,
    PageHeader,
    SearchInput,
    InviteCustomerDialogComponent,
  ],
})
export class CustomersListComponent {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(CustomersListStore);

  protected readonly inviteDialogVisible = signal(false);
  protected readonly selectedCustomerId = signal<string | undefined>(undefined);
  protected readonly selectedCustomerName = signal<string | undefined>(undefined);

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected addCustomer(): void {
    this.router.navigate(["/customers/add"]);
  }

  protected openInviteDialog(customerId?: string, customerName?: string): void {
    this.selectedCustomerId.set(customerId);
    this.selectedCustomerName.set(customerName);
    this.inviteDialogVisible.set(true);
  }

  protected onInvitationSent(): void {
    // Optionally refresh or show notification
  }

  protected confirmToDelete(id: string): void {
    this.toastService.confirmDelete("customer", () => this.deleteCustomer(id));
  }

  private async deleteCustomer(id: string): Promise<void> {
    await this.api.invoke(deleteCustomer, { id });
    this.toastService.showSuccess("The customer has been deleted successfully");
    this.store.removeItem(id);
  }
}
