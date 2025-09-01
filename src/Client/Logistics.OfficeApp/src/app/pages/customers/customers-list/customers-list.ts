import {CommonModule} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {RouterLink} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {IconFieldModule} from "primeng/iconfield";
import {InputIconModule} from "primeng/inputicon";
import {InputTextModule} from "primeng/inputtext";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {CustomerDto} from "@/core/api/models";
import {ToastService} from "@/core/services";

@Component({
  selector: "app-customers-list",
  templateUrl: "./customers-list.html",
  imports: [
    CommonModule,
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
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);

  protected readonly customers = signal<CustomerDto[]>([]);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly totalRecords = signal<number>(0);
  protected readonly first = signal<number>(0);

  protected search(event: Event): void {
    this.isLoading.set(true);
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.customerApi.getCustomers({search: searchValue}).subscribe((result) => {
      if (result.success && result.data) {
        this.customers.set(result.data);
        this.totalRecords.set(result.totalItems);
      }

      this.isLoading.set(false);
    });
  }

  protected load(event: TableLazyLoadEvent): void {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.formatSortField(event.sortField as string, event.sortOrder);

    this.apiService.customerApi
      .getCustomers({orderBy: sortField, page: page, pageSize: rows})
      .subscribe((result) => {
        if (result.success && result.data) {
          this.customers.set(result.data);
          this.totalRecords.set(result.totalItems);
        }

        this.isLoading.set(false);
      });
  }

  protected confirmToDelete(id: string): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this customer?",
      accept: () => this.deleteCustomer(id),
    });
  }

  protected deleteCustomer(id: string): void {
    this.isLoading.set(true);

    this.apiService.customerApi.deleteCustomer(id).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("The customer has been deleted successfully");
        this.customers.update((customers) => customers.filter((c) => c.id !== id));
      }

      this.isLoading.set(false);
    });
  }
}
