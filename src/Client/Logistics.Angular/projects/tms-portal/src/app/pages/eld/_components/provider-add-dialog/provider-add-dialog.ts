import { Component, inject, input, model, output } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import {
  type CreateEldProviderConfigurationCommand,
  type EldProviderType,
} from "@logistics/shared/api";
import { Alert } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { PasswordModule } from "primeng/password";
import { SelectModule } from "primeng/select";
import { FormField } from "@/shared/components";
import { ELD_PROVIDER_OPTIONS } from "../eld.constants";

@Component({
  selector: "app-eld-provider-add-dialog",
  templateUrl: "./provider-add-dialog.html",
  imports: [
    Alert,
    ButtonModule,
    DialogModule,
    FormField,
    InputTextModule,
    PasswordModule,
    ReactiveFormsModule,
    SelectModule,
  ],
})
export class EldProviderAddDialog {
  private readonly fb = inject(FormBuilder);

  public readonly visible = model.required<boolean>();
  public readonly saving = input(false);
  public readonly save = output<CreateEldProviderConfigurationCommand>();

  protected readonly providerOptions = ELD_PROVIDER_OPTIONS;

  protected readonly form = this.fb.group({
    providerType: ["demo" as EldProviderType, Validators.required],
    apiKey: ["", Validators.required],
    apiSecret: [""],
    webhookSecret: [""],
  });

  protected onShow(): void {
    this.form.reset({
      providerType: "demo" as EldProviderType,
      apiKey: "",
      apiSecret: "",
      webhookSecret: "",
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }
    const v = this.form.value;
    this.save.emit({
      providerType: v.providerType as EldProviderType,
      apiKey: v.apiKey ?? "",
      apiSecret: v.apiSecret,
      webhookSecret: v.webhookSecret,
    });
  }

  protected close(): void {
    this.visible.set(false);
  }
}
