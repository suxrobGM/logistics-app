import {CommonModule} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {RouterLink} from "@angular/router";
import {ConfirmationService} from "primeng/api";
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
  selector: "app-list-customers",
  templateUrl: "./list-customers.html",
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
export class ListCustomersComponent {
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);
  private readonly confirmationService = inject(ConfirmationService);

  protected readonly customers = signal<CustomerDto[]>([]);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly totalRecords = signal<number>(0);
  protected readonly first = signal<number>(0);

  search(event: Event): void {
    this.isLoading.set(true);
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getCustomers({search: searchValue}).subscribe((result) => {
      if (result.success && result.data) {
        this.customers.set(result.data);
        this.totalRecords.set(result.totalItems);
      }

      this.isLoading.set(false);
    });
  }

  load(event: TableLazyLoadEvent): void {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService
      .getCustomers({orderBy: sortField, page: page, pageSize: rows})
      .subscribe((result) => {
        if (result.success && result.data) {
          this.customers.set(result.data);
          this.totalRecords.set(result.totalItems);
        }

        this.isLoading.set(false);
      });
  }

  confirmToDelete(id: string): void {
    this.confirmationService.confirm({
      message: "Are you sure that you want to delete this customer?",
      accept: () => this.deleteCustomer(id),
    });
  }

  deleteCustomer(id: string): void {
    this.isLoading.set(true);

    this.apiService.deleteCustomer(id).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("The customer has been deleted successfully");
        this.customers.update((customers) => customers.filter((c) => c.id !== id));
      }

      this.isLoading.set(false);
    });
  }
}
