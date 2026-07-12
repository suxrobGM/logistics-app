import { Component, ElementRef, input, model, output, signal, viewChild } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import {
  type CreateLoadBoardConfigurationCommand,
  type LoadBoardProviderType,
} from "@logistics/shared/api";
import {
  Icon,
  Stack,
  Typography,
  UiButton,
  UiDialog,
  UiPasswordField,
  UiSelectField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { UiFormField } from "@/shared/components";
import { PROVIDER_OPTIONS } from "../loadboard.constants";

const EMPTY = {
  providerType: "demo" as LoadBoardProviderType,
  apiKey: "",
  apiSecret: "",
  companyDotNumber: "",
  companyMcNumber: "",
};

@Component({
  selector: "app-provider-add-dialog",
  templateUrl: "./provider-add-dialog.html",
  imports: [
    FormField,
    FormRoot,
    Icon,
    Stack,
    Typography,
    UiButton,
    UiDialog,
    UiFormField,
    UiPasswordField,
    UiSelectField,
    UiTextField,
    ValidatedForm,
  ],
})
export class ProviderAddDialog {
  public readonly visible = model.required<boolean>();
  public readonly saving = input(false);
  public readonly save = output<CreateLoadBoardConfigurationCommand>();

  protected readonly providerOptions = PROVIDER_OPTIONS;

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
            providerType: v.providerType,
            apiKey: v.apiKey,
            apiSecret: v.apiSecret,
            companyDotNumber: v.companyDotNumber,
            companyMcNumber: v.companyMcNumber,
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
