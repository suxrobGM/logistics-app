import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  getTerminalById,
  updateTerminal,
  type UpdateTerminalCommand,
} from "@logistics/shared/api";
import { CardModule } from "primeng/card";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { TerminalForm, type TerminalFormValue } from "../terminal-form/terminal-form";

@Component({
  selector: "app-terminal-edit",
  templateUrl: "./terminal-edit.html",
  imports: [CardModule, TerminalForm, PageHeader],
})
export class TerminalEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  protected readonly id = input.required<string>();

  protected readonly isLoading = signal(false);
  protected readonly initial = signal<Partial<TerminalFormValue> | null>(null);
  protected readonly title = signal<string>("Terminal");

  ngOnInit(): void {
    this.fetch();
  }

  protected async update(formValue: TerminalFormValue): Promise<void> {
    this.isLoading.set(true);
    const command: UpdateTerminalCommand = {
      id: this.id(),
      name: formValue.name,
      code: formValue.code,
      countryCode: formValue.countryCode,
      type: formValue.type,
      address: formValue.address ?? undefined,
      notes: formValue.notes,
    };

    try {
      await this.api.invoke(updateTerminal, { id: this.id(), body: command });
      this.toastService.showSuccess("Terminal has been updated successfully");
      this.router.navigateByUrl("/terminals");
    } finally {
      this.isLoading.set(false);
    }
  }

  private async fetch(): Promise<void> {
    this.isLoading.set(true);
    try {
      const terminal = await this.api.invoke(getTerminalById, { id: this.id() });
      if (!terminal) return;
      this.title.set(`${terminal.code ?? ""} — ${terminal.name ?? ""}`);
      this.initial.set({
        name: terminal.name ?? "",
        code: terminal.code ?? "",
        countryCode: terminal.countryCode ?? "",
        type: terminal.type,
        address: terminal.address ?? null,
        notes: terminal.notes ?? null,
      });
    } finally {
      this.isLoading.set(false);
    }
  }
}
