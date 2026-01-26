import { Component, inject, input, model, output, signal } from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { Api, createInvitation, getCustomers } from "@logistics/shared/api";
import type { CreateInvitationCommand, CustomerDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { ToastService } from "@/core/services";
import { LabeledField } from "@/shared/components";

@Component({
  selector: "app-invite-customer-dialog",
  templateUrl: "./invite-customer-dialog.html",
  imports: [
    DialogModule,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    SelectModule,
    InputTextModule,
    TextareaModule,
    LabeledField,
  ],
})
export class InviteCustomerDialogComponent {
  protected readonly form = new FormGroup({
    email: new FormControl("", [Validators.required, Validators.email]),
    customerId: new FormControl("", Validators.required),
    personalMessage: new FormControl(""),
  });

  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  // Optional: if provided, the customer is pre-selected and cannot be changed
  public readonly customerId = input<string>();
  public readonly customerName = input<string>();

  public readonly visible = model<boolean>(false);
  public readonly invitationSent = output<void>();

  protected readonly customers = signal<CustomerDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isLoadingCustomers = signal(false);

  onShow(): void {
    const preselectedCustomerId = this.customerId();
    if (preselectedCustomerId) {
      this.form.patchValue({ customerId: preselectedCustomerId });
      this.form.controls.customerId.disable();
    } else {
      this.form.controls.customerId.enable();
      this.fetchCustomers();
    }
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.toastService.showError("Please fill in all required fields");
      return;
    }

    const formValue = this.form.getRawValue();

    const command: CreateInvitationCommand = {
      email: formValue.email,
      type: "customer_user",
      tenantRole: "tenant.customer",
      customerId: formValue.customerId,
      personalMessage: formValue.personalMessage || undefined,
    };

    this.isLoading.set(true);
    try {
      await this.api.invoke(createInvitation, { body: command });
      this.toastService.showSuccess(`Invitation sent to ${formValue.email}`);
      this.invitationSent.emit();
      this.close();
    } catch {
      this.toastService.showError("Failed to send invitation");
    } finally {
      this.isLoading.set(false);
    }
  }

  close(): void {
    this.visible.set(false);
    this.form.reset();
  }

  private async fetchCustomers(): Promise<void> {
    this.isLoadingCustomers.set(true);
    try {
      const result = await this.api.invoke(getCustomers, { PageSize: 100 });
      if (result.items) {
        this.customers.set(result.items);
      }
    } catch {
      this.toastService.showError("Failed to load customers");
    } finally {
      this.isLoadingCustomers.set(false);
    }
  }
}
