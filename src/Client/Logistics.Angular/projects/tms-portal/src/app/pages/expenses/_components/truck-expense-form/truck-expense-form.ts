import { Component, effect, inject, input, output, signal, type OnInit } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
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
import { Grid, Stack } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DatePicker } from "primeng/datepicker";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { TenantService } from "@/core/services";
import { FormField } from "@/shared/components";
import { ExpenseReceiptUpload } from "../expense-receipt-upload/expense-receipt-upload";
import { TRUCK_CATEGORIES, VOLUME_UNIT_OPTIONS } from "../expense.constants";

@Component({
  selector: "app-truck-expense-form",
  templateUrl: "./truck-expense-form.html",
  imports: [
    ReactiveFormsModule,
    RouterModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    DatePicker,
    SelectModule,
    FormField,
    Grid,
    Stack,
    ExpenseReceiptUpload,
  ],
})
export class TruckExpenseForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly tenantService = inject(TenantService);
  private readonly localization = inject(LocalizationService);

  public readonly mode = input<"add" | "edit">("add");
  public readonly initial = input<ExpenseDto | null>(null);
  public readonly saved = output<void>();

  protected readonly isSaving = signal(false);
  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly receiptPath = signal<string>("");
  protected readonly categories = TRUCK_CATEGORIES;
  protected readonly volumeUnits = VOLUME_UNIT_OPTIONS;

  protected readonly form = this.fb.group({
    amount: [null as number | null, [Validators.required, Validators.min(0.01)]],
    vendorName: [""],
    expenseDate: [new Date() as Date | null, Validators.required],
    notes: [""],
    truckId: [null as string | null, Validators.required],
    category: ["fuel", Validators.required],
    odometerReading: [null as number | null],
    quantity: [null as number | null],
    quantityUnit: [this.defaultVolumeUnit() as VolumeUnit],
  });

  constructor() {
    effect(() => {
      const e = this.initial();
      if (!e) return;
      this.form.patchValue({
        amount: e.amount?.amount ?? null,
        vendorName: e.vendorName ?? "",
        expenseDate: e.expenseDate ? new Date(e.expenseDate) : new Date(),
        notes: e.notes ?? "",
        truckId: e.truckId ?? null,
        category: e.truckCategory ?? "fuel",
        odometerReading: e.odometerReading ?? null,
        quantity: e.quantity ?? null,
        quantityUnit: e.quantityUnit ?? this.defaultVolumeUnit(),
      });
      if (this.mode() === "edit") {
        this.form.controls.truckId.disable({ emitEvent: false });
      }
      if (e.receiptBlobPath) this.receiptPath.set(e.receiptBlobPath);
    });
  }

  ngOnInit(): void {
    this.loadTrucks();
  }

  protected onReceiptUploaded(path: string): void {
    this.receiptPath.set(path);
  }

  async submit(): Promise<void> {
    if (!this.form.valid) {
      this.toast.showError("Please fill all required fields.");
      return;
    }

    this.isSaving.set(true);
    const v = this.form.getRawValue();
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
              currency: this.initial()?.amount?.currency ?? this.tenantService.tenantCurrency(),
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

    this.isSaving.set(false);

    if (result || isEdit) {
      this.toast.showSuccess(`Truck expense ${isEdit ? "updated" : "created"} successfully.`);
      this.saved.emit();
      this.router.navigate(isEdit && editingId ? ["/expenses", editingId] : ["/expenses"]);
    }
  }

  private defaultVolumeUnit(): VolumeUnit {
    return this.localization.getVolumeUnit() === "L" ? "liters" : "gallons";
  }

  private async loadTrucks(): Promise<void> {
    const result = await this.api.invoke(getTrucks, { PageSize: 100 });
    if (result?.items) this.trucks.set(result.items);
  }
}
