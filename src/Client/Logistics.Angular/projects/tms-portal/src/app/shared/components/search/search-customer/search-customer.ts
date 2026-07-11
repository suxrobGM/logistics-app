import { Component, ElementRef, inject, input, model, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl } from "@logistics/shared";
import { Api, getCustomers, type CustomerDto } from "@logistics/shared/api";
import { UiButton, UiDialog } from "@logistics/shared/ui";
import {
  AutoComplete,
  AutoCompleteModule,
  type AutoCompleteSelectEvent,
} from "primeng/autocomplete";
import { CustomerForm } from "@/shared/components/domain-forms";

@Component({
  selector: "app-search-customer",
  templateUrl: "./search-customer.html",
  imports: [AutoCompleteModule, CustomerForm, FormsModule, UiButton, UiDialog],
})
export class SearchCustomer implements FormValueControl<CustomerDto | null> {
  private readonly api = inject(Api);

  protected readonly suggestedCustomers = signal<CustomerDto[]>([]);
  protected readonly lastQuery = signal<string>("");

  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<CustomerDto | null>(null);

  /** Driven by the Reactive Forms bridge / Signal Forms when disabled. */
  public readonly disabled = input<boolean>(false);

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  protected readonly customerDialogVisible = model<boolean>(false);

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

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
    this.value.set(event.value);
  }

  protected openCreateCustomer(autoComplete: AutoComplete): void {
    // close the suggestions panel before opening dialog
    autoComplete.hide();
    this.customerDialogVisible.set(true);
  }

  protected handleCustomerCreated(customer: CustomerDto): void {
    this.customerDialogVisible.set(false);
    this.value.set(customer);
  }
}
