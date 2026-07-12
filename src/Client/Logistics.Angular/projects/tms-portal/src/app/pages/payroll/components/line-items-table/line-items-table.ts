import { CommonModule } from "@angular/common";
import { Component, inject, input, output, signal } from "@angular/core";
import { form, FormField, FormRoot, min, required, submit } from "@angular/forms/signals";
import {
  addLineItem,
  Api,
  deleteLineItem,
  updateLineItem,
  type AddLineItemRequest,
  type InvoiceLineItemDto,
  type InvoiceLineItemType,
} from "@logistics/shared/api";
import { payrollLineItemTypeOptions } from "@logistics/shared/api/enums";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import {
  Icon,
  UiButton,
  UiDataTable,
  UiDialog,
  UiNumberField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  UiTooltip,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { UiFormField } from "@/shared/components";

const EMPTY = {
  description: "",
  type: "base_pay" as InvoiceLineItemType,
  amount: 0,
  quantity: 1,
  notes: "",
};

@Component({
  selector: "app-payroll-line-items-table",
  templateUrl: "./line-items-table.html",
  imports: [
    CommonModule,
    CurrencyFormatPipe,
    FormField,
    FormRoot,
    Icon,
    UiButton,
    UiDataTable,
    UiDialog,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    UiTooltip,
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
  protected readonly typeOptions = payrollLineItemTypeOptions;

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(this.model, (p) => {
    required(p.description, { message: "Description is required." });
    required(p.type, { message: "Type is required." });
    required(p.amount, { message: "Amount is required." });
    min(p.amount, 0, { message: "Amount must be 0 or greater." });
    required(p.quantity, { message: "Quantity is required." });
    min(p.quantity, 1, { message: "Quantity must be at least 1." });
  });

  getTypeLabel(type: InvoiceLineItemType): string {
    return payrollLineItemTypeOptions.find((opt) => opt.value === type)?.label ?? type;
  }

  openAddDialog(): void {
    this.editingItem.set(null);
    this.form().reset({ ...EMPTY });
    this.showDialog.set(true);
  }

  openEditDialog(item: InvoiceLineItemDto): void {
    this.editingItem.set(item);
    this.form().reset({
      description: item.description ?? "",
      type: item.type ?? EMPTY.type,
      amount: item.amount?.amount ?? 0,
      quantity: item.quantity ?? 1,
      notes: item.notes ?? "",
    });
    this.showDialog.set(true);
  }

  confirmDelete(item: InvoiceLineItemDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to delete "${item.description}"?`,
      header: "Confirm Delete",
      icon: "warning",
      accept: () => this.deleteItem(item),
    });
  }

  /**
   * The footer button lives in the dialog footer (outside the `<form>`), so submission is driven
   * imperatively via `submit()` rather than `[formRoot]`.
   */
  async saveItem(): Promise<void> {
    await submit(this.form, async () => {
      const value = this.model();
      const invoiceId = this.invoiceId();

      try {
        const editing = this.editingItem();
        if (editing) {
          await this.api.invoke(updateLineItem, {
            invoiceId,
            lineItemId: editing.id!,
            body: {
              description: value.description,
              type: value.type,
              amount: value.amount,
              quantity: value.quantity,
              notes: value.notes || undefined,
            },
          });
          this.toastService.showSuccess("Line item updated");
        } else {
          const request: AddLineItemRequest = {
            description: value.description,
            type: value.type,
            amount: value.amount,
            quantity: value.quantity,
            notes: value.notes || undefined,
          };
          await this.api.invoke(addLineItem, { id: invoiceId, body: request });
          this.toastService.showSuccess("Line item added");
        }

        this.showDialog.set(false);
        this.itemsChanged.emit();
      } catch {
        this.toastService.showError("Failed to save line item");
      }

      return undefined;
    });
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
