import { Component, type OnInit, inject, model, signal } from "@angular/core";
import {
  Api,
  deletePaymentMethod,
  getPaymentMethods,
  setDefaultPaymentMethod,
} from "@logistics/shared/api";
import type { PaymentMethodDto, SetDefaultPaymentMethodCommand } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { TagModule } from "primeng/tag";
import { TenantService, ToastService } from "@/core/services";
import { PaymentMethodDialogComponent } from "../payment-method-dialog/payment-method-dialog";

@Component({
  selector: "app-payment-methods-card",
  templateUrl: "./payment-methods-card.html",
  imports: [
    CardModule,
    ButtonModule,
    DialogModule,
    AddressPipe,
    TagModule,
    PaymentMethodDialogComponent,
  ],
})
export class PaymentMethodsCardComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly paymentMethods = signal<PaymentMethodDto[]>([]);
  protected readonly showDialog = model(false);
  protected readonly selectedPaymentMethod = signal<PaymentMethodDto | null>(null);

  ngOnInit(): void {
    this.fetchPaymentMethods();
  }

  getMethodLabel(method: PaymentMethodDto): string {
    switch (method.type) {
      case "card":
        return `Card ending in ${method.cardNumber?.substring(method.cardNumber.length - 4)}`;
      case "us_bank_account":
        return `${method.bankName ?? "US Bank"} - ${method.accountHolderName}`;
      case "international_bank_account":
        return `(International) ${method.bankName ?? "Bank"} - ${method.accountHolderName}`;
      default:
        return "Payment Method";
    }
  }

  updatePaymentMethod(method: PaymentMethodDto): void {
    this.selectedPaymentMethod.set(method);
    this.showDialog.set(true);
  }

  setDefaultPaymentMethod(method: PaymentMethodDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to set '${this.getMethodLabel(method)}' as your default payment method?`,
      accept: async () => {
        this.isLoading.set(true);
        const command: SetDefaultPaymentMethodCommand = {
          paymentMethodId: method.id,
        };

        await this.api.invoke(setDefaultPaymentMethod, {
          body: command,
        });
        this.toastService.showSuccess("Default payment method updated successfully.");
        this.fetchPaymentMethods();

        this.isLoading.set(false);
      },
    });
  }

  deletePaymentMethod(method: PaymentMethodDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to delete '${this.getMethodLabel(method)}'?`,
      accept: async () => {
        this.isLoading.set(true);

        await this.api.invoke(deletePaymentMethod, { id: method.id! });
        this.toastService.showSuccess("Payment method deleted successfully.");
        this.fetchPaymentMethods();

        this.isLoading.set(false);
      },
    });
  }

  async fetchPaymentMethods(): Promise<void> {
    const tenantId = this.tenantService.getTenantId();

    if (!tenantId) {
      return;
    }

    this.isLoading.set(true);

    const result = await this.api.invoke(getPaymentMethods, {});
    if (result) {
      // Move the default payment method to the top of the list
      result.sort((a, b) => (b.isDefault ? 1 : 0) - (a.isDefault ? 1 : 0));
      this.paymentMethods.set(result);
      console.log("Payment methods fetched successfully:", result);
    }

    this.isLoading.set(false);
  }
}
