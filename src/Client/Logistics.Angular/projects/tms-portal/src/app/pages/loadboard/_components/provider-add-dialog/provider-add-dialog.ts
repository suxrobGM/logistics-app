import { Component, inject, input, model, output } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import {
  type CreateLoadBoardConfigurationCommand,
  type LoadBoardProviderType,
} from "@logistics/shared/api";
import { Icon, Stack, Typography } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { PasswordModule } from "primeng/password";
import { SelectModule } from "primeng/select";
import { FormField } from "@/shared/components";
import { PROVIDER_OPTIONS } from "../loadboard.constants";

@Component({
  selector: "app-provider-add-dialog",
  templateUrl: "./provider-add-dialog.html",
  imports: [
    ButtonModule,
    DialogModule,
    FormField,
    Icon,
    InputTextModule,
    PasswordModule,
    ReactiveFormsModule,
    SelectModule,
    Stack,
    Typography,
  ],
})
export class ProviderAddDialog {
  private readonly fb = inject(FormBuilder);

  public readonly visible = model.required<boolean>();
  public readonly saving = input(false);
  public readonly save = output<CreateLoadBoardConfigurationCommand>();

  protected readonly providerOptions = PROVIDER_OPTIONS;

  protected readonly form = this.fb.group({
    providerType: ["demo" as LoadBoardProviderType, Validators.required],
    apiKey: ["", Validators.required],
    apiSecret: [""],
    companyDotNumber: [""],
    companyMcNumber: [""],
  });

  protected onShow(): void {
    this.form.reset({
      providerType: "demo" as LoadBoardProviderType,
      apiKey: "",
      apiSecret: "",
      companyDotNumber: "",
      companyMcNumber: "",
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }
    const v = this.form.value;
    this.save.emit({
      providerType: v.providerType as LoadBoardProviderType,
      apiKey: v.apiKey ?? "",
      apiSecret: v.apiSecret,
      companyDotNumber: v.companyDotNumber,
      companyMcNumber: v.companyMcNumber,
    });
  }

  protected close(): void {
    this.visible.set(false);
  }
}
