import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormGroup, FormControl, Validators, ReactiveFormsModule} from '@angular/forms';
import {ActivatedRoute} from '@angular/router';
import {CardModule} from 'primeng/card';
import {ButtonModule} from 'primeng/button';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {RadioButtonModule} from 'primeng/radiobutton';
import {InputMaskModule} from 'primeng/inputmask';
import {PaymentMethod, PaymentMethodEnum, PaymentStatus} from '@core/enums';
import {Address, Invoice, Payroll, ProcessPayment} from '@core/models';
import {RegexPatterns} from '@core/helpers';
import {ApiService, ToastService} from '@core/services';
import {AddressFormComponent, ValidationSummaryComponent} from '@shared/components';
import {InvoiceDetailsComponent, PayrollDetailsComponent} from '../components';


@Component({
  selector: 'app-process-payment',
  standalone: true,
  templateUrl: './process-payment.component.html',
  imports: [
    CommonModule,
    CardModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    ValidationSummaryComponent,
    RadioButtonModule,
    ButtonModule,
    InvoiceDetailsComponent,
    PayrollDetailsComponent,
    InputMaskModule,
    AddressFormComponent,
  ],
})
export class ProcessPaymentComponent implements OnInit {
  public paymentMethod = PaymentMethod;
  public paymentMethods = PaymentMethodEnum.toArray();
  public title = '';
  public isLoading = false;
  public isPaymentCompleted = false;
  public form: FormGroup<PaymentForm>;
  public payroll?: Payroll;
  public invoice?: Invoice;

  constructor(
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly route: ActivatedRoute)
  {
    this.form = new FormGroup<PaymentForm>({
      paymentMethod: new FormControl(PaymentMethod.CreditCard, {validators: Validators.required, nonNullable: true}),
      cardholderName: new FormControl(null),
      cardNumber: new FormControl(null),
      cardExpirationDate: new FormControl(null),
      cardCvv: new FormControl(null),
      billingAddress: new FormControl(null, {validators: Validators.required}),
      bankName: new FormControl(null),
      bankAccountNumber: new FormControl(null),
      bankRoutingNumber: new FormControl(null),
    });

    this.form.get('paymentMethod')?.valueChanges.subscribe((method: PaymentMethod) => {
      this.setConditionalValidators(method);
    });

    this.setConditionalValidators(this.form.value.paymentMethod!);
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
    const paymentId = this.payroll?.payment.id ?? this.invoice?.payment.id;
    
    if (!this.form.valid || !paymentId) {
      return;
    }

    this.isLoading = true;
    const command: ProcessPayment = {
      paymentId: paymentId,
      paymentMethod: this.form.value.paymentMethod!,
      billingAddress: this.form.value.billingAddress!,
      cardholderName: this.form.value.cardholderName!,
      cardNumber: this.form.value.cardNumber!,
      cardCvv: this.form.value.cardCvv!,
      cardExpirationDate: this.form.value.cardExpirationDate!,
      bankName: this.form.value.bankName!,
      bankAccountNumber: this.form.value.bankAccountNumber!,
      bankRoutingNumber: this.form.value.bankRoutingNumber!,
    }
    
    this.apiService.processPayment(command).subscribe((result) => {
      if (result.isSuccess) {
        this.toastService.showSuccess('Payment has been processed successfully');
        this.isPaymentCompleted = true;
      }
      else if (result.error) {
        this.toastService.showError(result.error);
      }

      this.isLoading = false;
    });
  }

  private setConditionalValidators(paymentMethod: PaymentMethod) {
    const cardholderName = this.form.get('cardholderName');
    const cardNumber = this.form.get('cardNumber');
    const cardExpireDate = this.form.get('cardExpireDate');
    const cardCvv = this.form.get('cardCvv');
    const bankName = this.form.get('bankName');
    const bankAccountNumber = this.form.get('bankAccountNumber');
    const bankRoutingNumber = this.form.get('bankRoutingNumber');

    if (paymentMethod === PaymentMethod.CreditCard) {
      cardholderName?.setValidators([Validators.required]);
      cardNumber?.setValidators([Validators.required, Validators.pattern(RegexPatterns.CREDIT_CARD_NUMBER)]);
      cardExpireDate?.setValidators([Validators.required, Validators.pattern(RegexPatterns.CARD_EXPIRATION_DATE)]);
      cardCvv?.setValidators([Validators.required, Validators.pattern(RegexPatterns.CARD_CVV)]);
      bankName?.clearValidators();
      bankAccountNumber?.clearValidators();
      bankRoutingNumber?.clearValidators();
      
    }
    else if (paymentMethod === PaymentMethod.BankAccount) {
      bankName?.setValidators([Validators.required]);
      bankAccountNumber?.setValidators([Validators.required]);
      bankRoutingNumber?.setValidators([Validators.required, Validators.pattern(RegexPatterns.ROUTING_NUMBER)]);
      cardholderName?.clearValidators();
      cardNumber?.clearValidators();
      cardExpireDate?.clearValidators();
      cardCvv?.clearValidators();
    }
    else {
      // If 'Cash' or anything else, clear all validators
      cardholderName?.clearValidators();
      cardNumber?.clearValidators();
      cardExpireDate?.clearValidators();
      cardCvv?.clearValidators();
      bankName?.clearValidators();
      bankAccountNumber?.clearValidators();
      bankRoutingNumber?.clearValidators();
    }

    cardholderName?.updateValueAndValidity();
    cardNumber?.updateValueAndValidity();
    cardExpireDate?.updateValueAndValidity();
    cardCvv?.updateValueAndValidity();
    bankName?.updateValueAndValidity();
    bankAccountNumber?.updateValueAndValidity();
    bankRoutingNumber?.updateValueAndValidity();
  }

  private fetchPayroll(payrollId: string) {
    this.isLoading = true;
    this.apiService.getPayroll(payrollId).subscribe((result) => {
      this.payroll = result.data;
      this.isPaymentCompleted = this.payroll?.payment.status === PaymentStatus.Paid;
      this.isLoading = false;
    });
  }

  private fetchInvoice(invoiceId: string) {
    this.isLoading = true;
    this.apiService.getInvoice(invoiceId).subscribe((result) => {
      this.invoice = result.data;
      this.isPaymentCompleted = this.invoice?.payment.status === PaymentStatus.Paid;
      this.isLoading = false;
    });
  }
}

interface PaymentForm {
  paymentMethod: FormControl<PaymentMethod>;
  cardholderName: FormControl<string | null>;
  cardNumber: FormControl<string | null>;
  cardExpirationDate: FormControl<string | null>;
  cardCvv: FormControl<string | null>;
  billingAddress: FormControl<Address | null>;
  bankName: FormControl<string | null>;
  bankAccountNumber: FormControl<string | null>;
  bankRoutingNumber: FormControl<string | null>;
}
