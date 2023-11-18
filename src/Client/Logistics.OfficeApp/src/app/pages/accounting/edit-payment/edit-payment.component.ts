/* eslint-disable @typescript-eslint/no-non-null-assertion */
import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {ActivatedRoute, Router, RouterModule} from '@angular/router';
import {ButtonModule} from 'primeng/button';
import {CardModule} from 'primeng/card';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {DropdownModule} from 'primeng/dropdown';
import {CreatePayment, UpdatePayment} from '@core/models';
import {ApiService, ToastService} from '@core/services';
import {
  PaymentMethod,
  PaymentFor,
  PaymentStatus,
  PaymentStatusEnum,
  PaymentMethodEnum,
  PaymentForEnum,
} from '@core/enums';
import {ValidationSummaryComponent} from '@shared/components';


@Component({
  selector: 'app-edit-payment',
  standalone: true,
  templateUrl: './edit-payment.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    RouterModule,
    DropdownModule,
    ValidationSummaryComponent,
  ],
})
export class EditPaymentComponent implements OnInit {
  public readonly paymentStatuses = PaymentStatusEnum.toArray();
  public readonly paymentMethods = PaymentMethodEnum.toArray();
  public readonly paymentForValues = PaymentForEnum.toArray();
  public title: string;
  public id: string | null;
  public isLoading: boolean;
  public form: FormGroup<PaymentForm>;

  constructor(
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly route: ActivatedRoute,
    private readonly router: Router)
  {
    this.title = 'Edit payment';
    this.id = null;
    this.isLoading = false;
  
    this.form = new FormGroup<PaymentForm>({
      comment: new FormControl<string>('', {validators: Validators.required, nonNullable: true}),
      paymentMethod: new FormControl<PaymentMethod>(PaymentMethod.BankAccount, {validators: Validators.required, nonNullable: true}),
      amount: new FormControl<number>(1, {validators: Validators.compose([Validators.required, Validators.min(0.01)]), nonNullable: true}),
      paymentFor: new FormControl<PaymentFor>(PaymentFor.Payroll, {validators: Validators.required, nonNullable: true}),
      paymentStatus: new FormControl<PaymentStatus>(PaymentStatus.Pending, {validators: Validators.required, nonNullable: true})
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params['id'];
    });

    if (this.isEditMode()) {
      this.title = 'Edit payment';
      this.fetchPayment();
    }
    else {
      this.title = 'Add a new payment';
    }
  }

  submit() {
    if (!this.form.valid) {
      return;
    }

    if (this.id) {
      this.updatePayment();
    }
    else {
      this.addPayment();
    }
  }

  isEditMode(): boolean {
    return this.id != null && this.id !== '';
  }

  private fetchPayment() {
    this.isLoading = true;

    this.apiService.getPayment(this.id!).subscribe((result) => {
      if (result.data) {
        const payment = result.data;

        this.form.patchValue({
          paymentMethod: payment.method,
          paymentFor: payment.paymentFor,
          paymentStatus: payment.status,
          amount: payment.amount,
          comment: payment.comment
        });
      }

      this.isLoading = false;
    })
  }

  private addPayment() {
    this.isLoading = true;

    const command: CreatePayment = {
      amount: this.form.value.amount!,
      method: this.form.value.paymentMethod!,
      paymentFor: this.form.value.paymentFor!,
      comment: this.form.value.comment
    }

    this.apiService.createPayment(command).subscribe((result) => {
      if (result.isSuccess) {
        this.toastService.showSuccess('A new payment has been added successfully');
        this.router.navigateByUrl('/accounting/payments');
      }

      this.isLoading = false;
    });
  }

  private updatePayment() {
    this.isLoading = true;

    const commad: UpdatePayment = {
      id: this.id!,
      amount: this.form.value.amount,
      method: this.form.value.paymentMethod,
      paymentFor: this.form.value.paymentFor,
      comment: this.form.value.comment,
      status: this.form.value.paymentStatus
    }

    this.apiService.updatePayment(commad).subscribe((result) => {
      if (result.isSuccess) {
        this.toastService.showSuccess('A payment data has been updated successfully');
        this.router.navigateByUrl('/accounting/payments');
      }

      this.isLoading = false;
    });
  }
}

interface PaymentForm {
  paymentMethod: FormControl<PaymentMethod>;
  amount: FormControl<number>;
  paymentFor: FormControl<PaymentFor>;
  paymentStatus: FormControl<PaymentStatus>;
  comment: FormControl<string>;
}
