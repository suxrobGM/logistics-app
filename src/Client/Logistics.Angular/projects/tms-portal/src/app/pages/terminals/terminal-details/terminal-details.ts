import { DatePipe } from "@angular/common";
import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import {
  Api,
  deleteTerminal,
  getTerminalById,
  type TerminalDto,
  type TerminalType,
} from "@logistics/shared/api";
import { terminalTypeOptions } from "@logistics/shared/api/enums";
import { Grid, Icon, Stack, Surface, Typography } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";

@Component({
  selector: "app-terminal-details",
  templateUrl: "./terminal-details.html",
  imports: [
    DatePipe,
    RouterLink,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
    PageHeader,
    Grid,
    Icon,
    Stack,
    Surface,
    Typography,
  ],
})
export class TerminalDetails implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly id = input.required<string>();

  protected readonly isLoading = signal(false);
  protected readonly isDeleting = signal(false);
  protected readonly terminal = signal<TerminalDto | null>(null);

  protected readonly title = computed(() => {
    const t = this.terminal();
    if (!t) return "Terminal";
    const code = t.code ?? "";
    const name = t.name ?? "";
    return code && name ? `${code} - ${name}` : code || name || "Terminal";
  });

  ngOnInit(): void {
    this.fetchTerminal();
  }

  protected typeLabel(type?: TerminalType): string {
    return terminalTypeOptions.find((o) => o.value === type)?.label ?? "";
  }

  protected onDelete(): void {
    this.toast.confirm({
      header: "Delete Terminal",
      message: "Are you sure you want to delete this terminal? This action cannot be undone.",
      icon: "pi pi-exclamation-triangle",
      acceptLabel: "Delete",
      rejectLabel: "Cancel",
      acceptButtonStyleClass: "p-button-danger",
      accept: async () => {
        this.isDeleting.set(true);
        try {
          await this.api.invoke(deleteTerminal, { id: this.id() });
          this.toast.showSuccess("Terminal has been deleted successfully");
          this.router.navigateByUrl("/terminals");
        } catch (error: unknown) {
          const message =
            (error as { error?: { error?: string }; message?: string })?.error?.error ??
            (error as { message?: string })?.message ??
            "Failed to delete terminal";
          this.toast.showError(message);
        } finally {
          this.isDeleting.set(false);
        }
      },
    });
  }

  private async fetchTerminal(): Promise<void> {
    this.isLoading.set(true);
    try {
      const terminal = await this.api.invoke(getTerminalById, { id: this.id() });
      if (!terminal) return;
      this.terminal.set(terminal);
    } finally {
      this.isLoading.set(false);
    }
  }
}
