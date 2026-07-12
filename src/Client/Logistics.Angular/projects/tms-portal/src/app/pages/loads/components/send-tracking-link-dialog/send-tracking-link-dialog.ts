import { Component, inject, input, model, output, signal } from "@angular/core";
import { email, form, FormField, FormRoot, required } from "@angular/forms/signals";
import { UiFormField, UiTextareaField, UiTextField, ValidatedForm } from "@logistics/shared";
import { Api, sendTrackingLinkEmail } from "@logistics/shared/api";
import { UiButton, UiDialog } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";

const EMPTY = { email: "", personalMessage: "" };

@Component({
  selector: "app-send-tracking-link-dialog",
  templateUrl: "./send-tracking-link-dialog.html",
  imports: [
    FormField,
    FormRoot,
    UiButton,
    UiDialog,
    UiFormField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class SendTrackingLinkDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly trackingLinkId = input.required<string>();
  public readonly visible = model<boolean>(false);
  public readonly sent = output<void>();

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.email, { message: "Email address is required." });
      email(p.email, { message: "Enter a valid email address." });
    },
    {
      submission: {
        action: async () => {
          try {
            await this.api.invoke(sendTrackingLinkEmail, {
              id: this.trackingLinkId(),
              body: {
                email: this.model().email,
                personalMessage: this.model().personalMessage || undefined,
              },
            });
          } catch {
            this.toastService.showError("Failed to send email");
            return undefined;
          }
          this.toastService.showSuccess("Tracking link sent by email");
          this.sent.emit();
          this.close();
          return undefined;
        },
      },
    },
  );

  onShow(): void {
    this.form().reset({ ...EMPTY });
  }

  close(): void {
    this.visible.set(false);
    this.form().reset({ ...EMPTY });
  }
}
