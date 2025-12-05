import { Component, OnInit, inject, input, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ApiService } from "@/core/api";
import { UpdateCustomerCommand } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { CustomerForm, CustomerFormValue } from "@/shared/components";

@Component({
  selector: "app-customer-edit",
  templateUrl: "./customer-edit.html",
  imports: [CardModule, ButtonModule, RouterModule, CustomerForm],
})
export class CustomerEditComponent implements OnInit {
  private readonly apiService = inject(ApiService);
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

  protected updateCustomer(formValue: CustomerFormValue): void {
    this.isLoading.set(true);

    const command: UpdateCustomerCommand = {
      id: this.id()!,
      name: formValue.name!,
    };

    this.apiService.customerApi.updateCustomer(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A customer data has been updated successfully");
      }

      this.isLoading.set(false);
    });
  }

  protected deleteCustomer(): void {
    this.isLoading.set(true);
    this.apiService.customerApi.deleteCustomer(this.id()!).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A customer has been deleted successfully");
        this.router.navigateByUrl("/customers");
      }

      this.isLoading.set(false);
    });
  }

  private fetchCustomer(customerId: string): void {
    this.isLoading.set(true);

    this.apiService.customerApi.getCustomer(customerId).subscribe((result) => {
      if (result.success && result.data) {
        const customer = result.data;
        this.initialData.set({ name: customer.name });
      }

      this.isLoading.set(false);
    });
  }
}
