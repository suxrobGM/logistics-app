import { Component, effect, inject, input, output, signal, type OnInit } from "@angular/core";
import { disabled, form, FormField, FormRoot, min, required } from "@angular/forms/signals";
import { Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import {
  Api,
  createBodyShopExpense,
  getTrucks,
  updateExpense,
  type ExpenseDto,
  type TruckDto,
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

@Component({
  selector: "app-body-shop-expense-form",
  templateUrl: "./body-shop-expense-form.html",
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
export class BodyShopExpenseForm implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly tenantService = inject(TenantService);

  public readonly mode = input<"add" | "edit">("add");
  public readonly initial = input<ExpenseDto | null>(null);
  public readonly saved = output<void>();

  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly receiptPath = signal<string>("");

  protected readonly model = signal({
    amount: null as number | null,
    vendorName: "",
    expenseDate: new Date() as Date | null,
    notes: "",
    truckId: null as string | null,
    vendorAddress: "",
    vendorPhone: "",
    repairDescription: "",
    estimatedCompletionDate: null as Date | null,
    actualCompletionDate: null as Date | null,
  });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.amount, { message: "Amount is required." });
      min(p.amount, 0.01, { message: "Amount must be greater than 0." });
      required(p.expenseDate, { message: "Expense date is required." });
      required(p.truckId, { message: "Truck is required." });
      // Truck is fixed once the expense exists - disable it when editing.
      disabled(p.truckId, { when: () => this.mode() === "edit" });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
          const expenseDate = v.expenseDate!.toISOString();
          const estDate = v.estimatedCompletionDate?.toISOString();
          const actDate = v.actualCompletionDate?.toISOString();

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
                    vendorName: v.vendorName,
                    expenseDate,
                    receiptBlobPath: this.receiptPath(),
                    notes: v.notes,
                    vendorAddress: v.vendorAddress,
                    vendorPhone: v.vendorPhone,
                    repairDescription: v.repairDescription,
                    estimatedCompletionDate: estDate,
                    actualCompletionDate: actDate,
                  },
                })
              : await this.api.invoke(createBodyShopExpense, {
                  body: {
                    amount: v.amount!,
                    currency: this.tenantService.tenantCurrency(),
                    vendorName: v.vendorName,
                    expenseDate,
                    receiptBlobPath: this.receiptPath(),
                    notes: v.notes,
                    truckId: v.truckId!,
                    vendorAddress: v.vendorAddress,
                    vendorPhone: v.vendorPhone,
                    repairDescription: v.repairDescription,
                    estimatedCompletionDate: estDate,
                    actualCompletionDate: actDate,
                  },
                });

          if (result || isEdit) {
            this.toast.showSuccess(
              `Body shop expense ${isEdit ? "updated" : "created"} successfully.`,
            );
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
        vendorAddress: e.vendorAddress ?? "",
        vendorPhone: e.vendorPhone ?? "",
        repairDescription: e.repairDescription ?? "",
        estimatedCompletionDate: e.estimatedCompletionDate
          ? new Date(e.estimatedCompletionDate)
          : null,
        actualCompletionDate: e.actualCompletionDate ? new Date(e.actualCompletionDate) : null,
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

  private async loadTrucks(): Promise<void> {
    const result = await this.api.invoke(getTrucks, { PageSize: 100 });
    if (result?.items) this.trucks.set(result.items);
  }
}
