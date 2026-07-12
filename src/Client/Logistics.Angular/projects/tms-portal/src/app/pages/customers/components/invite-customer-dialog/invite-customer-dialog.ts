import { Component, inject, input, model, output, signal } from "@angular/core";
import { disabled, email, form, FormField, FormRoot, required } from "@angular/forms/signals";
import {
  Api,
  createInvitation,
  getCustomers,
  type CreateInvitationCommand,
  type CustomerDto,
} from "@logistics/shared/api";
import {
  Spinner,
  Stack,
  UiButton,
  UiDialog,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { UiFormField } from "@/shared/components";

const EMPTY = { email: "", customerId: "", personalMessage: "" };

@Component({
  selector: "app-invite-customer-dialog",
  templateUrl: "./invite-customer-dialog.html",
  imports: [
    FormField,
    FormRoot,
    Spinner,
    Stack,
    UiButton,
    UiDialog,
    UiFormField,
    UiSelectField,
    UiTextField,
    UiTextareaField,
    ValidatedForm,
  ],
})
export class InviteCustomerDialogComponent {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  // Optional: if provided, the customer is pre-selected and cannot be changed
  public readonly customerId = input<string>();
  public readonly customerName = input<string>();

  public readonly visible = model<boolean>(false);
  public readonly invitationSent = output<void>();

  protected readonly customers = signal<CustomerDto[]>([]);
  protected readonly isLoadingCustomers = signal(false);

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.email, { message: "Email address is required." });
      email(p.email, { message: "Enter a valid email address." });
      required(p.customerId, { message: "Please select a customer." });
      // Pre-selected customers cannot be changed (mirrors the old controls.customerId.disable()).
      disabled(p.customerId, { when: () => !!this.customerId() });
    },
    {
      submission: {
        action: async () => {
          const value = this.model();
          const command: CreateInvitationCommand = {
            email: value.email,
            type: "customer_user",
            tenantRole: "tenant.customer",
            customerId: value.customerId,
            personalMessage: value.personalMessage || undefined,
          };

          try {
            await this.api.invoke(createInvitation, { body: command });
            this.toastService.showSuccess(`Invitation sent to ${value.email}`);
            this.invitationSent.emit();
            this.close();
          } catch {
            this.toastService.showError("Failed to send invitation");
          }
          return undefined;
        },
        onInvalid: () => this.toastService.showError("Please fill in all required fields"),
      },
    },
  );

  onShow(): void {
    const preselectedCustomerId = this.customerId();
    if (preselectedCustomerId) {
      this.model.update((v) => ({ ...v, customerId: preselectedCustomerId }));
    } else {
      this.fetchCustomers();
    }
  }

  close(): void {
    this.visible.set(false);
    this.form().reset({ ...EMPTY });
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
