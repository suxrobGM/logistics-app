import { Component, effect, inject, input, output, signal, type OnInit } from "@angular/core";
import { disabled, form, FormField, FormRoot, min, required } from "@angular/forms/signals";
import { Router, RouterModule } from "@angular/router";
import { LocalizationService, ToastService } from "@logistics/shared";
import {
  Api,
  createTruckExpense,
  getTrucks,
  updateExpense,
  type ExpenseDto,
  type TruckDto,
  type VolumeUnit,
} from "@logistics/shared/api";
import {
  Grid,
  Stack,
  UiButton,
  UiDateField,
  UiNumberField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { TenantService } from "@/core/services";
import { UiFormField } from "@/shared/components";
import { ExpenseReceiptUpload } from "../expense-receipt-upload/expense-receipt-upload";
import { TRUCK_CATEGORIES, VOLUME_UNIT_OPTIONS } from "../expense.constants";

@Component({
  selector: "app-truck-expense-form",
  templateUrl: "./truck-expense-form.html",
  imports: [
    ExpenseReceiptUpload,
    FormField,
    FormRoot,
    Grid,
    RouterModule,
    Stack,
    UiButton,
    UiDateField,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class TruckExpenseForm implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly tenantService = inject(TenantService);
  private readonly localization = inject(LocalizationService);

  public readonly mode = input<"add" | "edit">("add");
  public readonly initial = input<ExpenseDto | null>(null);
  public readonly saved = output<void>();

  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly receiptPath = signal<string>("");
  protected readonly categories = TRUCK_CATEGORIES;
  protected readonly volumeUnits = VOLUME_UNIT_OPTIONS;

  protected readonly model = signal({
    amount: null as number | null,
    vendorName: "",
    expenseDate: new Date() as Date | null,
    notes: "",
    truckId: null as string | null,
    category: "fuel" as string,
    odometerReading: null as number | null,
    quantity: null as number | null,
    quantityUnit: this.defaultVolumeUnit() as VolumeUnit,
  });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.amount, { message: "Amount is required." });
      min(p.amount, 0.01, { message: "Amount must be greater than 0." });
      required(p.expenseDate, { message: "Expense date is required." });
      required(p.truckId, { message: "Truck is required." });
      required(p.category, { message: "Category is required." });
      min(p.odometerReading, 0, { message: "Odometer reading cannot be negative." });
      min(p.quantity, 0, { message: "Quantity cannot be negative." });
      // Truck is fixed once the expense exists - disable it when editing.
      disabled(p.truckId, { when: () => this.mode() === "edit" });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
          const expenseDate = v.expenseDate!.toISOString();
          const isFuel = v.category === "fuel";

          const isEdit = this.mode() === "edit";
          const editingId = this.initial()?.id;

          const result =
            isEdit && editingId
              ? await this.api.invoke(updateExpense, {
                  id: editingId,
                  body: {
                    id: editingId,
                    amount: v.amount!,
                    currency:
                      this.initial()?.amount?.currency ?? this.tenantService.tenantCurrency(),
                    vendorName: v.vendorName ?? undefined,
                    expenseDate,
                    receiptBlobPath: this.receiptPath() || undefined,
                    notes: v.notes ?? undefined,
                    truckCategory: v.category as ExpenseDto["truckCategory"],
                    odometerReading: v.odometerReading ?? undefined,
                    quantity: isFuel ? (v.quantity ?? undefined) : undefined,
                    quantityUnit: isFuel ? (v.quantityUnit ?? undefined) : undefined,
                  },
                })
              : await this.api.invoke(createTruckExpense, {
                  body: {
                    amount: v.amount!,
                    currency: this.tenantService.tenantCurrency(),
                    vendorName: v.vendorName ?? undefined,
                    expenseDate,
                    receiptBlobPath: this.receiptPath(),
                    notes: v.notes ?? undefined,
                    truckId: v.truckId!,
                    category: v.category as ExpenseDto["truckCategory"],
                    odometerReading: v.odometerReading ?? undefined,
                    quantity: isFuel ? (v.quantity ?? undefined) : undefined,
                    quantityUnit: isFuel ? (v.quantityUnit ?? undefined) : undefined,
                  },
                });

          if (result || isEdit) {
            this.toast.showSuccess(`Truck expense ${isEdit ? "updated" : "created"} successfully.`);
            this.saved.emit();
            this.router.navigate(isEdit && editingId ? ["/expenses", editingId] : ["/expenses"]);
          }
          return undefined;
        },
      },
    },
  );

  constructor() {
    effect(() => {
      const e = this.initial();
      if (!e) return;
      this.model.update((v) => ({
        ...v,
        amount: e.amount?.amount ?? null,
        vendorName: e.vendorName ?? "",
        expenseDate: e.expenseDate ? new Date(e.expenseDate) : new Date(),
        notes: e.notes ?? "",
        truckId: e.truckId ?? null,
        category: e.truckCategory ?? "fuel",
        odometerReading: e.odometerReading ?? null,
        quantity: e.quantity ?? null,
        quantityUnit: e.quantityUnit ?? this.defaultVolumeUnit(),
      }));
      if (e.receiptBlobPath) this.receiptPath.set(e.receiptBlobPath);
    });
  }

  ngOnInit(): void {
    this.loadTrucks();
  }

  protected onReceiptUploaded(path: string): void {
    this.receiptPath.set(path);
  }

  private defaultVolumeUnit(): VolumeUnit {
    return this.localization.getVolumeUnit() === "L" ? "liters" : "gallons";
  }

  private async loadTrucks(): Promise<void> {
    const result = await this.api.invoke(getTrucks, { PageSize: 100 });
    if (result?.items) this.trucks.set(result.items);
  }
}
