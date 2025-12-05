import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ApiService } from "@/core/api";
import { CreateCustomerCommand } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { CustomerForm, CustomerFormValue } from "@/shared/components";

@Component({
  selector: "app-customer-add",
  templateUrl: "./customer-add.html",
  imports: [CardModule, ButtonModule, RouterModule, CustomerForm],
})
export class CustomerAddComponent {
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly initialData = signal<Partial<CustomerFormValue> | null>(null);

  protected createCustomer(formValue: CustomerFormValue): void {
    this.isLoading.set(true);

    const command: CreateCustomerCommand = {
      name: formValue.name!,
    };

    this.apiService.customerApi.createCustomer(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A customer has been created successfully");
        this.router.navigateByUrl("/customers");
      }

      this.isLoading.set(false);
    });
  }
}
