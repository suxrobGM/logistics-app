import {CommonModule} from "@angular/common";
import {Component, computed, input, model, signal, viewChild} from "@angular/core";
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
import {
  AddressDto,
  PaymentMethodType,
  UsBankAccountHolderType,
  UsBankAccountType,
  pymentMethodTypeOptions,
  usBankAccountHolderTypeOptions,
  usBankAccountTypeOptions,
} from "@/core/api/models";
import {StripeService, TenantService} from "@/core/services";

const enabledPaymentTypes = [
  PaymentMethodType.Card,
  PaymentMethodType.UsBankAccount,
  //PaymentMethodType.InternationalBankAccount,
];

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
  readonly availablePaymentMethods = input<PaymentMethodType[]>(enabledPaymentTypes);
  readonly paymentMethodId = input<string | null | undefined>(null);
  readonly stripeCard = viewChild.required<StripeCardComponent>("stripeCard");

  readonly pymentMethodTypes = computed(() =>
    this.availablePaymentMethods()
      .map((type) => pymentMethodTypeOptions.find((option) => option.value === type))
      .filter(Boolean)
  );

  readonly form: FormGroup<PaymentMethodForm>;
  readonly usBankAccountHolderTypes = usBankAccountHolderTypeOptions;
  readonly usBankAccountTypes = usBankAccountTypeOptions;

  private stripeCardNumberElement: StripeCardNumberElement | null = null;

  constructor(
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

  async submit(): Promise<void> {
    if (this.form.invalid || !this.stripeCardNumberElement) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    await this.stripeService.confirmCardSetup(
      this.stripeCardNumberElement,
      formValue.cardHolderName!,
      formValue.billingAddress!
    );

    this.isLoading.set(false);
    this.showDialog.set(false);
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
