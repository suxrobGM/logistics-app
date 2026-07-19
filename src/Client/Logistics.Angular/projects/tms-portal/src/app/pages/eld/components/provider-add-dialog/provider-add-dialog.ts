import { Component, input, model, output, signal } from "@angular/core";
import { form, FormField, required } from "@angular/forms/signals";
import {
  type CreateEldProviderConfigurationCommand,
  type EldProviderType,
} from "@logistics/shared/api";
import { Alert, UiSelectField } from "@logistics/shared/ui";
import {
  ProviderConnectDialog,
  UiFormField,
  UiPasswordField,
  UiTextField,
} from "@/shared/components";
import { ELD_PROVIDER_OPTIONS } from "../eld.constants";

// `providerType` is nullable because `ui-select-field` is a `FormValueControl<T | null>` and
// `[formField]` value types are invariant - a non-nullable `EldProviderType` would not bind.
const EMPTY = {
  providerType: "demo" as EldProviderType | null,
  apiKey: "",
  apiSecret: "",
  webhookSecret: "",
};

@Component({
  selector: "app-eld-provider-add-dialog",
  templateUrl: "./provider-add-dialog.html",
  imports: [
    Alert,
    FormField,
    ProviderConnectDialog,
    UiFormField,
    UiPasswordField,
    UiSelectField,
    UiTextField,
  ],
})
export class EldProviderAddDialog {
  public readonly visible = model.required<boolean>();
  public readonly saving = input(false);
  public readonly save = output<CreateEldProviderConfigurationCommand>();

  protected readonly providerOptions = ELD_PROVIDER_OPTIONS;

  protected readonly model = signal({ ...EMPTY });

  /** The parent owns the async save: the action just emits the command; the button binds `saving`. */
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
          // `required(p.providerType)` above means the action only runs with a provider chosen.
          this.save.emit({
            providerType: v.providerType as EldProviderType,
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
}
