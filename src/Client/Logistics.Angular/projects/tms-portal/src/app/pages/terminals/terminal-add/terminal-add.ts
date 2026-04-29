import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Api, createTerminal, type CreateTerminalCommand } from "@logistics/shared/api";
import { CardModule } from "primeng/card";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { TerminalForm, type TerminalFormValue } from "../terminal-form/terminal-form";

@Component({
  selector: "app-terminal-add",
  templateUrl: "./terminal-add.html",
  imports: [CardModule, TerminalForm, PageHeader],
})
export class TerminalAdd {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);

  protected async create(formValue: TerminalFormValue): Promise<void> {
    this.isLoading.set(true);
    const command: CreateTerminalCommand = {
      name: formValue.name,
      code: formValue.code,
      countryCode: formValue.countryCode,
      type: formValue.type,
      address: formValue.address ?? undefined,
      notes: formValue.notes,
    };

    try {
      await this.api.invoke(createTerminal, { body: command });
      this.toastService.showSuccess("A new terminal has been created successfully");
      this.router.navigateByUrl("/terminals");
    } finally {
      this.isLoading.set(false);
    }
  }
}
