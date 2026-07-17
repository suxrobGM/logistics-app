import { Component, ElementRef, input, model, output, signal, viewChild } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import {
  type CreateFuelCardProviderConfigurationCommand,
  type FuelCardProviderType,
} from "@logistics/shared/api";
import { Alert, UiButton, UiDialog, UiSelectField, ValidatedForm } from "@logistics/shared/ui";
import { UiFormField, UiPasswordField, UiTextField } from "@/shared/components";
import { FUEL_CARD_PROVIDER_OPTIONS } from "../fuel-cards.constants";

// `providerType` is nullable because `ui-select-field` is a `FormValueControl<T | null>` and
// `[formField]` value types are invariant — a non-nullable `FuelCardProviderType` would not bind.
const EMPTY = {
  providerType: "demo" as FuelCardProviderType | null,
  apiKey: "",
  apiSecret: "",
  externalAccountId: "",
};

@Component({
  selector: "app-fuel-card-provider-add-dialog",
  templateUrl: "./provider-add-dialog.html",
  imports: [
    Alert,
    FormField,
    FormRoot,
    UiButton,
    UiDialog,
    UiFormField,
    UiPasswordField,
    UiSelectField,
    UiTextField,
    ValidatedForm,
  ],
})
export class FuelCardProviderAddDialog {
  public readonly visible = model.required<boolean>();
  public readonly saving = input(false);
  public readonly save = output<CreateFuelCardProviderConfigurationCommand>();

  protected readonly providerOptions = FUEL_CARD_PROVIDER_OPTIONS;

  private readonly formEl = viewChild.required("formEl", { read: ElementRef });

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
          this.save.emit({
            providerType: v.providerType as FuelCardProviderType,
            apiKey: v.apiKey,
            apiSecret: v.apiSecret || null,
            externalAccountId: v.externalAccountId || null,
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
