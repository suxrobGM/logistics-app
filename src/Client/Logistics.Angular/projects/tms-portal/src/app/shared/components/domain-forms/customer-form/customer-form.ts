import { Component, computed, effect, inject, input, output, signal } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import { isEuCountry, regionAllowedCountries } from "@logistics/shared";
import {
  Api,
  createCustomer,
  updateCustomer,
  type Address,
  type CreateCustomerCommand,
  type CustomerDto,
  type CustomerStatus,
  type UpdateCustomerCommand,
} from "@logistics/shared/api";
import { customerStatusOptions } from "@logistics/shared/api/enums";
import {
  AddressForm,
  Grid,
  Spinner,
  Stack,
  UiButton,
  UiCheckboxField,
  UiFormField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { TenantService } from "@/core/services/tenant.service";

export interface CustomerFormValue {
  name: string;
  email: string;
  phone: string;
  status: CustomerStatus;
  address: Address | null;
  notes: string;
  taxId: string;
  isVatExempt: boolean;
}

const EMPTY: CustomerFormValue = {
  name: "",
  email: "",
  phone: "",
  status: "active",
  address: null,
  notes: "",
  taxId: "",
  isVatExempt: false,
};

@Component({
  selector: "app-customer-form",
  templateUrl: "./customer-form.html",
  imports: [
    AddressForm,
    FormField,
    FormRoot,
    Grid,
    RouterLink,
    Spinner,
    Stack,
    UiButton,
    UiCheckboxField,
    UiFormField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class CustomerForm {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly tenantService = inject(TenantService);

  protected readonly statusOptions = customerStatusOptions;
  protected readonly allowedCountries = computed(() =>
    regionAllowedCountries(this.tenantService.tenantData()?.settings?.region),
  );

  public readonly mode = input.required<"create" | "edit">();
  public readonly id = input<string>(); // Required for edit mode
  public readonly initial = input<Partial<CustomerFormValue> | null>(null);

  public readonly save = output<CustomerDto>();
  public readonly remove = output<void>();

  protected readonly model = signal<CustomerFormValue>({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.name, { message: "Customer name is required." });
      required(p.status, { message: "Status is required." });
      required(p.address, { message: "Address is required." });
      // Tax ID is mandatory once the billing country is an EU member (reverse-charge B2B).
      required(p.taxId, {
        when: ({ valueOf }) => isEuCountry(valueOf(p.address)?.country),
        message: "Tax ID is required for EU customers.",
      });
    },
    {
      submission: {
        action: async () => {
          const value = this.model();

          if (this.mode() === "create") {
            const command: CreateCustomerCommand = {
              name: value.name,
              email: value.email || null,
              phone: value.phone || null,
              status: value.status,
              address: value.address!,
              notes: value.notes || null,
              taxId: value.taxId || null,
              isVatExempt: value.isVatExempt,
            };

            const result = await this.api.invoke(createCustomer, { body: command });
            if (result) {
              this.toastService.showSuccess("A new customer has been created successfully");
              this.save.emit(result);
            }
          } else {
            const command: UpdateCustomerCommand = {
              id: this.id()!,
              name: value.name,
              email: value.email || null,
              phone: value.phone || null,
              status: value.status,
              address: value.address!,
              notes: value.notes || null,
              taxId: value.taxId || null,
              isVatExempt: value.isVatExempt,
            };
            await this.api.invoke(updateCustomer, { id: this.id()!, body: command });
            this.toastService.showSuccess("Customer data has been updated successfully");
            this.save.emit({ id: this.id()!, name: value.name });
          }

          return undefined;
        },
      },
    },
  );

  /** True when the customer's billing country is an EU member — drives the
   *  Tax-ID required hint + validator. Updates as the address sub-form changes. */
  protected readonly customerIsEu = computed(() => isEuCountry(this.model().address?.country));

  constructor() {
    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this customer?",
      accept: () => this.remove.emit(),
    });
  }

  private patch(src: Partial<CustomerFormValue>): void {
    this.model.update((v) => ({ ...v, ...src }));
  }
}
