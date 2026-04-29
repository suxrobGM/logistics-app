import { Component, effect, inject, input, output } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared";
import type { Address, Region } from "@logistics/shared/api";
import { regionOptions } from "@logistics/shared/api/enums";
import { AddressForm, LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { Select } from "primeng/select";

export interface TenantFormValue {
  name: string;
  companyName: string;
  billingEmail: string;
  dotNumber: string;
  companyAddress: Address;
  ownerEmail: string;
  ownerFirstName: string;
  ownerLastName: string;
  region: Region;
}

@Component({
  selector: "adm-tenant-form",
  templateUrl: "./tenant-form.html",
  imports: [
    ButtonModule,
    ValidationSummary,
    ReactiveFormsModule,
    RouterLink,
    ProgressSpinnerModule,
    LabeledField,
    InputTextModule,
    Select,
    AddressForm,
  ],
})
export class TenantForm {
  private readonly toastService = inject(ToastService);
  protected readonly regionOptions = regionOptions;

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<TenantFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<TenantFormValue>();
  public readonly remove = output<void>();

  protected readonly form = new FormGroup({
    name: new FormControl("", { validators: Validators.required, nonNullable: true }),
    companyName: new FormControl("", { validators: Validators.required, nonNullable: true }),
    billingEmail: new FormControl("", {
      validators: [Validators.required, Validators.email],
      nonNullable: true,
    }),
    dotNumber: new FormControl("", { nonNullable: true }),
    region: new FormControl<Region>("us", {
      validators: Validators.required,
      nonNullable: true,
    }),
    companyAddress: new FormControl<Address | null>(null, { validators: Validators.required }),
    ownerFirstName: new FormControl("", { nonNullable: true }),
    ownerLastName: new FormControl("", { nonNullable: true }),
    ownerEmail: new FormControl("", { nonNullable: true }),
  });

  constructor() {
    // Set owner field validators based on mode
    effect(() => {
      if (this.mode() === "create") {
        this.form.controls.ownerFirstName.setValidators(Validators.required);
        this.form.controls.ownerLastName.setValidators(Validators.required);
        this.form.controls.ownerEmail.setValidators([Validators.required, Validators.email]);
      } else {
        this.form.controls.ownerFirstName.clearValidators();
        this.form.controls.ownerLastName.clearValidators();
        this.form.controls.ownerEmail.clearValidators();
      }

      this.form.controls.ownerFirstName.updateValueAndValidity();
      this.form.controls.ownerLastName.updateValueAndValidity();
      this.form.controls.ownerEmail.updateValueAndValidity();
    });

    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.save.emit(this.form.getRawValue() as TenantFormValue);
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this tenant? This action cannot be undone.",
      header: "Confirm Delete",
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => this.remove.emit(),
    });
  }

  private patch(src: Partial<TenantFormValue>): void {
    this.form.patchValue({
      ...src,
    });
  }
}
