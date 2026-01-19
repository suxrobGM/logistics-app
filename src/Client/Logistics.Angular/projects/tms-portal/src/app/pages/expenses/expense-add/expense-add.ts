import { CommonModule } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import {
  Api,
  createCompanyExpense,
  createTruckExpense,
  createBodyShopExpense,
  getTrucks,
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

  readonly isLoading = signal(false);
  readonly activeTab = signal(0);
  readonly trucks = signal<TruckDto[]>([]);
  readonly receiptPath = signal<string>("");

  readonly companyCategories: CategoryOption[] = [
    { label: "Office", value: "Office" },
    { label: "Software", value: "Software" },
    { label: "Insurance", value: "Insurance" },
    { label: "Legal", value: "Legal" },
    { label: "Travel", value: "Travel" },
    { label: "Other", value: "Other" },
  ];

  readonly truckCategories: CategoryOption[] = [
    { label: "Fuel", value: "Fuel" },
    { label: "Maintenance", value: "Maintenance" },
    { label: "Tires", value: "Tires" },
    { label: "Registration", value: "Registration" },
    { label: "Toll", value: "Toll" },
    { label: "Parking", value: "Parking" },
    { label: "Other", value: "Other" },
  ];

  // Company expense form
  companyForm: FormGroup = this.fb.group({
    amount: [null, [Validators.required, Validators.min(0.01)]],
    vendorName: [""],
    expenseDate: [new Date(), Validators.required],
    notes: [""],
    category: ["Office", Validators.required],
  });

  // Truck expense form
  truckForm: FormGroup = this.fb.group({
    amount: [null, [Validators.required, Validators.min(0.01)]],
    vendorName: [""],
    expenseDate: [new Date(), Validators.required],
    notes: [""],
    truckId: [null, Validators.required],
    category: ["Fuel", Validators.required],
    odometerReading: [null],
  });

  // Body shop expense form
  bodyShopForm: FormGroup = this.fb.group({
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

  onReceiptUpload(event: { files: File[] }): void {
    // TODO: Upload to blob storage and get path
    // For now, use a placeholder path
    if (event.files.length > 0) {
      this.receiptPath.set(`receipts/${Date.now()}_${event.files[0].name}`);
      this.messageService.add({
        severity: "success",
        summary: "Receipt Uploaded",
        detail: "Receipt file attached successfully.",
      });
    }
  }

  async submitCompanyExpense(): Promise<void> {
    if (!this.companyForm.valid || !this.receiptPath()) {
      this.messageService.add({
        severity: "error",
        summary: "Validation Error",
        detail: "Please fill all required fields and attach a receipt.",
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
        receiptBlobPath: this.receiptPath(),
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
    if (!this.truckForm.valid || !this.receiptPath()) {
      this.messageService.add({
        severity: "error",
        summary: "Validation Error",
        detail: "Please fill all required fields and attach a receipt.",
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
        receiptBlobPath: this.receiptPath(),
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
    if (!this.bodyShopForm.valid || !this.receiptPath()) {
      this.messageService.add({
        severity: "error",
        summary: "Validation Error",
        detail: "Please fill all required fields and attach a receipt.",
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
        receiptBlobPath: this.receiptPath(),
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
