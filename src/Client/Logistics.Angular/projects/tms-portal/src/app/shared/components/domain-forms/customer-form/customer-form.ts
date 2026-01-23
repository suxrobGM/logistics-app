import { Component, effect, inject, input, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { Api, createCustomer, updateCustomer } from "@logistics/shared/api";
import type { CustomerDto, UpdateCustomerCommand } from "@logistics/shared/api/models";
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastService } from "@/core/services";

export interface CustomerFormValue {
  name: string;
}

@Component({
  selector: "app-customer-form",
  templateUrl: "./customer-form.html",
  imports: [
    ButtonModule,
    ValidationSummary,
    ReactiveFormsModule,
    RouterLink,
    ProgressSpinnerModule,
    LabeledField,
    InputTextModule,
  ],
})
export class CustomerForm {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);

  public readonly mode = input.required<"create" | "edit">();
  public readonly id = input<string>(); // Required for edit mode
  public readonly initial = input<Partial<CustomerFormValue> | null>(null);

  public readonly save = output<CustomerDto>();
  public readonly remove = output<void>();

  protected readonly form = new FormGroup({
    name: new FormControl("", { validators: Validators.required, nonNullable: true }),
  });

  constructor() {
    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue() as CustomerFormValue;

    if (this.mode() === "create") {
      const result = await this.api.invoke(createCustomer, { body: formValue });
      if (result) {
        this.toastService.showSuccess("A new customer has been created successfully");
        this.save.emit(result);
      }
    } else {
      const command: UpdateCustomerCommand = {
        id: this.id()!,
        name: formValue.name,
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
