/* eslint-disable @typescript-eslint/no-empty-function */
import { Component, forwardRef, inject, model, output, signal } from "@angular/core";
import { type ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from "@angular/forms";
import { Api, type CustomerDto, createCustomer, getCustomers } from "@logistics/shared/api";
import {
  AutoComplete,
  AutoCompleteModule,
  type AutoCompleteSelectEvent,
} from "primeng/autocomplete";
import { Button } from "primeng/button";
import { Dialog } from "primeng/dialog";
import { ToastService } from "@/core/services";
import { CustomerForm, type CustomerFormValue } from "@/shared/components/domain-forms";

@Component({
  selector: "app-search-customer",
  templateUrl: "./search-customer.html",
  imports: [AutoCompleteModule, FormsModule, Button, Dialog, CustomerForm],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchCustomer),
      multi: true,
    },
  ],
})
export class SearchCustomer implements ControlValueAccessor {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly suggestedCustomers = signal<CustomerDto[]>([]);
  protected readonly lastQuery = signal<string>("");

  public readonly selectedCustomer = model<CustomerDto | null>(null);
  public readonly selectedCustomerChange = output<CustomerDto | null>();

  protected readonly customerDialogVisible = model<boolean>(false);

  protected async searchCustomer(event: { query: string }): Promise<void> {
    console.log("Searching for customers with query:", event.query);

    const q = event.query?.trim() ?? "";
    this.lastQuery.set(q);

    if (q.length < 2) {
      this.suggestedCustomers.set([]);
      return;
    }

    try {
      const result = await this.api.invoke(getCustomers, { Search: q });
      const items = result.items ?? [];
      console.log("Found customers:", items);

      this.suggestedCustomers.set(items); // [] triggers the "empty" template
    } catch {
      this.suggestedCustomers.set([]);
    }
  }

  protected changeSelectedCustomer(event: AutoCompleteSelectEvent): void {
    console.log("Selected customer:", event.value);

    this.selectedCustomerChange.emit(event.value);
    this.onChange(event.value);
  }

  protected openCreateCustomer(autoComplete: AutoComplete): void {
    // close the suggestions panel before opening dialog
    autoComplete.hide();
    this.customerDialogVisible.set(true);
  }

  protected async createCustomer(formValue: CustomerFormValue): Promise<void> {
    const result = await this.api.invoke(createCustomer, { body: formValue });
    if (result) {
      this.toastService.showSuccess("A new customer has been created successfully");
      this.customerDialogVisible.set(false);
      this.selectedCustomer.set(result);
    }
  }

  //#region Implementation Reactive forms

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: CustomerDto | null): void {
    console.log("onChange called with value:", value);
  }
  private onTouched(): void {}

  writeValue(value: CustomerDto | null): void {
    console.log("writeValue called with value:", value);

    this.selectedCustomer.set(value);
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.selectedCustomer.set(null);
    }
  }

  //#endregion
}
