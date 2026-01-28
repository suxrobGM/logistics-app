import { Component, effect, inject, input, model, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Api, updateCustomer } from "@logistics/shared/api";
import type {
  Address,
  CustomerDto,
  CustomerStatus,
  UpdateCustomerCommand,
} from "@logistics/shared/api";
import { customerStatusOptions } from "@logistics/shared/api/enums";
import { AddressForm, LabeledField, ValidationSummary } from "@logistics/shared/components";
import { AccordionModule } from "primeng/accordion";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";

@Component({
  selector: "app-customer-edit-dialog",
  templateUrl: "./customer-edit-dialog.html",
  imports: [
    DialogModule,
    ButtonModule,
    ReactiveFormsModule,
    SelectModule,
    InputTextModule,
    TextareaModule,
    AccordionModule,
    LabeledField,
    ValidationSummary,
    AddressForm,
  ],
})
export class CustomerEditDialog {
  private readonly api = inject(Api);

  readonly visible = model<boolean>(false);
  readonly customer = input<CustomerDto | null>(null);
  readonly saved = output<void>();
  readonly deleted = output<void>();

  protected readonly form: FormGroup<UpdateCustomerForm>;
  protected readonly statusOptions = customerStatusOptions;
  protected readonly isLoading = signal(false);

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
    });

    effect(() => {
      const cust = this.customer();
      if (cust && this.visible()) {
        this.populateForm(cust);
      }
    });
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

  private populateForm(cust: CustomerDto): void {
    this.form.patchValue({
      name: cust.name ?? "",
      email: cust.email ?? null,
      phone: cust.phone ?? null,
      status: cust.status ?? "active",
      address: cust.address ?? null,
      notes: cust.notes ?? null,
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
}
