import { CommonModule, CurrencyPipe } from "@angular/common";
import { Component, inject, input, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Api, addLineItem, deleteLineItem, updateLineItem } from "@logistics/shared/api";
import type {
  AddLineItemRequest,
  InvoiceLineItemDto,
  InvoiceLineItemType,
} from "@logistics/shared/api";
import { payrollLineItemTypeOptions } from "@logistics/shared/api/enums";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TextareaModule } from "primeng/textarea";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { LabeledField } from "@/shared/components";

interface LineItemForm {
  description: FormControl<string>;
  type: FormControl<InvoiceLineItemType>;
  amount: FormControl<number>;
  quantity: FormControl<number>;
  notes: FormControl<string | null>;
}

@Component({
  selector: "app-payroll-line-items-table",
  templateUrl: "./line-items-table.html",
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    TextareaModule,
    TooltipModule,
    LabeledField,
    CurrencyPipe,
  ],
})
export class PayrollLineItemsTable {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  readonly invoiceId = input.required<string>();
  readonly lineItems = input<InvoiceLineItemDto[]>([]);
  readonly itemsChanged = output<void>();

  protected readonly showDialog = signal(false);
  protected readonly editingItem = signal<InvoiceLineItemDto | null>(null);
  protected readonly isSaving = signal(false);
  protected readonly typeOptions = payrollLineItemTypeOptions;

  protected readonly form = new FormGroup<LineItemForm>({
    description: new FormControl("", { validators: Validators.required, nonNullable: true }),
    type: new FormControl<InvoiceLineItemType>("base_pay", {
      validators: Validators.required,
      nonNullable: true,
    }),
    amount: new FormControl(0, {
      validators: [Validators.required, Validators.min(0)],
      nonNullable: true,
    }),
    quantity: new FormControl(1, {
      validators: [Validators.required, Validators.min(1)],
      nonNullable: true,
    }),
    notes: new FormControl<string | null>(null),
  });

  getTypeLabel(type: InvoiceLineItemType): string {
    return payrollLineItemTypeOptions.find((opt) => opt.value === type)?.label ?? type;
  }

  openAddDialog(): void {
    this.editingItem.set(null);
    this.form.reset({
      description: "",
      type: "base_pay",
      amount: 0,
      quantity: 1,
      notes: null,
    });
    this.showDialog.set(true);
  }

  openEditDialog(item: InvoiceLineItemDto): void {
    this.editingItem.set(item);
    this.form.patchValue({
      description: item.description ?? "",
      type: item.type,
      amount: item.amount?.amount ?? 0,
      quantity: item.quantity ?? 1,
      notes: item.notes ?? null,
    });
    this.showDialog.set(true);
  }

  confirmDelete(item: InvoiceLineItemDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to delete "${item.description}"?`,
      header: "Confirm Delete",
      icon: "pi pi-exclamation-triangle",
      accept: () => this.deleteItem(item),
    });
  }

  async saveItem(): Promise<void> {
    if (!this.form.valid) return;

    this.isSaving.set(true);
    const formValue = this.form.value;
    const invoiceId = this.invoiceId();

    try {
      const editing = this.editingItem();
      if (editing) {
        await this.api.invoke(updateLineItem, {
          invoiceId,
          lineItemId: editing.id!,
          body: {
            description: formValue.description,
            type: formValue.type,
            amount: formValue.amount,
            quantity: formValue.quantity,
            notes: formValue.notes ?? undefined,
          },
        });
        this.toastService.showSuccess("Line item updated");
      } else {
        const request: AddLineItemRequest = {
          description: formValue.description!,
          type: formValue.type!,
          amount: formValue.amount!,
          quantity: formValue.quantity!,
          notes: formValue.notes ?? undefined,
        };
        await this.api.invoke(addLineItem, { id: invoiceId, body: request });
        this.toastService.showSuccess("Line item added");
      }

      this.showDialog.set(false);
      this.itemsChanged.emit();
    } catch {
      this.toastService.showError("Failed to save line item");
    } finally {
      this.isSaving.set(false);
    }
  }

  private async deleteItem(item: InvoiceLineItemDto): Promise<void> {
    try {
      await this.api.invoke(deleteLineItem, {
        invoiceId: this.invoiceId(),
        lineItemId: item.id!,
      });
      this.toastService.showSuccess("Line item deleted");
      this.itemsChanged.emit();
    } catch {
      this.toastService.showError("Failed to delete line item");
    }
  }
}
