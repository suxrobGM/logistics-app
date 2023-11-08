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
      cardNumber: new FormControl(''),
      cardExpireDate: new FormControl(''),
      cardCvv: new FormControl(''),
      bankAccountNumber: new FormControl(''),
      bankAccountRoutingNumber: new FormControl(''),
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
    const cardNumberControl = this.form.get('cardNumber');
    const cardExpireDateControl = this.form.get('cardExpireDate');
    const cardCvvControl = this.form.get('cardCvv');
    const bankAccountNumberControl = this.form.get('bankAccountNumber');
    const bankAccountRoutingNumberControl = this.form.get('bankAccountRoutingNumber');

    this.form.get('paymentMethod')?.valueChanges.subscribe((method: PaymentMethod) => {
      if (method === PaymentMethod.CreditCard) {
        cardNumberControl?.setValidators([Validators.required, Validators.pattern(RegexPatterns.CREDIT_CARD_NUMBER)]);
        cardExpireDateControl?.setValidators([Validators.required, Validators.pattern(RegexPatterns.CARD_EXPIRATION_DATE)]);
        cardCvvControl?.setValidators([Validators.required, Validators.pattern(RegexPatterns.CARD_CVV)]);
        bankAccountNumberControl?.clearValidators();
        bankAccountRoutingNumberControl?.clearValidators();
      }
      else if (method === PaymentMethod.BankAccount) {
        bankAccountNumberControl?.setValidators([Validators.required]);
        bankAccountRoutingNumberControl?.setValidators([Validators.required, Validators.pattern(RegexPatterns.ROUTING_NUMBER)]);
        cardNumberControl?.clearValidators();
        cardExpireDateControl?.clearValidators();
        cardCvvControl?.clearValidators();
      }
      else {
        // If 'Cash' or anything else, clear all validators
        cardNumberControl?.clearValidators();
        cardExpireDateControl?.clearValidators();
        cardCvvControl?.clearValidators();
        bankAccountNumberControl?.clearValidators();
        bankAccountRoutingNumberControl?.clearValidators();
      }

      cardNumberControl?.updateValueAndValidity();
      cardExpireDateControl?.updateValueAndValidity();
      cardCvvControl?.updateValueAndValidity();
      bankAccountNumberControl?.updateValueAndValidity();
      bankAccountRoutingNumberControl?.updateValueAndValidity();
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
  cardNumber: FormControl<string | null>;
  cardExpireDate: FormControl<string | null>;
  cardCvv: FormControl<string | null>;
  bankAccountNumber: FormControl<string | null>;
  bankAccountRoutingNumber: FormControl<string | null>;
}
