import { Component, effect, inject, input, output, signal } from "@angular/core";
import { form, FormField, FormRoot, min, required } from "@angular/forms/signals";
import { Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import { Api, createCompanyExpense, updateExpense, type ExpenseDto } from "@logistics/shared/api";
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
import { COMPANY_CATEGORIES } from "../expense.constants";

@Component({
  selector: "app-company-expense-form",
  templateUrl: "./company-expense-form.html",
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
export class CompanyExpenseForm {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly tenantService = inject(TenantService);

  public readonly mode = input<"add" | "edit">("add");
  public readonly initial = input<ExpenseDto | null>(null);
  public readonly saved = output<void>();

  protected readonly receiptPath = signal<string>("");
  protected readonly categories = COMPANY_CATEGORIES;

  protected readonly model = signal({
    amount: null as number | null,
    vendorName: "",
    expenseDate: new Date() as Date | null,
    notes: "",
    category: "office" as string,
  });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.amount, { message: "Amount is required." });
      min(p.amount, 0.01, { message: "Amount must be greater than 0." });
      required(p.expenseDate, { message: "Expense date is required." });
      required(p.category, { message: "Category is required." });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
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
                    currency:
                      this.initial()?.amount?.currency ?? this.tenantService.tenantCurrency(),
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

          if (result || isEdit) {
            this.toast.showSuccess(
              `Company expense ${isEdit ? "updated" : "created"} successfully.`,
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
        category: e.companyCategory ?? "office",
      }));
      if (e.receiptBlobPath) this.receiptPath.set(e.receiptBlobPath);
    });
  }

  protected onReceiptUploaded(path: string): void {
    this.receiptPath.set(path);
  }
}
