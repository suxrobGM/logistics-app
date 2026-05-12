import { Component, effect, inject, input, output, signal } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import { Api, createCompanyExpense, updateExpense, type ExpenseDto } from "@logistics/shared/api";
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
import { COMPANY_CATEGORIES } from "../expense.constants";

@Component({
  selector: "app-company-expense-form",
  templateUrl: "./company-expense-form.html",
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
export class CompanyExpenseForm {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly tenantService = inject(TenantService);

  public readonly mode = input<"add" | "edit">("add");
  public readonly initial = input<ExpenseDto | null>(null);
  public readonly saved = output<void>();

  protected readonly isSaving = signal(false);
  protected readonly receiptPath = signal<string>("");
  protected readonly categories = COMPANY_CATEGORIES;

  protected readonly form = this.fb.group({
    amount: [null as number | null, [Validators.required, Validators.min(0.01)]],
    vendorName: [""],
    expenseDate: [new Date() as Date | null, Validators.required],
    notes: [""],
    category: ["office", Validators.required],
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
        category: e.companyCategory ?? "office",
      });
      if (e.receiptBlobPath) this.receiptPath.set(e.receiptBlobPath);
    });
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
    const v = this.form.value;
    const expenseDate = v.expenseDate!.toISOString();

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
              companyCategory: v.category as ExpenseDto["companyCategory"],
            },
          })
        : await this.api.invoke(createCompanyExpense, {
            body: {
              amount: v.amount!,
              currency: this.tenantService.tenantCurrency(),
              vendorName: v.vendorName ?? undefined,
              expenseDate,
              receiptBlobPath: this.receiptPath(),
              notes: v.notes ?? undefined,
              category: v.category as ExpenseDto["companyCategory"],
            },
          });

    this.isSaving.set(false);

    if (result || isEdit) {
      this.toast.showSuccess(`Company expense ${isEdit ? "updated" : "created"} successfully.`);
      this.saved.emit();
      this.router.navigate(isEdit && editingId ? ["/expenses", editingId] : ["/expenses"]);
    }
  }
}
