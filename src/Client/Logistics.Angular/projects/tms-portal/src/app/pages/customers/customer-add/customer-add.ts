import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, createCustomer } from "@logistics/shared/api";
import type { CreateCustomerCommand } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ToastService } from "@/core/services";
import { CustomerForm, type CustomerFormValue } from "@/shared/components";

@Component({
  selector: "app-customer-add",
  templateUrl: "./customer-add.html",
  imports: [CardModule, ButtonModule, RouterModule, CustomerForm, DividerModule],
})
export class CustomerAddComponent {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly initialData = signal<Partial<CustomerFormValue> | null>(null);

  protected async createCustomer(formValue: CustomerFormValue): Promise<void> {
    this.isLoading.set(true);

    const command: CreateCustomerCommand = {
      name: formValue.name!,
    };

    await this.api.invoke(createCustomer, { body: command });
    this.toastService.showSuccess("A customer has been created successfully");
    this.router.navigateByUrl("/customers");

    this.isLoading.set(false);
  }
}
