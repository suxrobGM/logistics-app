import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getCustomers$Json, deleteCustomer$Json } from "@/core/api";
import type { CustomerDto } from "@/core/api/models";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-customers-list",
  templateUrl: "./customers-list.html",
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    InputTextModule,
    ConfirmDialogModule,
    TooltipModule,
    IconFieldModule,
    InputIconModule,
  ],
})
export class CustomersListComponent {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly customers = signal<CustomerDto[]>([]);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly totalRecords = signal<number>(0);
  protected readonly first = signal<number>(0);

  protected async search(event: Event): Promise<void> {
    this.isLoading.set(true);
    const searchValue = (event.target as HTMLInputElement).value;

    const result = await this.api.invoke(getCustomers$Json, { Search: searchValue });
    if (result.success && result.data) {
      this.customers.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoading.set(false);
  }

  protected async load(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = formatSortField(event.sortField as string, event.sortOrder);

    const result = await this.api.invoke(getCustomers$Json, {
      OrderBy: sortField,
      Page: page,
      PageSize: rows,
    });
    if (result.success && result.data) {
      this.customers.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoading.set(false);
  }

  protected confirmToDelete(id: string): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this customer?",
      accept: () => this.deleteCustomer(id),
    });
  }

  protected async deleteCustomer(id: string): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(deleteCustomer$Json, { id });
    if (result.success) {
      this.toastService.showSuccess("The customer has been deleted successfully");
      this.customers.update((customers) => customers.filter((c) => c.id !== id));
    }

    this.isLoading.set(false);
  }
}
