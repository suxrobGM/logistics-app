import { Component, effect, inject, input, output } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared";
import type { Address } from "@logistics/shared/api";
import { AddressForm, LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";

export interface TenantFormValue {
  name: string;
  companyName: string;
  billingEmail: string;
  dotNumber: string;
  companyAddress: Address | null;
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
    AddressForm,
  ],
})
export class TenantForm {
  private readonly toastService = inject(ToastService);

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
    companyAddress: new FormControl<Address | null>(null),
  });

  constructor() {
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
