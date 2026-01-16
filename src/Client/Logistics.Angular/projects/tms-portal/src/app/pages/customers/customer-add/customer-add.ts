import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { CustomerForm, type CustomerFormValue } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { Api, createCustomer } from "@/core/api";
import type { CreateCustomerCommand } from "@/core/api/models";
import { ToastService } from "@/core/services";

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
