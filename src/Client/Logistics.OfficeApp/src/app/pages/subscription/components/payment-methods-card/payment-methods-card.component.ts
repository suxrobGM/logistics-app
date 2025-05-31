import {CommonModule} from "@angular/common";
import {Component, OnInit, model, signal} from "@angular/core";
import {ConfirmationService} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {DialogModule} from "primeng/dialog";
import {TagModule} from "primeng/tag";
import {ApiService} from "@/core/api";
import {
  DeletePaymentMethodCommand,
  PaymentMethodDto,
  PaymentMethodType,
  SetDefaultPaymentMethodCommand,
} from "@/core/api/models";
import {TenantService, ToastService} from "@/core/services";
import {AddressPipe} from "@/shared/pipes";
import {PaymentMethodDialogComponent} from "../payment-method-dialog/payment-method-dialog.component";

@Component({
  selector: "app-payment-methods-card",
  templateUrl: "./payment-methods-card.component.html",
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    DialogModule,
    AddressPipe,
    TagModule,
    PaymentMethodDialogComponent,
  ],
})
export class PaymentMethodsCardComponent implements OnInit {
  readonly isLoading = signal(false);
  readonly paymentMethods = signal<PaymentMethodDto[]>([]);
  readonly showDialog = model(false);
  readonly selectedPaymentMethod = signal<PaymentMethodDto | null>(null);

  constructor(
    private readonly apiService: ApiService,
    private readonly tenantService: TenantService,
    private readonly confirmationService: ConfirmationService,
    private readonly toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.fetchPaymentMethods();
  }

  getMethodLabel(method: PaymentMethodDto): string {
    switch (method.type) {
      case PaymentMethodType.Card:
        return `Card ending in ${method.cardNumber?.substring(method.cardNumber.length - 4)}`;
      case PaymentMethodType.UsBankAccount:
        return `${method.bankName ?? "US Bank"} - ${method.accountHolderName}`;
      case PaymentMethodType.InternationalBankAccount:
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
    this.confirmationService.confirm({
      message: `Are you sure you want to set '${this.getMethodLabel(method)}' as your default payment method?`,
      accept: () => {
        this.isLoading.set(true);
        const command: SetDefaultPaymentMethodCommand = {
          paymentMethodId: method.id,
        };

        this.apiService.paymentApi.setDefaultPaymentMethod(command).subscribe((result) => {
          if (result.success) {
            this.toastService.showSuccess("Default payment method updated successfully.");
            this.fetchPaymentMethods();
          } else {
            this.toastService.showError("Failed to update default payment method.");
          }

          this.isLoading.set(false);
        });
      },
    });
  }

  deletePaymentMethod(method: PaymentMethodDto): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete '${this.getMethodLabel(method)}'?`,
      accept: () => {
        this.isLoading.set(true);
        const command: DeletePaymentMethodCommand = {
          paymentMethodId: method.id,
        };

        this.apiService.paymentApi.deletePaymentMethod(command).subscribe((result) => {
          if (result.success) {
            this.toastService.showSuccess("Payment method deleted successfully.");
            this.fetchPaymentMethods();
          } else {
            this.toastService.showError("Failed to delete payment method.");
          }

          this.isLoading.set(false);
        });
      },
    });
  }

  fetchPaymentMethods(): void {
    const tenantId = this.tenantService.getTenantId();

    if (!tenantId) {
      return;
    }

    this.isLoading.set(true);

    this.apiService.paymentApi.getPaymentMethods().subscribe((result) => {
      if (result.success) {
        // Move the default payment method to the top of the list
        result.data!.sort((a, b) => (b.isDefault ? 1 : 0) - (a.isDefault ? 1 : 0));
        this.paymentMethods.set(result.data!);
        console.log("Payment methods fetched successfully:", result.data);
      }

      this.isLoading.set(false);
    });
  }
}
