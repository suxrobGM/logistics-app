import {CommonModule} from "@angular/common";
import {Component, input, model, signal} from "@angular/core";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {DialogModule} from "primeng/dialog";
import {InputMaskModule} from "primeng/inputmask";
import {SelectModule} from "primeng/select";
import {AddressFormComponent, ValidationSummaryComponent} from "@/components";
import {ApiService} from "@/core/api";
import {
  AddressDto,
  CardBrand,
  CardFundingType,
  CreatePaymentMethodCommand,
  PaymentMethodType,
  UpdatePaymentMethodCommand,
} from "@/core/models";
import {TenantService} from "@/core/services";

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
  ],
})
export class PaymentMethodDialogComponent {
  readonly showDialog = model(false);
  readonly isLoading = signal(false);
  readonly paymentMethodId = input<string | null | undefined>(null);
  readonly form: FormGroup<PaymentMethodForm>;

  readonly pymentMethodTypes = [
    {label: "Credit/Debit Card", value: PaymentMethodType.Card},
    {label: "US Bank Account", value: PaymentMethodType.UsBankAccount},
    {label: "International Bank", value: PaymentMethodType.InternationalBankAccount},
  ];
  readonly cardBrands = [
    {label: "American Express", value: CardBrand.AmericanExpress},
    {label: "Discover", value: CardBrand.Discover},
    {label: "MasterCard", value: CardBrand.MasterCard},
    {label: "Visa", value: CardBrand.Visa},
  ];
  readonly cardFundingTypes = [
    {label: "Credit", value: CardFundingType.Credit},
    {label: "Debit", value: CardFundingType.Debit},
    {label: "Prepaid", value: CardFundingType.Prepaid},
  ];

  constructor(
    private readonly apiService: ApiService,
    private readonly tenantService: TenantService
  ) {
    const companyAddress = this.tenantService.getTenantData()?.companyAddress;

    this.form = new FormGroup<PaymentMethodForm>({
      methodType: new FormControl(
        {value: null, disabled: !this.isEditMode()},
        {
          validators: !this.isEditMode() ? Validators.required : null,
          nonNullable: true,
        }
      ),
      cardBrand: new FormControl(null),
      cardFundingType: new FormControl<CardFundingType | null>(null),
      cardHolderName: new FormControl(null),
      cardNumber: new FormControl(""),
      cardExpirationDate: new FormControl(""),
      cardCvv: new FormControl(null),
      billingAddress: new FormControl(companyAddress ?? null, {
        validators: Validators.required,
      }),
      bankName: new FormControl(null),
      bankAccountNumber: new FormControl(null),
      bankRoutingNumber: new FormControl(null),
      bankAccountHolderName: new FormControl(null),
      swiftCode: new FormControl(null),
    });
  }

  isEditMode(): boolean {
    return this.paymentMethodId != null;
  }

  getDialogTitle(): string {
    return this.isEditMode() ? "Edit Payment Method" : "Add Payment Method";
  }

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.isLoading.set(true);

    const formValue = this.form.getRawValue();
    const expMonth = formValue.cardExpirationDate?.split("/")[0];
    const expYear = formValue.cardExpirationDate?.split("/")[1];

    const payload: CreatePaymentMethodCommand | UpdatePaymentMethodCommand = {
      tenantId: this.tenantService.getTenantId()!,
      type: formValue.methodType!,
      cardBrand: formValue.cardBrand!,
      fundingType: formValue.cardFundingType!,
      cardHolderName: formValue.cardHolderName!,
      cardNumber: formValue.cardNumber!,
      expMonth: expMonth ? parseInt(expMonth, 10) : undefined,
      expYear: expYear ? parseInt(expYear, 10) : undefined,
      cvv: formValue.cardCvv!,
      billingAddress: formValue.billingAddress!,
      bankName: formValue.bankName!,
      accountNumber: formValue.bankAccountNumber!,
      routingNumber: formValue.bankRoutingNumber!,
      accountHolderName: formValue.bankAccountHolderName!,
      swiftCode: formValue.swiftCode!,
    };

    if (this.isEditMode()) {
      (payload as UpdatePaymentMethodCommand).id = this.paymentMethodId()!;

      this.apiService.paymentMethodApi
        .updatePaymentMethod(payload as UpdatePaymentMethodCommand)
        .subscribe((result) => {
          if (result.success) {
            this.showDialog.set(false);
          }

          this.isLoading.set(false);
        });
    } else {
      this.apiService.paymentMethodApi
        .createPaymentMethod(payload as CreatePaymentMethodCommand)
        .subscribe((result) => {
          if (result.success) {
            this.showDialog.set(false);
          }

          this.isLoading.set(false);
        });
    }
  }
}

interface PaymentMethodForm {
  methodType: FormControl<PaymentMethodType | null>;
  cardBrand: FormControl<CardBrand | null>;
  cardFundingType: FormControl<CardFundingType | null>;
  cardHolderName: FormControl<string | null>;
  cardNumber: FormControl<string | null>;
  cardExpirationDate: FormControl<string | null>;
  cardCvv: FormControl<string | null>;
  billingAddress: FormControl<AddressDto | null>;
  bankName: FormControl<string | null>;
  bankAccountNumber: FormControl<string | null>;
  bankRoutingNumber: FormControl<string | null>;
  bankAccountHolderName: FormControl<string | null>;
  swiftCode: FormControl<string | null>;
}
