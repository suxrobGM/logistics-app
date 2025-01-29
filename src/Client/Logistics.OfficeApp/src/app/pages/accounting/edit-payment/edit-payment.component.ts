import {Component, input, OnInit, signal} from "@angular/core";
import {CommonModule} from "@angular/common";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {Router, RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {DropdownModule} from "primeng/dropdown";
import {CreatePaymentCommand, UpdatePaymentCommand} from "@/core/models";
import {ApiService, ToastService} from "@/core/services";
import {
  PaymentMethod,
  PaymentFor,
  PaymentStatus,
  PaymentStatusEnum,
  PaymentMethodEnum,
  PaymentForEnum,
} from "@/core/enums";
import {ValidationSummaryComponent} from "@/components";

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
    DropdownModule,
    ValidationSummaryComponent,
  ],
})
export class EditPaymentComponent implements OnInit {
  public readonly paymentStatuses = PaymentStatusEnum.toArray();
  public readonly paymentMethods = PaymentMethodEnum.toArray();
  public readonly paymentForValues = PaymentForEnum.toArray();
  public title = signal("Edit payment");
  public id = input<string>("");
  public isLoading = signal(false);
  public form: FormGroup<PaymentForm>;

  constructor(
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly router: Router
  ) {
    this.form = new FormGroup<PaymentForm>({
      comment: new FormControl<string>("", {validators: Validators.required, nonNullable: true}),
      paymentMethod: new FormControl<PaymentMethod>(PaymentMethod.BankAccount, {
        validators: Validators.required,
        nonNullable: true,
      }),
      amount: new FormControl<number>(1, {
        validators: Validators.compose([Validators.required, Validators.min(0.01)]),
        nonNullable: true,
      }),
      paymentFor: new FormControl<PaymentFor>(PaymentFor.Payroll, {
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
          comment: payment.comment,
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
      comment: this.form.value.comment,
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
      comment: this.form.value.comment,
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
  paymentMethod: FormControl<PaymentMethod>;
  amount: FormControl<number>;
  paymentFor: FormControl<PaymentFor>;
  paymentStatus: FormControl<PaymentStatus>;
  comment: FormControl<string>;
}
