import { Component, effect, inject, input, output } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastService } from "@/core/services";
import { FormField } from "../form-field/form-field";
import { ValidationSummary } from "../validation-summary/validation-summary";

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
    FormField,
    InputTextModule,
  ],
})
export class CustomerForm {
  private readonly toastService = inject(ToastService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<CustomerFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<CustomerFormValue>();
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

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.save.emit(this.form.getRawValue() as CustomerFormValue);
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
