import { Component, computed, effect, inject, input, model, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
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
  FormField,
  Grid,
  Stack,
  ValidationSummary,
} from "@logistics/shared/components";
import { AccordionModule } from "primeng/accordion";
import { ButtonModule } from "primeng/button";
import { CheckboxModule } from "primeng/checkbox";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { TenantService } from "@/core/services/tenant.service";

@Component({
  selector: "app-customer-edit-dialog",
  templateUrl: "./customer-edit-dialog.html",
  imports: [
    DialogModule,
    ButtonModule,
    CheckboxModule,
    ReactiveFormsModule,
    SelectModule,
    InputTextModule,
    TextareaModule,
    AccordionModule,
    FormField,
    ValidationSummary,
    AddressForm,
    Grid,
    Stack,
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

  protected readonly form: FormGroup<UpdateCustomerForm>;
  protected readonly statusOptions = customerStatusOptions;
  protected readonly isLoading = signal(false);

  /** True when the customer's billing country is an EU member — drives the
   *  Tax-ID required hint + validator. */
  protected readonly customerIsEu = computed(() =>
    isEuCountry(this.form?.controls.address.value?.country),
  );

  constructor() {
    this.form = new FormGroup<UpdateCustomerForm>({
      name: new FormControl<string>("", {
        validators: Validators.required,
        nonNullable: true,
      }),
      email: new FormControl<string | null>(null),
      phone: new FormControl<string | null>(null),
      status: new FormControl<CustomerStatus>("active", {
        validators: Validators.required,
        nonNullable: true,
      }),
      address: new FormControl<Address | null>(null),
      notes: new FormControl<string | null>(null),
      taxId: new FormControl<string | null>(null),
      isVatExempt: new FormControl<boolean>(false, { nonNullable: true }),
    });

    effect(() => {
      const cust = this.customer();
      if (cust && this.visible()) {
        this.populateForm(cust);
      }
    });

    this.form.controls.address.valueChanges.subscribe(() => this.applyTaxIdValidators());
  }

  async save(): Promise<void> {
    if (!this.form.valid) return;

    const cust = this.customer();
    if (!cust?.id) return;

    const formValue = this.form.getRawValue();

    const command: UpdateCustomerCommand = {
      id: cust.id,
      name: formValue.name,
      email: formValue.email,
      phone: formValue.phone,
      status: formValue.status,
      address: formValue.address!,
      notes: formValue.notes,
      taxId: formValue.taxId,
      isVatExempt: formValue.isVatExempt,
    };

    this.isLoading.set(true);
    try {
      await this.api.invoke(updateCustomer, {
        id: cust.id,
        body: command,
      });
      this.saved.emit();
    } finally {
      this.isLoading.set(false);
    }
  }

  close(): void {
    this.visible.set(false);
  }

  private applyTaxIdValidators(): void {
    const taxId = this.form.controls.taxId;
    const required = isEuCountry(this.form.controls.address.value?.country);
    taxId.setValidators(required ? [Validators.required] : []);
    taxId.updateValueAndValidity({ emitEvent: false });
  }

  private populateForm(cust: CustomerDto): void {
    this.form.patchValue({
      name: cust.name ?? "",
      email: cust.email ?? null,
      phone: cust.phone ?? null,
      status: cust.status ?? "active",
      address: cust.address ?? null,
      notes: cust.notes ?? null,
      taxId: cust.taxId ?? null,
      isVatExempt: cust.isVatExempt ?? false,
    });
  }
}

interface UpdateCustomerForm {
  name: FormControl<string>;
  email: FormControl<string | null>;
  phone: FormControl<string | null>;
  status: FormControl<CustomerStatus>;
  address: FormControl<Address | null>;
  notes: FormControl<string | null>;
  taxId: FormControl<string | null>;
  isVatExempt: FormControl<boolean>;
}
