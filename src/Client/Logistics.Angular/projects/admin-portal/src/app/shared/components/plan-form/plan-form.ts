import { Component, effect, inject, input, output, signal } from "@angular/core";
import { form, FormField, FormRoot, max, min, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared";
import type { BillingInterval, PlanTier } from "@logistics/shared/api";
import {
  CurrencyField,
  UiButton,
  UiFormField,
  UiNumberField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";

export interface PlanFormValue {
  name: string;
  description: string;
  tier: PlanTier;
  price: number;
  perTruckPrice: number;
  maxTrucks: number | null;
  weeklyAiRequestQuota: number | null;
  interval: BillingInterval;
  intervalCount: number;
}

const TIER_OPTIONS = [
  { label: "Starter", value: "starter" },
  { label: "Professional", value: "professional" },
  { label: "Enterprise", value: "enterprise" },
];

const INTERVAL_OPTIONS = [
  { label: "Day", value: "day" },
  { label: "Week", value: "week" },
  { label: "Month", value: "month" },
  { label: "Year", value: "year" },
];

const EMPTY: PlanFormValue = {
  name: "",
  description: "",
  tier: "starter",
  price: 0,
  perTruckPrice: 0,
  maxTrucks: null,
  weeklyAiRequestQuota: null,
  interval: "month",
  intervalCount: 1,
};

@Component({
  selector: "adm-plan-form",
  templateUrl: "./plan-form.html",
  imports: [
    CurrencyField,
    FormField,
    FormRoot,
    RouterLink,
    UiButton,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class PlanForm {
  private readonly toastService = inject(ToastService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<PlanFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<PlanFormValue>();
  public readonly remove = output<void>();

  protected readonly tierOptions = TIER_OPTIONS;
  protected readonly intervalOptions = INTERVAL_OPTIONS;

  protected readonly model = signal<PlanFormValue>({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.name, { message: "Plan name is required." });
      required(p.tier, { message: "Plan tier is required." });
      required(p.price, { message: "Base monthly fee is required." });
      min(p.price, 0, { message: "Base monthly fee cannot be negative." });
      required(p.perTruckPrice, { message: "Per truck fee is required." });
      min(p.perTruckPrice, 0, { message: "Per truck fee cannot be negative." });
      min(p.maxTrucks, 1, { message: "Max trucks must be at least 1." });
      min(p.weeklyAiRequestQuota, 1, { message: "Weekly AI request quota must be at least 1." });
      required(p.interval, { message: "Billing interval is required." });
      required(p.intervalCount, { message: "Interval count is required." });
      min(p.intervalCount, 1, { message: "Interval count must be at least 1." });
      max(p.intervalCount, 12, { message: "Interval count cannot exceed 12." });
    },
    {
      submission: {
        action: async () => {
          this.save.emit(this.model());
          return undefined;
        },
      },
    },
  );

  constructor() {
    effect(() => {
      const initial = this.initial();
      if (initial) {
        this.model.update((v) => ({ ...v, ...initial }));
      }
    });
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message:
        "Are you sure that you want to delete this subscription plan? This action cannot be undone.",
      header: "Confirm Delete",
      icon: "warning",
      severity: "danger",
      accept: () => this.remove.emit(),
    });
  }
}
