import {Component, OnInit} from '@angular/core';
import {CommonModule, CurrencyPipe, DatePipe, PercentPipe} from '@angular/common';
import {FormGroup, FormControl, Validators, ReactiveFormsModule} from '@angular/forms';
import {ActivatedRoute} from '@angular/router';
import {CardModule} from 'primeng/card';
import {ButtonModule} from 'primeng/button';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {RadioButtonModule} from 'primeng/radiobutton';
import {PaymentMethod, PaymentMethodEnum, SalaryType, SalaryTypeEnum, convertEnumToArray} from '@core/enums';
import {Invoice, Payroll, Tenant} from '@core/models';
import {RegexPatterns} from '@core/helpers';
import {ApiService, TenantService, ToastService} from '@core/services';
import {ValidationSummaryComponent} from '@shared/components';


@Component({
  selector: 'app-make-payment',
  standalone: true,
  templateUrl: './make-payment.component.html',
  styleUrls: ['./make-payment.component.scss'],
  imports: [
    CommonModule,
    CardModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    ValidationSummaryComponent,
    RadioButtonModule,
    CurrencyPipe,
    DatePipe,
    PercentPipe,
    ButtonModule,
  ],
})
export class MakePaymentComponent implements OnInit {
  public paymentMethods = convertEnumToArray(PaymentMethodEnum)
  public title = '';
  public isLoading = false;
  public form: FormGroup<PaymentForm>;
  public tenantData: Tenant | null;
  public payroll?: Payroll;
  public invoice?: Invoice;

  constructor(
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly tenantService: TenantService,
    private readonly route: ActivatedRoute)
  {
    this.form = new FormGroup<PaymentForm>({
      paymentMethod: new FormControl(PaymentMethod.CreditCard, {validators: Validators.required, nonNullable: true}),
      cardholderName: new FormControl(''),
      cardNumber: new FormControl(''),
      cardExpireDate: new FormControl(''),
      cardCvv: new FormControl(''),
      billingAddress: new FormControl(''),
      bankName: new FormControl(''),
      bankAccountNumber: new FormControl(''),
      bankRoutingNumber: new FormControl(''),
    });

    this.tenantData = tenantService.getTenantData();
    this.setConditionalValidators();
  }

  ngOnInit(): void {
    let invoiceId: string | null = null;
    let payrollId: string | null = null;

    this.route.params.subscribe((params) => {
      invoiceId = params['invoiceId'];
      payrollId = params['payrollId'];
    });

    if (invoiceId) {
      this.title = 'Invoice payment';
      this.fetchInvoice(invoiceId);
    }
    else if (payrollId) {
      this.title = 'Payroll payment';
      this.fetchPayroll(payrollId);
    }
  }

  submit() {
    if (!this.form.valid) {
      return;
    }

    
  }

  isShareOfGrossSalary(salaryType: SalaryType): boolean {
    return salaryType === SalaryType.ShareOfGross;
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return SalaryTypeEnum.getDescription(enumValue);
  }

  private setConditionalValidators() {
    const cardholderName = this.form.get('cardholderName');
    const cardNumber = this.form.get('cardNumber');
    const cardExpireDate = this.form.get('cardExpireDate');
    const cardCvv = this.form.get('cardCvv');
    const billingAddress = this.form.get('billingAddress');
    const bankName = this.form.get('bankName');
    const bankAccountNumber = this.form.get('bankAccountNumber');
    const bankRoutingNumber = this.form.get('bankRoutingNumber');

    this.form.get('paymentMethod')?.valueChanges.subscribe((method: PaymentMethod) => {
      if (method === PaymentMethod.CreditCard) {
        cardholderName?.setValidators([Validators.required]);
        cardNumber?.setValidators([Validators.required, Validators.pattern(RegexPatterns.CREDIT_CARD_NUMBER)]);
        cardExpireDate?.setValidators([Validators.required, Validators.pattern(RegexPatterns.CARD_EXPIRATION_DATE)]);
        cardCvv?.setValidators([Validators.required, Validators.pattern(RegexPatterns.CARD_CVV)]);
        bankName?.clearValidators();
        bankAccountNumber?.clearValidators();
        bankRoutingNumber?.clearValidators();
      }
      else if (method === PaymentMethod.BankAccount) {
        bankName?.setValidators([Validators.required]);
        bankAccountNumber?.setValidators([Validators.required]);
        bankRoutingNumber?.setValidators([Validators.required, Validators.pattern(RegexPatterns.ROUTING_NUMBER)]);
        cardholderName?.clearValidators();
        cardNumber?.clearValidators();
        cardExpireDate?.clearValidators();
        cardCvv?.clearValidators();
        billingAddress?.clearValidators();
      }
      else {
        // If 'Cash' or anything else, clear all validators
        cardholderName?.clearValidators();
        cardNumber?.clearValidators();
        cardExpireDate?.clearValidators();
        cardCvv?.clearValidators();
        billingAddress?.clearValidators();
        bankName?.clearValidators();
        bankAccountNumber?.clearValidators();
        bankRoutingNumber?.clearValidators();
      }

      cardholderName?.updateValueAndValidity();
      cardNumber?.updateValueAndValidity();
      cardExpireDate?.updateValueAndValidity();
      cardCvv?.updateValueAndValidity();
      billingAddress?.updateValueAndValidity();
      bankName?.updateValueAndValidity();
      bankAccountNumber?.updateValueAndValidity();
      bankRoutingNumber?.updateValueAndValidity();
    });
  }

  private fetchPayroll(payrollId: string) {
    this.isLoading = true;
    this.apiService.getPayroll(payrollId).subscribe((result) => {
      this.payroll = result.data;
      this.isLoading = false;
    });
  }

  private fetchInvoice(invoiceId: string) {
    this.isLoading = true;
    this.apiService.getInvoice(invoiceId).subscribe((result) => {
      this.invoice = result.data;
      this.isLoading = false;
    });
  }
}

interface PaymentForm {
  paymentMethod: FormControl<PaymentMethod>;
  cardholderName: FormControl<string | null>;
  cardNumber: FormControl<string | null>;
  cardExpireDate: FormControl<string | null>;
  cardCvv: FormControl<string | null>;
  billingAddress: FormControl<string | null>;
  bankName: FormControl<string | null>;
  bankAccountNumber: FormControl<string | null>;
  bankRoutingNumber: FormControl<string | null>;
}
