import {CommonModule} from "@angular/common";
import {Component, OnInit, input, signal} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {Router, RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {SelectModule} from "primeng/select";
import {ValidationSummaryComponent} from "@/components";
import {ApiService} from "@/core/api";
import {
  CreatePaymentCommand,
  PaymentFor,
  PaymentMethodType,
  PaymentStatus,
  UpdatePaymentCommand,
  paymentForOptions,
  paymentMethodTypeOptions,
  paymentStatusOptions,
} from "@/core/api/models";
import {ToastService} from "@/core/services";

@Component({
  selector: "app-edit-payment",
  standalone: true,
  templateUrl: "./edit-payment.component.html",
  styleUrls: [],
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    RouterModule,
    SelectModule,
    ValidationSummaryComponent,
  ],
})
export class EditPaymentComponent implements OnInit {
  readonly title = signal("Edit payment");
  readonly id = input<string>("");
  readonly isLoading = signal(false);
  readonly form: FormGroup<PaymentForm>;
  readonly paymentStatuses = paymentStatusOptions;
  readonly paymentMethods = paymentMethodTypeOptions;
  readonly paymentForValues = paymentForOptions;

  constructor(
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly router: Router
  ) {
    this.form = new FormGroup<PaymentForm>({
      notes: new FormControl<string>("", {validators: Validators.required, nonNullable: true}),
      paymentMethod: new FormControl<PaymentMethodType>(PaymentMethodType.UsBankAccount, {
        validators: Validators.required,
        nonNullable: true,
      }),
      amount: new FormControl<number>(1, {
        validators: Validators.compose([Validators.required, Validators.min(0.01)]),
        nonNullable: true,
      }),
      paymentFor: new FormControl<PaymentFor>(PaymentFor.EmployeePayroll, {
        validators: Validators.required,
        nonNullable: true,
      }),
      paymentStatus: new FormControl<PaymentStatus>(PaymentStatus.Pending, {
        validators: Validators.required,
        nonNullable: true,
      }),
    });
  }

  ngOnInit(): void {
    if (this.isEditMode()) {
      this.title.set("Edit payment");
      this.fetchPayment();
    } else {
      this.title.set("Add a new payment");
    }
  }

  submit() {
    if (!this.form.valid) {
      return;
    }

    if (this.isEditMode()) {
      this.updatePayment();
    } else {
      this.addPayment();
    }
  }

  isEditMode(): boolean {
    return this.id() != null && this.id() !== "";
  }

  private fetchPayment() {
    this.isLoading.set(true);

    this.apiService.getPayment(this.id()!).subscribe((result) => {
      if (result.data) {
        const payment = result.data;

        this.form.patchValue({
          paymentMethod: payment.method,
          paymentFor: payment.paymentFor,
          paymentStatus: payment.status,
          amount: payment.amount,
          notes: payment.notes,
        });
      }

      this.isLoading.set(false);
    });
  }

  private addPayment() {
    this.isLoading.set(true);

    const command: CreatePaymentCommand = {
      amount: this.form.value.amount!,
      method: this.form.value.paymentMethod!,
      paymentFor: this.form.value.paymentFor!,
      notes: this.form.value.notes,
    };

    this.apiService.createPayment(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A new payment has been added successfully");
        this.router.navigateByUrl("/accounting/payments");
      }

      this.isLoading.set(false);
    });
  }

  private updatePayment() {
    this.isLoading.set(true);

    const commad: UpdatePaymentCommand = {
      id: this.id()!,
      amount: this.form.value.amount,
      method: this.form.value.paymentMethod,
      paymentFor: this.form.value.paymentFor,
      notes: this.form.value.notes,
      status: this.form.value.paymentStatus,
    };

    this.apiService.updatePayment(commad).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A payment data has been updated successfully");
        this.router.navigateByUrl("/accounting/payments");
      }

      this.isLoading.set(false);
    });
  }
}

interface PaymentForm {
  paymentMethod: FormControl<PaymentMethodType>;
  amount: FormControl<number>;
  paymentFor: FormControl<PaymentFor>;
  paymentStatus: FormControl<PaymentStatus>;
  notes: FormControl<string>;
}
