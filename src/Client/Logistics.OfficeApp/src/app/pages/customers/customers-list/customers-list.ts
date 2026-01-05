import { Component, inject } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, deleteCustomer$Json } from "@/core/api";
import { ToastService } from "@/core/services";
import { DataContainer } from "@/shared/components";
import { CustomersListStore } from "../store/customers-list.store";

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
    InputTextModule,
    ConfirmDialogModule,
    IconFieldModule,
    InputIconModule,
    DataContainer,
  ],
})
export class CustomersListComponent {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(CustomersListStore);

  protected search(event: Event): void {
    const searchValue = (event.target as HTMLInputElement).value;
    this.store.setSearch(searchValue);
  }

  protected addCustomer(): void {
    this.router.navigate(["/customers/add"]);
  }

  protected confirmToDelete(id: string): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this customer?",
      accept: () => this.deleteCustomer(id),
    });
  }

  private async deleteCustomer(id: string): Promise<void> {
    const result = await this.api.invoke(deleteCustomer$Json, { id });
    if (result.success) {
      this.toastService.showSuccess("The customer has been deleted successfully");
      this.store.removeItem(id);
    }
  }
}
