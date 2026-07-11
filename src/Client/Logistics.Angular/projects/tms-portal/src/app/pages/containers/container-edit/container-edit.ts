import { Component, inject, input, signal, type OnInit } from "@angular/core";
import {
  form,
  FormField,
  FormRoot,
  maxLength,
  min,
  minLength,
  pattern,
  required,
} from "@angular/forms/signals";
import { Router, RouterLink } from "@angular/router";
import {
  Api,
  getContainerById,
  setContainerTerminal,
  updateContainer,
  updateContainerStatus,
  type ContainerDto,
  type ContainerIsoType,
  type ContainerStatus,
  type TerminalDto,
  type UpdateContainerCommand,
  type UpdateContainerStatusCommand,
} from "@logistics/shared/api";
import { containerIsoTypeOptions, containerStatusOptions } from "@logistics/shared/api/enums";
import {
  Badge,
  Card,
  Container,
  Grid,
  Icon,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiDialog,
  UiFormField,
  UiNumberField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { SearchTerminal } from "@/shared/components/search";

interface ContainerFormModel {
  number: string;
  isoType: ContainerIsoType;
  sealNumber: string;
  bookingReference: string;
  billOfLadingNumber: string;
  grossWeight: number | null;
  notes: string;
}

interface StatusFormModel {
  targetStatus: ContainerStatus | null;
  terminal: TerminalDto | null;
}

interface TerminalFormModel {
  terminal: TerminalDto | null;
}

const CONTAINER_EMPTY: ContainerFormModel = {
  number: "",
  isoType: "gp40",
  sealNumber: "",
  bookingReference: "",
  billOfLadingNumber: "",
  grossWeight: null,
  notes: "",
};

const STATUS_EMPTY: StatusFormModel = { targetStatus: null, terminal: null };
const TERMINAL_EMPTY: TerminalFormModel = { terminal: null };

@Component({
  selector: "app-container-edit",
  templateUrl: "./container-edit.html",
  imports: [
    Badge,
    Card,
    Container,
    FormField,
    FormRoot,
    Grid,
    Icon,
    PageHeader,
    RouterLink,
    SearchTerminal,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiDialog,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class ContainerEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  protected readonly id = input.required<string>();

  protected readonly isoTypeOptions = containerIsoTypeOptions;
  protected readonly statusOptions = containerStatusOptions;

  protected readonly isLoading = signal(false);
  protected readonly container = signal<ContainerDto | null>(null);

  protected readonly statusDialogVisible = signal(false);
  protected readonly terminalDialogVisible = signal(false);

  protected readonly model = signal<ContainerFormModel>({ ...CONTAINER_EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.number, { message: "Container number is required." });
      minLength(p.number, 11, {
        message: "Container number must be exactly 11 characters.",
      });
      maxLength(p.number, 11, {
        message: "Container number must be exactly 11 characters.",
      });
      pattern(p.number, /^[A-Z]{4}\d{7}$/, {
        message:
          "Container number must be 4 uppercase letters followed by 7 digits (e.g., MSCU1234567).",
      });
      required(p.isoType, { message: "ISO type is required." });
      min(p.grossWeight, 0, { message: "Gross weight cannot be negative." });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
          const command: UpdateContainerCommand = {
            id: this.id(),
            number: v.number.toUpperCase(),
            isoType: v.isoType,
            sealNumber: v.sealNumber || null,
            bookingReference: v.bookingReference || null,
            billOfLadingNumber: v.billOfLadingNumber || null,
            grossWeight: v.grossWeight,
            notes: v.notes || null,
          };

          try {
            await this.api.invoke(updateContainer, { id: this.id(), body: command });
          } catch {
            this.toastService.showError("Failed to update container");
            return undefined;
          }
          this.toastService.showSuccess("Container has been updated successfully");
          this.router.navigate(["/containers", this.id()]);
          return undefined;
        },
      },
    },
  );

  protected readonly statusModel = signal<StatusFormModel>({ ...STATUS_EMPTY });

  protected readonly statusForm = form(
    this.statusModel,
    (p) => {
      required(p.targetStatus, { message: "Please select a target status." });
      required(p.terminal, {
        when: ({ valueOf }) =>
          valueOf(p.targetStatus) === "at_port" || valueOf(p.targetStatus) === "returned",
        message: "A terminal is required for this status.",
      });
    },
    {
      submission: {
        action: async () => {
          const v = this.statusModel();
          const command: UpdateContainerStatusCommand = {
            id: this.id(),
            targetStatus: v.targetStatus!,
            terminalId: v.terminal?.id ?? null,
          };

          try {
            await this.api.invoke(updateContainerStatus, { id: this.id(), body: command });
          } catch {
            this.toastService.showError("Failed to update container status");
            return undefined;
          }
          this.toastService.showSuccess("Container status has been updated");
          this.statusDialogVisible.set(false);
          await this.fetchContainer();
          return undefined;
        },
      },
    },
  );

  protected readonly terminalModel = signal<TerminalFormModel>({ ...TERMINAL_EMPTY });

  protected readonly terminalForm = form(
    this.terminalModel,
    (p) => {
      required(p.terminal, { message: "Please select a terminal." });
    },
    {
      submission: {
        action: async () => {
          const terminal = this.terminalModel().terminal;
          if (!terminal?.id) {
            return undefined;
          }

          try {
            await this.api.invoke(setContainerTerminal, {
              id: this.id(),
              body: { terminalId: terminal.id },
            });
          } catch {
            this.toastService.showError("Failed to update terminal");
            return undefined;
          }
          this.toastService.showSuccess("Container terminal has been updated");
          this.terminalDialogVisible.set(false);
          await this.fetchContainer();
          return undefined;
        },
      },
    },
  );

  ngOnInit(): void {
    this.fetchContainer();
  }

  protected statusSeverity(status?: ContainerStatus): UiBadgeIntent {
    switch (status) {
      case "loaded":
      case "in_transit":
        return "info";
      case "delivered":
        return "success";
      case "at_port":
      case "returned":
        return "warn";
      default:
        return "secondary";
    }
  }

  protected statusLabel(status?: ContainerStatus): string {
    return containerStatusOptions.find((o) => o.value === status)?.label ?? "";
  }

  protected get statusRequiresTerminal(): boolean {
    const target = this.statusModel().targetStatus;
    return target === "at_port" || target === "returned";
  }

  protected openStatusDialog(): void {
    this.statusForm().reset({ ...STATUS_EMPTY });
    this.statusDialogVisible.set(true);
  }

  protected openTerminalDialog(): void {
    this.terminalForm().reset({ ...TERMINAL_EMPTY });
    this.terminalDialogVisible.set(true);
  }

  private async fetchContainer(): Promise<void> {
    this.isLoading.set(true);
    try {
      const container = await this.api.invoke(getContainerById, { id: this.id() });
      if (!container) return;
      this.container.set(container);
      this.model.set({
        number: container.number ?? "",
        isoType: container.isoType ?? "gp40",
        sealNumber: container.sealNumber ?? "",
        bookingReference: container.bookingReference ?? "",
        billOfLadingNumber: container.billOfLadingNumber ?? "",
        grossWeight: container.grossWeight ?? null,
        notes: container.notes ?? "",
      });
    } finally {
      this.isLoading.set(false);
    }
  }
}
