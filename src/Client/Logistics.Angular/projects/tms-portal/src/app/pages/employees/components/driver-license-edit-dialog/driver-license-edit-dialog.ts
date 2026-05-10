import { Component, computed, effect, inject, input, model, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import {
  Api,
  createDriverLicense,
  updateDriverLicense,
  type CreateDriverLicenseCommand,
  type DriverLicenseDto,
  type DriverLicenseStatus,
  type LicenseClass,
  type LicenseEndorsement,
  type UpdateDriverLicenseCommand,
} from "@logistics/shared/api";
import {
  driverLicenseStatusOptions,
  licenseClassOptions,
  licenseEndorsementOptions,
} from "@logistics/shared/api/enums";
import { Stack } from "@logistics/shared/components";
import { ToastService } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { CheckboxModule } from "primeng/checkbox";
import { DatePickerModule } from "primeng/datepicker";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { FormField, ValidationSummary } from "@/shared/components";

@Component({
  selector: "app-driver-license-edit-dialog",
  templateUrl: "./driver-license-edit-dialog.html",
  imports: [
    DialogModule,
    ButtonModule,
    ReactiveFormsModule,
    InputTextModule,
    SelectModule,
    DatePickerModule,
    CheckboxModule,
    FormField,
    ValidationSummary,
    Stack,
  ],
})
export class DriverLicenseEditDialog {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  readonly visible = model<boolean>(false);
  readonly employeeId = input.required<string>();
  readonly license = input<DriverLicenseDto | null>(null);
  readonly saved = output<void>();

  protected readonly form: FormGroup<DriverLicenseForm>;
  protected readonly classOptions = licenseClassOptions;
  protected readonly statusOptions = driverLicenseStatusOptions;
  protected readonly endorsementOptions = licenseEndorsementOptions;
  protected readonly isLoading = signal(false);

  protected readonly mode = computed<"create" | "update">(() =>
    this.license() ? "update" : "create",
  );
  protected readonly title = computed(() =>
    this.mode() === "create" ? "Add driver license" : "Edit driver license",
  );

  constructor() {
    this.form = new FormGroup<DriverLicenseForm>({
      licenseNumber: new FormControl<string>("", {
        validators: [Validators.required, Validators.maxLength(64)],
        nonNullable: true,
      }),
      licenseClass: new FormControl<LicenseClass>("us_class_a", {
        validators: Validators.required,
        nonNullable: true,
      }),
      issuingCountry: new FormControl<string>("US", {
        validators: [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(2),
          Validators.pattern(/^[A-Z]{2}$/),
        ],
        nonNullable: true,
      }),
      issuingRegion: new FormControl<string | null>(null),
      issuedDate: new FormControl<Date | null>(null, Validators.required),
      expiresAt: new FormControl<Date | null>(null, Validators.required),
      medicalCertExpiresAt: new FormControl<Date | null>(null),
      status: new FormControl<DriverLicenseStatus>("active", { nonNullable: true }),
      endorsements: new FormControl<LicenseEndorsement[]>([], { nonNullable: true }),
    });

    effect(() => {
      const lic = this.license();
      if (this.visible()) {
        if (lic) {
          this.populateForm(lic);
        } else {
          this.form.reset({
            licenseNumber: "",
            licenseClass: "us_class_a",
            issuingCountry: "US",
            issuingRegion: null,
            issuedDate: null,
            expiresAt: null,
            medicalCertExpiresAt: null,
            status: "active",
            endorsements: [],
          });
        }
      }
    });
  }

  async save(): Promise<void> {
    if (!this.form.valid) return;
    const v = this.form.value;
    const endorsements = v.endorsements ?? [];

    this.isLoading.set(true);
    try {
      const lic = this.license();
      if (lic?.id) {
        const command: UpdateDriverLicenseCommand = {
          licenseId: lic.id,
          licenseClass: v.licenseClass,
          endorsements,
          issuingRegion: v.issuingRegion ?? null,
          issuedDate: v.issuedDate?.toISOString(),
          expiresAt: v.expiresAt?.toISOString(),
          medicalCertExpiresAt: v.medicalCertExpiresAt?.toISOString() ?? null,
          status: v.status,
        };
        await this.api.invoke(updateDriverLicense, {
          userId: this.employeeId(),
          licenseId: lic.id,
          body: command,
        });
        this.toast.showSuccess("Driver license updated");
      } else {
        const command: CreateDriverLicenseCommand = {
          employeeId: this.employeeId(),
          licenseNumber: v.licenseNumber!,
          licenseClass: v.licenseClass,
          endorsements,
          issuingCountry: v.issuingCountry!,
          issuingRegion: v.issuingRegion ?? null,
          issuedDate: v.issuedDate!.toISOString(),
          expiresAt: v.expiresAt!.toISOString(),
          medicalCertExpiresAt: v.medicalCertExpiresAt?.toISOString() ?? null,
        };
        await this.api.invoke(createDriverLicense, {
          userId: this.employeeId(),
          body: command,
        });
        this.toast.showSuccess("Driver license added");
      }
      this.saved.emit();
      this.close();
    } catch {
      this.toast.showError("Failed to save driver license");
    } finally {
      this.isLoading.set(false);
    }
  }

  close(): void {
    this.visible.set(false);
  }

  private populateForm(lic: DriverLicenseDto): void {
    this.form.patchValue({
      licenseNumber: lic.licenseNumber ?? "",
      licenseClass: lic.licenseClass ?? "us_class_a",
      issuingCountry: lic.issuingCountry ?? "US",
      issuingRegion: lic.issuingRegion ?? null,
      issuedDate: lic.issuedDate ? new Date(lic.issuedDate) : null,
      expiresAt: lic.expiresAt ? new Date(lic.expiresAt) : null,
      medicalCertExpiresAt: lic.medicalCertExpiresAt ? new Date(lic.medicalCertExpiresAt) : null,
      status: lic.status ?? "active",
      endorsements: lic.endorsements ?? [],
    });

    // License number is immutable on update — disable the control.
    this.form.controls.licenseNumber.disable();
  }
}

interface DriverLicenseForm {
  licenseNumber: FormControl<string>;
  licenseClass: FormControl<LicenseClass>;
  issuingCountry: FormControl<string>;
  issuingRegion: FormControl<string | null>;
  issuedDate: FormControl<Date | null>;
  expiresAt: FormControl<Date | null>;
  medicalCertExpiresAt: FormControl<Date | null>;
  status: FormControl<DriverLicenseStatus>;
  endorsements: FormControl<LicenseEndorsement[]>;
}
