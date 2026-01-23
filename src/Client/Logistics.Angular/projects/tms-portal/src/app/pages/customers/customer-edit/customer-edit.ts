import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, deleteCustomer, getCustomerById } from "@logistics/shared/api";
import type { CustomerDto } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ToastService } from "@/core/services";
import { CustomerForm, type CustomerFormValue } from "@/shared/components";

@Component({
  selector: "app-customer-edit",
  templateUrl: "./customer-edit.html",
  imports: [CardModule, ButtonModule, RouterModule, CustomerForm],
})
export class CustomerEditComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly id = input<string>();
  protected readonly isLoading = signal<boolean>(false);
  protected readonly initialData = signal<Partial<CustomerFormValue> | null>(null);

  ngOnInit(): void {
    const customerId = this.id();
    if (customerId) {
      this.fetchCustomer(customerId);
    }
  }

  protected handleCustomerUpdated(_customer: CustomerDto): void {
    // Customer form handles the API call and toast, nothing else needed here
  }

  protected async deleteCustomer(): Promise<void> {
    this.isLoading.set(true);

    await this.api.invoke(deleteCustomer, { id: this.id()! });
    this.toastService.showSuccess("A customer has been deleted successfully");
    this.router.navigateByUrl("/customers");
    this.isLoading.set(false);
  }

  private async fetchCustomer(customerId: string): Promise<void> {
    this.isLoading.set(true);

    const customer = await this.api.invoke(getCustomerById, { id: customerId });
    if (customer) {
      this.initialData.set({ name: customer.name ?? undefined });
    }

    this.isLoading.set(false);
  }
}
