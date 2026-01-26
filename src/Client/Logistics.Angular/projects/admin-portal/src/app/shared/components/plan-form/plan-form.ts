import { Component, effect, inject, input, output } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared";
import type { BillingInterval, TrialPeriod } from "@logistics/shared/api";
import { CurrencyInput, LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";

export interface PlanFormValue {
  name: string;
  description: string;
  price: number;
  interval: BillingInterval;
  intervalCount: number;
  trialPeriod: TrialPeriod;
}

const INTERVAL_OPTIONS = [
  { label: "Day", value: "day" },
  { label: "Week", value: "week" },
  { label: "Month", value: "month" },
  { label: "Year", value: "year" },
];

const TRIAL_PERIOD_OPTIONS = [
  { label: "None", value: "none" },
  { label: "7 Days", value: "seven_days" },
  { label: "14 Days", value: "fourteen_days" },
  { label: "30 Days", value: "thirty_days" },
];

@Component({
  selector: "adm-plan-form",
  templateUrl: "./plan-form.html",
  imports: [
    ButtonModule,
    ValidationSummary,
    ReactiveFormsModule,
    RouterLink,
    ProgressSpinnerModule,
    LabeledField,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    SelectModule,
    CurrencyInput,
  ],
})
export class PlanForm {
  private readonly toastService = inject(ToastService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<PlanFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<PlanFormValue>();
  public readonly remove = output<void>();

  protected readonly intervalOptions = INTERVAL_OPTIONS;
  protected readonly trialPeriodOptions = TRIAL_PERIOD_OPTIONS;

  protected readonly form = new FormGroup({
    name: new FormControl("", { validators: Validators.required, nonNullable: true }),
    description: new FormControl("", { nonNullable: true }),
    price: new FormControl<number>(0, {
      validators: [Validators.required, Validators.min(0)],
      nonNullable: true,
    }),
    interval: new FormControl<BillingInterval>("month", {
      validators: Validators.required,
      nonNullable: true,
    }),
    intervalCount: new FormControl<number>(1, {
      validators: [Validators.required, Validators.min(1)],
      nonNullable: true,
    }),
    trialPeriod: new FormControl<TrialPeriod>("none", { nonNullable: true }),
  });

  constructor() {
    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.save.emit(this.form.getRawValue() as PlanFormValue);
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message:
        "Are you sure that you want to delete this subscription plan? This action cannot be undone.",
      header: "Confirm Delete",
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => this.remove.emit(),
    });
  }

  private patch(src: Partial<PlanFormValue>): void {
    this.form.patchValue({
      ...src,
    });
  }
}
