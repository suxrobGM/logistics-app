import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import {
  Api,
  createBodyShopExpense,
  createCompanyExpense,
  createTruckExpense,
  getTrucks,
  uploadExpenseReceipt,
} from "@logistics/shared/api";
import type { TruckDto } from "@logistics/shared/api/models";
import { MessageService } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePicker } from "primeng/datepicker";
import { FileUploadModule } from "primeng/fileupload";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TabsModule } from "primeng/tabs";
import { TextareaModule } from "primeng/textarea";
import { ToastModule } from "primeng/toast";
import { LabeledField, PageHeader } from "@/shared/components";

interface CategoryOption {
  label: string;
  value: string;
}

@Component({
  selector: "app-expense-add",
  templateUrl: "./expense-add.html",
  providers: [MessageService],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    CardModule,
    ButtonModule,
    TabsModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    DatePicker,
    SelectModule,
    FileUploadModule,
    ToastModule,
    LabeledField,
    PageHeader,
  ],
})
export class ExpenseAddPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  protected readonly isLoading = signal(false);
  protected readonly activeTab = signal(0);
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

  // Company expense form
  protected readonly companyForm: FormGroup = this.fb.group({
    amount: [null, [Validators.required, Validators.min(0.01)]],
    vendorName: [""],
    expenseDate: [new Date(), Validators.required],
    notes: [""],
    category: ["office", Validators.required],
  });

  // Truck expense form
  protected readonly truckForm: FormGroup = this.fb.group({
    amount: [null, [Validators.required, Validators.min(0.01)]],
    vendorName: [""],
    expenseDate: [new Date(), Validators.required],
    notes: [""],
    truckId: [null, Validators.required],
    category: ["fuel", Validators.required],
    odometerReading: [null],
  });

  // Body shop expense form
  protected readonly bodyShopForm: FormGroup = this.fb.group({
    amount: [null, [Validators.required, Validators.min(0.01)]],
    vendorName: [""],
    expenseDate: [new Date(), Validators.required],
    notes: [""],
    truckId: [null, Validators.required],
    vendorAddress: [""],
    vendorPhone: [""],
    repairDescription: [""],
    estimatedCompletionDate: [null],
    actualCompletionDate: [null],
  });

  ngOnInit(): void {
    this.loadTrucks();
  }

  onTabChange(index: string | number | undefined): void {
    if (typeof index !== "number") return;
    this.activeTab.set(index);
    this.receiptPath.set("");
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

  async submitCompanyExpense(): Promise<void> {
    if (!this.companyForm.valid) {
      this.messageService.add({
        severity: "error",
        summary: "Validation Error",
        detail: "Please fill all required fields.",
      });
      return;
    }

    this.isLoading.set(true);
    const formValue = this.companyForm.value;

    const result = await this.api.invoke(createCompanyExpense, {
      body: {
        amount: formValue.amount,
        currency: "USD",
        vendorName: formValue.vendorName,
        expenseDate: formValue.expenseDate.toISOString(),
        receiptBlobPath: this.receiptPath() || null,
        notes: formValue.notes,
        category: formValue.category,
      },
    });

    this.isLoading.set(false);

    if (result) {
      this.messageService.add({
        severity: "success",
        summary: "Success",
        detail: "Company expense created successfully.",
      });
      this.router.navigate(["/expenses"]);
    }
  }

  async submitTruckExpense(): Promise<void> {
    if (!this.truckForm.valid) {
      this.messageService.add({
        severity: "error",
        summary: "Validation Error",
        detail: "Please fill all required fields.",
      });
      return;
    }

    this.isLoading.set(true);
    const formValue = this.truckForm.value;

    const result = await this.api.invoke(createTruckExpense, {
      body: {
        amount: formValue.amount,
        currency: "USD",
        vendorName: formValue.vendorName,
        expenseDate: formValue.expenseDate.toISOString(),
        receiptBlobPath: this.receiptPath() || null,
        notes: formValue.notes,
        truckId: formValue.truckId,
        category: formValue.category,
        odometerReading: formValue.odometerReading,
      },
    });

    this.isLoading.set(false);

    if (result) {
      this.messageService.add({
        severity: "success",
        summary: "Success",
        detail: "Truck expense created successfully.",
      });
      this.router.navigate(["/expenses"]);
    }
  }

  async submitBodyShopExpense(): Promise<void> {
    if (!this.bodyShopForm.valid) {
      this.messageService.add({
        severity: "error",
        summary: "Validation Error",
        detail: "Please fill all required fields.",
      });
      return;
    }

    this.isLoading.set(true);
    const formValue = this.bodyShopForm.value;

    const result = await this.api.invoke(createBodyShopExpense, {
      body: {
        amount: formValue.amount,
        currency: "USD",
        vendorName: formValue.vendorName,
        expenseDate: formValue.expenseDate.toISOString(),
        receiptBlobPath: this.receiptPath() || null,
        notes: formValue.notes,
        truckId: formValue.truckId,
        vendorAddress: formValue.vendorAddress,
        vendorPhone: formValue.vendorPhone,
        repairDescription: formValue.repairDescription,
        estimatedCompletionDate: formValue.estimatedCompletionDate?.toISOString(),
        actualCompletionDate: formValue.actualCompletionDate?.toISOString(),
      },
    });

    this.isLoading.set(false);

    if (result) {
      this.messageService.add({
        severity: "success",
        summary: "Success",
        detail: "Body shop expense created successfully.",
      });
      this.router.navigate(["/expenses"]);
    }
  }

  private async loadTrucks(): Promise<void> {
    const result = await this.api.invoke(getTrucks, { PageSize: 100 });
    if (result?.items) {
      this.trucks.set(result.items);
    }
  }
}
