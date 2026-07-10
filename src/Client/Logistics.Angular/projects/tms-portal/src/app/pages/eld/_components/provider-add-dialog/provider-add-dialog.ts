import { Component, ElementRef, input, model, output, signal, viewChild } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import {
  type CreateEldProviderConfigurationCommand,
  type EldProviderType,
} from "@logistics/shared/api";
import { Alert, ValidatedForm } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { SelectModule } from "primeng/select";
import { UiFormField, UiPasswordField, UiTextField } from "@/shared/components";
import { ELD_PROVIDER_OPTIONS } from "../eld.constants";

const EMPTY = {
  providerType: "demo" as EldProviderType,
  apiKey: "",
  apiSecret: "",
  webhookSecret: "",
};

@Component({
  selector: "app-eld-provider-add-dialog",
  templateUrl: "./provider-add-dialog.html",
  imports: [
    ValidatedForm,
    Alert,
    ButtonModule,
    DialogModule,
    FormRoot,
    FormField,
    FormsModule,
    SelectModule,
    UiFormField,
    UiTextField,
    UiPasswordField,
  ],
})
export class EldProviderAddDialog {
  public readonly visible = model.required<boolean>();
  public readonly saving = input(false);
  public readonly save = output<CreateEldProviderConfigurationCommand>();

  protected readonly providerOptions = ELD_PROVIDER_OPTIONS;

  private readonly formEl = viewChild.required("formEl", { read: ElementRef });

  protected readonly model = signal({ ...EMPTY });

  /**
   * `[formRoot]` runs `submission.action` on submit. It marks the whole tree touched first, skips the
   * action while invalid, and drives `form().submitting()`. The parent owns the async save, so the
   * button stays bound to the `saving` input, and the action just emits the command.
   */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.providerType, { message: "Provider is required." });
      required(p.apiKey, { message: "API key is required." });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
          this.save.emit({
            providerType: v.providerType,
            apiKey: v.apiKey,
            apiSecret: v.apiSecret,
            webhookSecret: v.webhookSecret,
          });
          return undefined;
        },
      },
    },
  );

  protected onShow(): void {
    this.form().reset({ ...EMPTY });
  }

  /** The footer buttons live outside the `<form>`, so submit it imperatively via a real submit event. */
  protected requestSubmit(): void {
    (this.formEl().nativeElement as HTMLFormElement).requestSubmit();
  }

  protected close(): void {
    this.visible.set(false);
  }
}
