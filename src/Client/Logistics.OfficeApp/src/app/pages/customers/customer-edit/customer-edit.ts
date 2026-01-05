import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { Api, deleteCustomer$Json, getCustomerById$Json, updateCustomer$Json } from "@/core/api";
import type { UpdateCustomerCommand } from "@/core/api/models";
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

  protected async updateCustomer(formValue: CustomerFormValue): Promise<void> {
    this.isLoading.set(true);

    const command: UpdateCustomerCommand = {
      id: this.id()!,
      name: formValue.name!,
    };

    const result = await this.api.invoke(updateCustomer$Json, {
      id: this.id()!,
      body: command,
    });
    if (result.success) {
      this.toastService.showSuccess("A customer data has been updated successfully");
    }

    this.isLoading.set(false);
  }

  protected async deleteCustomer(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(deleteCustomer$Json, { id: this.id()! });
    if (result.success) {
      this.toastService.showSuccess("A customer has been deleted successfully");
      this.router.navigateByUrl("/customers");
    }

    this.isLoading.set(false);
  }

  private async fetchCustomer(customerId: string): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getCustomerById$Json, { id: customerId });
    if (result.success && result.data) {
      const customer = result.data;
      this.initialData.set({ name: customer.name ?? undefined });
    }

    this.isLoading.set(false);
  }
}
