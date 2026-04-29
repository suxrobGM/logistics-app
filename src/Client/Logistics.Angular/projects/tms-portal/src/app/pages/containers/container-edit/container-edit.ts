import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
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
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { Select } from "primeng/select";
import { TagModule } from "primeng/tag";
import { TextareaModule } from "primeng/textarea";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { SearchTerminal } from "@/shared/components/search";

@Component({
  selector: "app-container-edit",
  templateUrl: "./container-edit.html",
  imports: [
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    CardModule,
    CheckboxModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    Select,
    TagModule,
    TextareaModule,
    LabeledField,
    ValidationSummary,
    PageHeader,
    SearchTerminal,
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

  protected readonly form = new FormGroup({
    number: new FormControl("", {
      validators: [
        Validators.required,
        Validators.minLength(11),
        Validators.maxLength(11),
        Validators.pattern(/^[A-Z]{4}\d{7}$/),
      ],
      nonNullable: true,
    }),
    isoType: new FormControl<ContainerIsoType>("gp40", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    sealNumber: new FormControl<string | null>(null),
    bookingReference: new FormControl<string | null>(null),
    billOfLadingNumber: new FormControl<string | null>(null),
    grossWeight: new FormControl<number | null>(null),
    notes: new FormControl<string | null>(null),
  });

  protected readonly statusForm = new FormGroup({
    targetStatus: new FormControl<ContainerStatus | null>(null, {
      validators: [Validators.required],
    }),
    terminal: new FormControl<TerminalDto | null>(null),
  });

  protected readonly terminalForm = new FormGroup({
    terminal: new FormControl<TerminalDto | null>(null, {
      validators: [Validators.required],
    }),
  });

  ngOnInit(): void {
    this.fetchContainer();
  }

  protected statusSeverity(status?: ContainerStatus): "info" | "success" | "warn" | "secondary" {
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
    const target = this.statusForm.controls.targetStatus.value;
    return target === "at_port" || target === "returned";
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    const command: UpdateContainerCommand = {
      id: this.id(),
      number: formValue.number.toUpperCase(),
      isoType: formValue.isoType,
      sealNumber: formValue.sealNumber,
      bookingReference: formValue.bookingReference,
      billOfLadingNumber: formValue.billOfLadingNumber,
      grossWeight: formValue.grossWeight,
      notes: formValue.notes,
    };

    try {
      await this.api.invoke(updateContainer, { id: this.id(), body: command });
      this.toastService.showSuccess("Container has been updated successfully");
      this.router.navigateByUrl("/containers");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected openStatusDialog(): void {
    this.statusForm.reset({ targetStatus: null, terminal: null });
    this.statusDialogVisible.set(true);
  }

  protected async submitStatus(): Promise<void> {
    if (this.statusForm.invalid) return;
    const value = this.statusForm.getRawValue();
    const target = value.targetStatus!;

    if ((target === "at_port" || target === "returned") && !value.terminal?.id) {
      this.toastService.showError("A terminal is required for this status");
      return;
    }

    const command: UpdateContainerStatusCommand = {
      id: this.id(),
      targetStatus: target,
      terminalId: value.terminal?.id ?? null,
    };

    try {
      await this.api.invoke(updateContainerStatus, { id: this.id(), body: command });
      this.toastService.showSuccess("Container status has been updated");
      this.statusDialogVisible.set(false);
      await this.fetchContainer();
    } catch {
      this.toastService.showError("Failed to update container status");
    }
  }

  protected openTerminalDialog(): void {
    this.terminalForm.reset({ terminal: null });
    this.terminalDialogVisible.set(true);
  }

  protected async submitTerminal(): Promise<void> {
    if (this.terminalForm.invalid) return;
    const terminal = this.terminalForm.controls.terminal.value;
    if (!terminal?.id) return;

    try {
      await this.api.invoke(setContainerTerminal, {
        id: this.id(),
        body: { terminalId: terminal.id },
      });
      this.toastService.showSuccess("Container terminal has been updated");
      this.terminalDialogVisible.set(false);
      await this.fetchContainer();
    } catch {
      this.toastService.showError("Failed to update terminal");
    }
  }

  private async fetchContainer(): Promise<void> {
    this.isLoading.set(true);
    try {
      const container = await this.api.invoke(getContainerById, { id: this.id() });
      if (!container) return;
      this.container.set(container);
      this.form.patchValue({
        number: container.number ?? "",
        isoType: container.isoType ?? "gp40",
        sealNumber: container.sealNumber,
        bookingReference: container.bookingReference,
        billOfLadingNumber: container.billOfLadingNumber,
        grossWeight: container.grossWeight ?? null,
        notes: container.notes,
      });
    } finally {
      this.isLoading.set(false);
    }
  }
}
