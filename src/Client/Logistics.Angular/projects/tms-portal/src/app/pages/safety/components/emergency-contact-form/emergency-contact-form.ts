import { Component, effect, inject, input, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import {
  Api,
  type CreateEmergencyContactCommand,
  type EmergencyContactDto,
  type EmergencyContactType,
  type UpdateEmergencyContactCommand,
  createEmergencyContact,
  updateEmergencyContact,
} from "@logistics/shared/api";
import { LabeledField, PhoneInput, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { ToggleSwitchModule } from "primeng/toggleswitch";
import { ToastService } from "@/core/services";

export interface EmergencyContactFormValue {
  name: string;
  contactType: EmergencyContactType;
  phoneNumber: string;
  email: string | null;
  priority: number;
  isActive: boolean;
}

const contactTypeOptions = [
  { label: "Safety Manager", value: "safety_manager" },
  { label: "Dispatcher", value: "dispatcher" },
  { label: "Fleet Manager", value: "fleet_manager" },
  { label: "Emergency Services", value: "emergency_services" },
  { label: "Family Member", value: "family_member" },
  { label: "Insurance", value: "insurance" },
  { label: "Tow Service", value: "tow_service" },
];

@Component({
  selector: "app-emergency-contact-form",
  templateUrl: "./emergency-contact-form.html",
  imports: [
    ButtonModule,
    ValidationSummary,
    ReactiveFormsModule,
    RouterLink,
    ProgressSpinnerModule,
    LabeledField,
    PhoneInput,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    ToggleSwitchModule,
  ],
})
export class EmergencyContactForm {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly contactTypeOptions = contactTypeOptions;

  public readonly mode = input.required<"create" | "edit">();
  public readonly id = input<string>();
  public readonly initial = input<Partial<EmergencyContactFormValue> | null>(null);

  public readonly save = output<EmergencyContactDto>();
  public readonly remove = output<void>();

  protected readonly form = new FormGroup({
    name: new FormControl("", { validators: Validators.required, nonNullable: true }),
    contactType: new FormControl<EmergencyContactType>("safety_manager", {
      validators: Validators.required,
      nonNullable: true,
    }),
    phoneNumber: new FormControl<string | null>(null, { validators: Validators.required }),
    email: new FormControl<string | null>(null, { validators: Validators.email }),
    priority: new FormControl<number>(1, {
      validators: [Validators.required, Validators.min(1), Validators.max(10)],
      nonNullable: true,
    }),
    isActive: new FormControl<boolean>(true, { nonNullable: true }),
  });

  constructor() {
    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    try {
      if (this.mode() === "create") {
        const command: CreateEmergencyContactCommand = {
          name: formValue.name,
          contactType: formValue.contactType,
          phoneNumber: formValue.phoneNumber,
          email: formValue.email,
          priority: formValue.priority,
        };

        const result = await this.api.invoke(createEmergencyContact, { body: command });
        if (result) {
          this.toastService.showSuccess("Emergency contact created successfully");
          this.save.emit(result);
        }
      } else {
        const command: UpdateEmergencyContactCommand = {
          id: this.id()!,
          name: formValue.name,
          contactType: formValue.contactType,
          phoneNumber: formValue.phoneNumber,
          email: formValue.email,
          priority: formValue.priority,
          isActive: formValue.isActive,
        };
        const result = await this.api.invoke(updateEmergencyContact, {
          id: this.id()!,
          body: command,
        });
        if (result) {
          this.toastService.showSuccess("Emergency contact updated successfully");
          this.save.emit(result);
        }
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure you want to delete this emergency contact?",
      accept: () => this.remove.emit(),
    });
  }

  private patch(src: Partial<EmergencyContactFormValue>): void {
    this.form.patchValue({
      ...src,
    });
  }
}
