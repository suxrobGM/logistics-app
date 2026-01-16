import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { CustomerForm, type CustomerFormValue } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { Api, deleteCustomer, getCustomerById, updateCustomer } from "@/core/api";
import type { UpdateCustomerCommand } from "@/core/api/models";
import { ToastService } from "@/core/services";

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

    await this.api.invoke(updateCustomer, {
      id: this.id()!,
      body: command,
    });
    this.toastService.showSuccess("A customer data has been updated successfully");
    this.isLoading.set(false);
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
