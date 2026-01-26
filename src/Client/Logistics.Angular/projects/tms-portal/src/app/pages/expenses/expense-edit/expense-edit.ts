import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import {
  Api,
  getExpenseById,
  getTrucks,
  updateExpense,
  uploadExpenseReceipt,
} from "@logistics/shared/api";
import type { ExpenseDto, TruckDto } from "@logistics/shared/api";
import { MessageService } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePicker } from "primeng/datepicker";
import { FileUploadModule } from "primeng/fileupload";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { ToastModule } from "primeng/toast";
import { LabeledField, PageHeader } from "@/shared/components";

interface CategoryOption {
  label: string;
  value: string;
}

@Component({
  selector: "app-expense-edit",
  templateUrl: "./expense-edit.html",
  providers: [MessageService],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    DatePicker,
    SelectModule,
    FileUploadModule,
    ToastModule,
    ProgressSpinnerModule,
    LabeledField,
    PageHeader,
  ],
})
export class ExpenseEditPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  protected readonly id = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly expense = signal<ExpenseDto | null>(null);
  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly receiptPath = signal<string>("");

  protected readonly companyCategories: CategoryOption[] = [
    { label: "Office", value: "office" },
    { label: "Software", value: "software" },
    { label: "Insurance", value: "insurance" },
    { label: "Legal", value: "legal" },
    { label: "Travel", value: "travel" },
    { label: "Other", value: "other" },
  ];

  protected readonly truckCategories: CategoryOption[] = [
    { label: "Fuel", value: "fuel" },
    { label: "Maintenance", value: "maintenance" },
    { label: "Tires", value: "tires" },
    { label: "Registration", value: "registration" },
    { label: "Toll", value: "toll" },
    { label: "Parking", value: "parking" },
    { label: "Other", value: "other" },
  ];

  protected readonly form: FormGroup = this.fb.group({
    amount: [null, [Validators.required, Validators.min(0.01)]],
    vendorName: [""],
    expenseDate: [new Date(), Validators.required],
    notes: [""],
    // Company expense fields
    companyCategory: [""],
    // Truck expense fields
    truckId: [null],
    truckCategory: [""],
    odometerReading: [null],
    // Body shop fields
    vendorAddress: [""],
    vendorPhone: [""],
    repairDescription: [""],
    estimatedCompletionDate: [null],
    actualCompletionDate: [null],
  });

  ngOnInit(): void {
    this.loadExpense();
    this.loadTrucks();
  }

  async onReceiptUpload(event: { files: File[] }): Promise<void> {
    if (event.files.length === 0) return;

    const file = event.files[0];
    this.isLoading.set(true);

    const result = await this.api.invoke(uploadExpenseReceipt, {
      body: { File: file },
    });

    this.isLoading.set(false);

    if (result?.blobPath) {
      this.receiptPath.set(result.blobPath);
      this.messageService.add({
        severity: "success",
        summary: "Receipt Uploaded",
        detail: "Receipt file attached successfully.",
      });
    } else {
      this.messageService.add({
        severity: "error",
        summary: "Upload Failed",
        detail: "Failed to upload receipt. Please try again.",
      });
    }
  }

  async onSubmit(): Promise<void> {
    if (!this.form.valid) {
      this.messageService.add({
        severity: "error",
        summary: "Validation Error",
        detail: "Please fill all required fields.",
      });
      return;
    }

    this.isSaving.set(true);
    const formValue = this.form.value;
    const e = this.expense();

    const result = await this.api.invoke(updateExpense, {
      id: this.id(),
      body: {
        id: this.id(),
        amount: formValue.amount,
        currency: e?.amount?.currency || "USD",
        vendorName: formValue.vendorName,
        expenseDate:
          formValue.expenseDate instanceof Date
            ? formValue.expenseDate.toISOString()
            : formValue.expenseDate,
        receiptBlobPath: this.receiptPath() || e?.receiptBlobPath || undefined,
        notes: formValue.notes,
        companyCategory: e?.type === "company" ? formValue.companyCategory : undefined,
        truckCategory: e?.type === "truck" ? formValue.truckCategory : undefined,
        odometerReading: e?.type === "truck" ? formValue.odometerReading : undefined,
        vendorAddress: e?.type === "body_shop" ? formValue.vendorAddress : undefined,
        vendorPhone: e?.type === "body_shop" ? formValue.vendorPhone : undefined,
        repairDescription: e?.type === "body_shop" ? formValue.repairDescription : undefined,
        estimatedCompletionDate:
          e?.type === "body_shop" && formValue.estimatedCompletionDate
            ? formValue.estimatedCompletionDate instanceof Date
              ? formValue.estimatedCompletionDate.toISOString()
              : formValue.estimatedCompletionDate
            : undefined,
        actualCompletionDate:
          e?.type === "body_shop" && formValue.actualCompletionDate
            ? formValue.actualCompletionDate instanceof Date
              ? formValue.actualCompletionDate.toISOString()
              : formValue.actualCompletionDate
            : undefined,
      },
    });

    this.isSaving.set(false);

    if (result !== undefined) {
      this.messageService.add({
        severity: "success",
        summary: "Success",
        detail: "Expense updated successfully.",
      });
      this.router.navigate(["/expenses", this.id()]);
    }
  }

  getTypeLabel(type: string | undefined): string {
    switch (type) {
      case "company":
        return "Company Expense";
      case "truck":
        return "Truck Expense";
      case "body_shop":
        return "Body Shop Expense";
      default:
        return "Expense";
    }
  }

  private async loadExpense(): Promise<void> {
    this.isLoading.set(true);
    const result = await this.api.invoke(getExpenseById, { id: this.id() });

    if (result) {
      this.expense.set(result);
      this.populateForm(result);
      if (result.receiptBlobPath) {
        this.receiptPath.set(result.receiptBlobPath);
      }
    }

    this.isLoading.set(false);
  }

  private populateForm(e: ExpenseDto): void {
    this.form.patchValue({
      amount: e.amount?.amount,
      vendorName: e.vendorName,
      expenseDate: e.expenseDate ? new Date(e.expenseDate) : new Date(),
      notes: e.notes,
      companyCategory: e.companyCategory,
      truckId: e.truckId,
      truckCategory: e.truckCategory,
      odometerReading: e.odometerReading,
      vendorAddress: e.vendorAddress,
      vendorPhone: e.vendorPhone,
      repairDescription: e.repairDescription,
      estimatedCompletionDate: e.estimatedCompletionDate
        ? new Date(e.estimatedCompletionDate)
        : null,
      actualCompletionDate: e.actualCompletionDate ? new Date(e.actualCompletionDate) : null,
    });
  }

  private async loadTrucks(): Promise<void> {
    const result = await this.api.invoke(getTrucks, { PageSize: 100 });
    if (result?.items) {
      this.trucks.set(result.items);
    }
  }
}
