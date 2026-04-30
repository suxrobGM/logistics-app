import { Component, inject, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import {
  Api,
  createContainer,
  type ContainerIsoType,
  type CreateContainerCommand,
  type TerminalDto,
} from "@logistics/shared/api";
import { containerIsoTypeOptions } from "@logistics/shared/api/enums";
import {
  Container,
  Grid,
  Icon,
  LabeledField,
  Stack,
  Surface,
  Typography,
  ValidationSummary,
} from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { Select } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { SearchTerminal } from "@/shared/components/search";

@Component({
  selector: "app-container-add",
  templateUrl: "./container-add.html",
  imports: [
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    CardModule,
    CheckboxModule,
    InputTextModule,
    InputNumberModule,
    Select,
    TextareaModule,
    LabeledField,
    ValidationSummary,
    PageHeader,
    SearchTerminal,
    Container,
    Grid,
    Icon,
    Stack,
    Surface,
    Typography,
  ],
})
export class ContainerAdd {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  protected readonly isoTypeOptions = containerIsoTypeOptions;
  protected readonly isLoading = signal(false);

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
    grossWeight: new FormControl<number>(0, { nonNullable: true }),
    isLaden: new FormControl<boolean>(false, { nonNullable: true }),
    currentTerminal: new FormControl<TerminalDto | null>(null),
    notes: new FormControl<string | null>(null),
  });

  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    const command: CreateContainerCommand = {
      number: formValue.number.toUpperCase(),
      isoType: formValue.isoType,
      sealNumber: formValue.sealNumber,
      bookingReference: formValue.bookingReference,
      billOfLadingNumber: formValue.billOfLadingNumber,
      grossWeight: formValue.grossWeight,
      isLaden: formValue.isLaden,
      currentTerminalId: formValue.currentTerminal?.id ?? null,
      notes: formValue.notes,
    };

    try {
      await this.api.invoke(createContainer, { body: command });
      this.toastService.showSuccess("A new container has been created successfully");
      this.router.navigateByUrl("/containers");
    } finally {
      this.isLoading.set(false);
    }
  }
}
