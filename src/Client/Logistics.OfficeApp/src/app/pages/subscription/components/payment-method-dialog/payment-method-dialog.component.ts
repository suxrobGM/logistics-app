import {CommonModule} from "@angular/common";
import {Component, input, model, signal, viewChild} from "@angular/core";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {StripeCardNumberElement} from "@stripe/stripe-js";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {DialogModule} from "primeng/dialog";
import {InputMaskModule} from "primeng/inputmask";
import {InputTextModule} from "primeng/inputtext";
import {KeyFilterModule} from "primeng/keyfilter";
import {SelectModule} from "primeng/select";
import {AddressFormComponent, StripeCardComponent, ValidationSummaryComponent} from "@/components";
import {ApiService} from "@/core/api";
import {
  AddressDto,
  CreatePaymentMethodCommand,
  PaymentMethodType,
  UpdatePaymentMethodCommand,
  UsBankAccountHolderType,
  UsBankAccountType,
  pymentMethodTypeOptions,
  usBankAccountHolderTypeOptions,
  usBankAccountTypeOptions,
} from "@/core/api/models";
import {StripeService, TenantService} from "@/core/services";

@Component({
  selector: "app-payment-method-dialog",
  templateUrl: "./payment-method-dialog.component.html",
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    DialogModule,
    FormsModule,
    ReactiveFormsModule,
    ValidationSummaryComponent,
    AddressFormComponent,
    SelectModule,
    InputMaskModule,
    InputTextModule,
    KeyFilterModule,
    StripeCardComponent,
  ],
})
export class PaymentMethodDialogComponent {
  readonly showDialog = model(false);
  readonly isLoading = signal(false);
  readonly paymentMethodId = input<string | null | undefined>(null);
  readonly stripeCard = viewChild.required<StripeCardComponent>("stripeCard");
  readonly form: FormGroup<PaymentMethodForm>;
  readonly pymentMethodTypes = pymentMethodTypeOptions.filter(
    (i) => i.value === PaymentMethodType.Card
  );
  readonly usBankAccountHolderTypes = usBankAccountHolderTypeOptions;
  readonly usBankAccountTypes = usBankAccountTypeOptions;

  private stripeCardNumberElement: StripeCardNumberElement | null = null;

  constructor(
    private readonly apiService: ApiService,
    private readonly tenantService: TenantService,
    private readonly stripeService: StripeService
  ) {
    const companyAddress = this.tenantService.getTenantData()?.companyAddress;

    this.form = new FormGroup<PaymentMethodForm>({
      methodType: new FormControl(PaymentMethodType.Card, {
        validators: Validators.required,
        nonNullable: true,
      }),
      cardHolderName: new FormControl(null),
      billingAddress: new FormControl(companyAddress ?? null, {
        validators: Validators.required,
      }),
      bankName: new FormControl(null),
      bankAccountNumber: new FormControl(null),
      bankRoutingNumber: new FormControl(null),
      bankAccountHolderName: new FormControl(null),
      bankAccountType: new FormControl<UsBankAccountType | null>(null),
      bankAccountHolderType: new FormControl<UsBankAccountHolderType | null>(null),
      swiftCode: new FormControl(null),
    });
  }

  isEditMode(): boolean {
    return this.paymentMethodId() != null;
  }

  getDialogTitle(): string {
    return this.isEditMode() ? "Edit Payment Method" : "Add Payment Method";
  }

  submit(): void {
    if (this.form.invalid || !this.stripeCardNumberElement) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    const payload: CreatePaymentMethodCommand | UpdatePaymentMethodCommand = {
      type: formValue.methodType!,
      cardHolderName: formValue.cardHolderName!,
      billingAddress: formValue.billingAddress!,
      bankName: formValue.bankName!,
      accountNumber: formValue.bankAccountNumber!,
      routingNumber: formValue.bankRoutingNumber!,
      accountHolderName: formValue.bankAccountHolderName!,
      accountHolderType: formValue.bankAccountHolderType!,
      accountType: formValue.bankAccountType!,
      swiftCode: formValue.swiftCode!,
    };

    console.log("isEditMode", this.isEditMode(), this.paymentMethodId());
    console.log("payload", payload);

    this.stripeService.confirmCardSetup(
      this.stripeCardNumberElement,
      formValue.cardHolderName!,
      formValue.billingAddress!
    );

    // if (this.isEditMode()) {
    //   (payload as UpdatePaymentMethodCommand).id = this.paymentMethodId()!;

    //   this.apiService.paymentApi
    //     .updatePaymentMethod(payload as UpdatePaymentMethodCommand)
    //     .subscribe((result) => {
    //       if (result.success) {
    //         this.showDialog.set(false);
    //       }

    //       this.isLoading.set(false);
    //     });
    // } else {
    //   this.apiService.paymentApi
    //     .createPaymentMethod(payload as CreatePaymentMethodCommand)
    //     .subscribe((result) => {
    //       if (result.success) {
    //         this.showDialog.set(false);
    //       }

    //       this.isLoading.set(false);
    //     });
    // }
  }

  async mountStripeCard(): Promise<void> {
    const {cardNumber} = await this.stripeCard().mountElements();
    this.stripeCardNumberElement = cardNumber;
  }
}

interface PaymentMethodForm {
  methodType: FormControl<PaymentMethodType | null>;
  cardHolderName: FormControl<string | null>;
  billingAddress: FormControl<AddressDto | null>;
  bankName: FormControl<string | null>;
  bankAccountNumber: FormControl<string | null>;
  bankRoutingNumber: FormControl<string | null>;
  bankAccountHolderName: FormControl<string | null>;
  bankAccountType: FormControl<UsBankAccountType | null>;
  bankAccountHolderType: FormControl<UsBankAccountHolderType | null>;
  swiftCode: FormControl<string | null>;
}
