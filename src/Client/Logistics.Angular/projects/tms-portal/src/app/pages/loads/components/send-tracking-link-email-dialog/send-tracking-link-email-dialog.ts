import { Component, inject, input, model, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Api, sendTrackingLinkEmail } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { TextareaModule } from "primeng/textarea";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-send-tracking-link-email-dialog",
  templateUrl: "./send-tracking-link-email-dialog.html",
  imports: [DialogModule, ButtonModule, ReactiveFormsModule, InputTextModule, TextareaModule],
})
export class SendTrackingLinkEmailDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly trackingLinkId = input.required<string>();
  public readonly visible = model<boolean>(false);
  public readonly sent = output<void>();

  protected readonly isSending = signal(false);

  protected readonly form = new FormGroup({
    email: new FormControl("", [Validators.required, Validators.email]),
    personalMessage: new FormControl(""),
  });

  onShow(): void {
    this.form.reset();
  }

  close(): void {
    this.visible.set(false);
    this.form.reset();
  }

  async send(): Promise<void> {
    if (this.form.invalid) return;

    this.isSending.set(true);
    try {
      await this.api.invoke(sendTrackingLinkEmail, {
        id: this.trackingLinkId(),
        body: {
          email: this.form.value.email!,
          personalMessage: this.form.value.personalMessage || undefined,
        },
      });
      this.toastService.showSuccess("Tracking link sent by email");
      this.sent.emit();
      this.close();
    } catch {
      this.toastService.showError("Failed to send email");
    } finally {
      this.isSending.set(false);
    }
  }
}
