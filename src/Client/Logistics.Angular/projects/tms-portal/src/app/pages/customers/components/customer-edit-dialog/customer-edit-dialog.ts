import { Component, computed, effect, inject, input, model, output, signal } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { isEuCountry, regionAllowedCountries } from "@logistics/shared";
import {
  Api,
  updateCustomer,
  type Address,
  type CustomerDto,
  type CustomerStatus,
  type UpdateCustomerCommand,
} from "@logistics/shared/api";
import { customerStatusOptions } from "@logistics/shared/api/enums";
import {
  AddressForm,
  Grid,
  Stack,
  UiAccordionImports,
  UiButton,
  UiCheckboxField,
  UiDialog,
  UiFormField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { TenantService } from "@/core/services/tenant.service";

interface UpdateCustomerModel {
  name: string;
  email: string;
  phone: string;
  status: CustomerStatus;
  address: Address | null;
  notes: string;
  taxId: string;
  isVatExempt: boolean;
}

const EMPTY: UpdateCustomerModel = {
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
  selector: "app-customer-edit-dialog",
  templateUrl: "./customer-edit-dialog.html",
  imports: [
    UiAccordionImports,
    AddressForm,
    FormField,
    FormRoot,
    Grid,
    Stack,
    UiButton,
    UiCheckboxField,
    UiDialog,
    UiFormField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class CustomerEditDialog {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);

  protected readonly allowedCountries = computed(() =>
    regionAllowedCountries(this.tenantService.tenantData()?.settings?.region),
  );

  readonly visible = model<boolean>(false);
  readonly customer = input<CustomerDto | null>(null);
  readonly saved = output<void>();
  readonly deleted = output<void>();

  protected readonly statusOptions = customerStatusOptions;
  protected readonly model = signal<UpdateCustomerModel>({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.name, { message: "Customer name is required." });
      required(p.status, { message: "Status is required." });
      // Tax ID is mandatory once the billing country is an EU member (reverse-charge B2B).
      required(p.taxId, {
        when: ({ valueOf }) => isEuCountry(valueOf(p.address)?.country),
        message: "Tax ID is required for EU customers.",
      });
    },
    {
      submission: {
        action: async () => {
          const cust = this.customer();
          if (!cust?.id) {
            return undefined;
          }

          const value = this.model();
          const command: UpdateCustomerCommand = {
            id: cust.id,
            name: value.name,
            email: value.email || null,
            phone: value.phone || null,
            status: value.status,
            address: value.address!,
            notes: value.notes || null,
            taxId: value.taxId || null,
            isVatExempt: value.isVatExempt,
          };

          await this.api.invoke(updateCustomer, { id: cust.id, body: command });
          this.saved.emit();
          return undefined;
        },
      },
    },
  );

  /** True when the customer's billing country is an EU member — drives the
   *  Tax-ID required hint + validator. */
  protected readonly customerIsEu = computed(() => isEuCountry(this.model().address?.country));

  constructor() {
    effect(() => {
      const cust = this.customer();
      if (cust && this.visible()) {
        this.populateForm(cust);
      }
    });
  }

  close(): void {
    this.visible.set(false);
  }

  private populateForm(cust: CustomerDto): void {
    this.model.set({
      name: cust.name ?? "",
      email: cust.email ?? "",
      phone: cust.phone ?? "",
      status: cust.status ?? "active",
      address: cust.address ?? null,
      notes: cust.notes ?? "",
      taxId: cust.taxId ?? "",
      isVatExempt: cust.isVatExempt ?? false,
    });
  }
}
