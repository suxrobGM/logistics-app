/* eslint-disable @typescript-eslint/no-empty-function */
import { Component, forwardRef, inject, model, output, signal } from "@angular/core";
import { type ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from "@angular/forms";
import {
  AutoComplete,
  AutoCompleteModule,
  type AutoCompleteSelectEvent,
} from "primeng/autocomplete";
import { Button } from "primeng/button";
import { Dialog } from "primeng/dialog";
import { Api, type CustomerDto, createCustomer, getCustomers } from "@/core/api";
import { ToastService } from "@/core/services";
import { CustomerForm, type CustomerFormValue } from "../customer-form/customer-form";

@Component({
  selector: "app-search-customer",
  templateUrl: "./search-customer.html",
  imports: [AutoCompleteModule, FormsModule, Button, Dialog, CustomerForm],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchCustomerComponent),
      multi: true,
    },
  ],
})
export class SearchCustomerComponent implements ControlValueAccessor {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly suggestedCustomers = signal<CustomerDto[]>([]);
  protected readonly lastQuery = signal<string>("");

  public readonly selectedCustomer = model<CustomerDto | null>(null);
  public readonly selectedCustomerChange = output<CustomerDto | null>();

  protected readonly customerDialogVisible = model<boolean>(false);

  protected async searchCustomer(event: { query: string }): Promise<void> {
    const q = event.query?.trim() ?? "";
    this.lastQuery.set(q);

    if (q.length < 2) {
      this.suggestedCustomers.set([]);
      return;
    }

    try {
      const result = await this.api.invoke(getCustomers, { Search: q });
      const items = result.items ?? [];
      this.suggestedCustomers.set(items); // [] triggers the "empty" template
    } catch {
      this.suggestedCustomers.set([]);
    }
  }

  protected changeSelectedCustomer(event: AutoCompleteSelectEvent): void {
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
  private onChange(value: CustomerDto | null): void {}
  private onTouched(): void {}

  writeValue(value: CustomerDto | null): void {
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
