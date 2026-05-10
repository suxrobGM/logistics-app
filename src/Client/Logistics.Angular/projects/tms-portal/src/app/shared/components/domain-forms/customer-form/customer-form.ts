import { Component, computed, effect, inject, input, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
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
  FormField,
  Grid,
  Stack,
  ValidationSummary,
} from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CheckboxModule } from "primeng/checkbox";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { ToastService } from "@/core/services";
import { TenantService } from "@/core/services/tenant.service";

export interface CustomerFormValue {
  name: string;
  email: string | null;
  phone: string | null;
  status: CustomerStatus;
  address: Address | null;
  notes: string | null;
  taxId: string | null;
  isVatExempt: boolean;
}

@Component({
  selector: "app-customer-form",
  templateUrl: "./customer-form.html",
  imports: [
    ButtonModule,
    CheckboxModule,
    ValidationSummary,
    ReactiveFormsModule,
    RouterLink,
    ProgressSpinnerModule,
    FormField,
    Grid,
    Stack,
    InputTextModule,
    TextareaModule,
    SelectModule,
    AddressForm,
  ],
})
export class CustomerForm {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly tenantService = inject(TenantService);

  protected readonly isLoading = signal(false);
  protected readonly statusOptions = customerStatusOptions;
  protected readonly allowedCountries = computed(() =>
    regionAllowedCountries(this.tenantService.tenantData()?.settings?.region),
  );

  public readonly mode = input.required<"create" | "edit">();
  public readonly id = input<string>(); // Required for edit mode
  public readonly initial = input<Partial<CustomerFormValue> | null>(null);

  public readonly save = output<CustomerDto>();
  public readonly remove = output<void>();

  protected readonly form = new FormGroup({
    name: new FormControl("", { validators: Validators.required, nonNullable: true }),
    email: new FormControl<string | null>(null),
    phone: new FormControl<string | null>(null),
    status: new FormControl<CustomerStatus>("active", {
      validators: Validators.required,
      nonNullable: true,
    }),
    address: new FormControl<Address | null>(null, { validators: Validators.required }),
    notes: new FormControl<string | null>(null),
    taxId: new FormControl<string | null>(null),
    isVatExempt: new FormControl<boolean>(false, { nonNullable: true }),
  });

  /** True when the customer's billing country is an EU member — drives the
   *  Tax-ID required hint + validator. Updates as the address sub-form changes. */
  protected readonly customerIsEu = computed(() =>
    isEuCountry(this.form.controls.address.value?.country),
  );

  constructor() {
    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });

    // Re-evaluate the Tax-ID required validator whenever the country changes.
    this.form.controls.address.valueChanges.subscribe(() => this.applyTaxIdValidators());
  }

  private applyTaxIdValidators(): void {
    const taxId = this.form.controls.taxId;
    const required = isEuCountry(this.form.controls.address.value?.country);
    taxId.setValidators(required ? [Validators.required] : []);
    taxId.updateValueAndValidity({ emitEvent: false });
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    if (this.mode() === "create") {
      const command: CreateCustomerCommand = {
        name: formValue.name,
        email: formValue.email,
        phone: formValue.phone,
        status: formValue.status,
        address: formValue.address!,
        notes: formValue.notes,
        taxId: formValue.taxId,
        isVatExempt: formValue.isVatExempt,
      };

      const result = await this.api.invoke(createCustomer, { body: command });
      if (result) {
        this.toastService.showSuccess("A new customer has been created successfully");
        this.save.emit(result);
      }
    } else {
      const command: UpdateCustomerCommand = {
        id: this.id()!,
        name: formValue.name,
        email: formValue.email,
        phone: formValue.phone,
        status: formValue.status,
        address: formValue.address!,
        notes: formValue.notes,
        taxId: formValue.taxId,
        isVatExempt: formValue.isVatExempt,
      };
      await this.api.invoke(updateCustomer, { id: this.id()!, body: command });
      this.toastService.showSuccess("Customer data has been updated successfully");
      this.save.emit({ id: this.id()!, name: formValue.name });
    }

    this.isLoading.set(false);
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this customer?",
      accept: () => this.remove.emit(),
    });
  }

  private patch(src: Partial<CustomerFormValue>): void {
    this.form.patchValue({
      ...src,
    });
  }
}
