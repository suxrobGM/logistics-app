import { Component, computed, inject, input, model, output, signal } from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import {
  AddressForm,
  LabeledField,
  StripeCard,
  ValidationSummary,
} from "@logistics/shared/components";
import type { StripeCardNumberElement } from "@stripe/stripe-js";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { DividerModule } from "primeng/divider";
import { InputMaskModule } from "primeng/inputmask";
import { InputTextModule } from "primeng/inputtext";
import { KeyFilterModule } from "primeng/keyfilter";
import { SelectModule } from "primeng/select";
import { Api, createPaymentMethod } from "@/core/api";
import {
  type AddressDto,
  type CreatePaymentMethodCommand,
  type PaymentMethodType,
  type UsBankAccountHolderType,
  type UsBankAccountType,
  paymentMethodTypeOptions,
  usBankAccountHolderTypeOptions,
  usBankAccountTypeOptions,
} from "@/core/api/models";
import { StripeService, TenantService, ToastService } from "@/core/services";

const enabledPaymentTypes: PaymentMethodType[] = [
  "card",
  "us_bank_account",
  //"international_bank_account",
];

@Component({
  selector: "app-payment-method-dialog",
  templateUrl: "./payment-method-dialog.html",
  imports: [
    CardModule,
    ButtonModule,
    DialogModule,
    FormsModule,
    ReactiveFormsModule,
    ValidationSummary,
    AddressForm,
    SelectModule,
    InputMaskModule,
    InputTextModule,
    KeyFilterModule,
    StripeCard,
    LabeledField,
    DividerModule,
  ],
})
export class PaymentMethodDialogComponent {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);
  private readonly stripeService = inject(StripeService);
  private readonly toastService = inject(ToastService);

  public readonly showDialog = model(false);
  public readonly availablePaymentMethods = input<PaymentMethodType[]>(enabledPaymentTypes);
  public readonly paymentMethodAdded = output<void>();
  protected readonly isLoading = signal(false);

  protected readonly paymentMethodTypes = computed(() =>
    this.availablePaymentMethods()
      .map((type) => paymentMethodTypeOptions.find((option) => option.value === type))
      .filter(Boolean),
  );

  protected readonly form: FormGroup<PaymentMethodForm>;
  protected readonly usBankAccountHolderTypes = usBankAccountHolderTypeOptions;
  protected readonly usBankAccountTypes = usBankAccountTypeOptions;

  private stripeCardNumberElement: StripeCardNumberElement | null = null;

  constructor() {
    const companyAddress = this.tenantService.getTenantData()?.companyAddress;

    this.form = new FormGroup<PaymentMethodForm>({
      methodType: new FormControl("card", {
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

  async submit(): Promise<void> {
    if (this.form.invalid) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    if (formValue.methodType === "card") {
      await this.addCard();
    } else if (formValue.methodType === "us_bank_account") {
      await this.addUsBankAccount();
    }

    this.showDialog.set(false);
  }

  async addCard(): Promise<void> {
    if (!this.stripeCardNumberElement) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    const result = await this.stripeService.confirmCardSetup(
      this.stripeCardNumberElement,
      formValue.cardHolderName!,
      formValue.billingAddress!,
    );

    if (result.error) {
      this.toastService.showError("Failed to add card.");
      console.error(result.error);
    } else {
      console.log("SetupIntent succeeded:", result.setupIntent);
      this.toastService.showSuccess("Card added successfully.");

      // Wait a little bit to handle backend the stripe webhook
      setTimeout(() => {
        this.paymentMethodAdded.emit();
      }, 1000);
    }

    this.isLoading.set(false);
  }

  async addUsBankAccount(): Promise<void> {
    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    const result = await this.stripeService.confirmUsBankSetup(
      {
        accountHolderName: formValue.bankAccountHolderName!,
        accountNumber: formValue.bankAccountNumber!,
        routingNumber: formValue.bankRoutingNumber!,
        accountHolderType: formValue.bankAccountHolderType!,
        accountType: formValue.bankAccountType!,
        bankName: formValue.bankName!,
      },
      formValue.billingAddress!,
    );

    if (result.error) {
      this.toastService.showError("Failed to add US bank account.");
      console.error(result.error);
      this.isLoading.set(false);
      return;
    }

    console.log("SetupIntent succeeded:", result.setupIntent);
    const billingAddress = formValue.billingAddress!;
    billingAddress.country = "US"; // Stripe requires US country for US bank accounts

    // Add US Bank account to the backend and wait for verification
    const payload: CreatePaymentMethodCommand = {
      type: "us_bank_account",
      billingAddress: billingAddress,
      accountHolderName: formValue.bankAccountHolderName!,
      accountNumber: `********${formValue.bankAccountNumber?.slice(-4)}`, // Masked for security
      routingNumber: formValue.bankRoutingNumber!,
      accountHolderType: formValue.bankAccountHolderType!,
      accountType: formValue.bankAccountType!,
      bankName: formValue.bankName!,
      stripePaymentMethodId: result.setupIntent.payment_method?.toString(),
      verificationStatus:
        result.setupIntent.next_action?.type === "verify_with_microdeposits"
          ? "pending"
          : "unverified",
      verificationUrl:
        result.setupIntent.next_action?.verify_with_microdeposits?.hosted_verification_url,
    };

    await this.api.invoke(createPaymentMethod, { body: payload });
    this.toastService.showSuccess("US Bank account added successfully. Now you need to verify it.");
    this.paymentMethodAdded.emit();

    this.isLoading.set(false);
  }

  setCardNumberElement(element: StripeCardNumberElement): void {
    this.stripeCardNumberElement = element;
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
