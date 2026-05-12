import { Component, effect, inject, input, output, signal, type OnInit } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
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

@Component({
  selector: "app-body-shop-expense-form",
  templateUrl: "./body-shop-expense-form.html",
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
export class BodyShopExpenseForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly tenantService = inject(TenantService);

  public readonly mode = input<"add" | "edit">("add");
  public readonly initial = input<ExpenseDto | null>(null);
  public readonly saved = output<void>();

  protected readonly isSaving = signal(false);
  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly receiptPath = signal<string>("");

  protected readonly form = this.fb.group({
    amount: [null as number | null, [Validators.required, Validators.min(0.01)]],
    vendorName: [""],
    expenseDate: [new Date() as Date | null, Validators.required],
    notes: [""],
    truckId: [null as string | null, Validators.required],
    vendorAddress: [""],
    vendorPhone: [""],
    repairDescription: [""],
    estimatedCompletionDate: [null as Date | null],
    actualCompletionDate: [null as Date | null],
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
        vendorAddress: e.vendorAddress ?? "",
        vendorPhone: e.vendorPhone ?? "",
        repairDescription: e.repairDescription ?? "",
        estimatedCompletionDate: e.estimatedCompletionDate
          ? new Date(e.estimatedCompletionDate)
          : null,
        actualCompletionDate: e.actualCompletionDate ? new Date(e.actualCompletionDate) : null,
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
              currency: this.initial()?.amount?.currency ?? this.tenantService.tenantCurrency(),
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

    this.isSaving.set(false);

    if (result || isEdit) {
      this.toast.showSuccess(`Body shop expense ${isEdit ? "updated" : "created"} successfully.`);
      this.saved.emit();
      this.router.navigate(isEdit && editingId ? ["/expenses", editingId] : ["/expenses"]);
    }
  }

  private async loadTrucks(): Promise<void> {
    const result = await this.api.invoke(getTrucks, { PageSize: 100 });
    if (result?.items) this.trucks.set(result.items);
  }
}
