import {
  Component,
  computed,
  effect,
  inject,
  input,
  model,
  output,
  signal,
  untracked,
} from "@angular/core";
import {
  disabled,
  form,
  FormField,
  FormRoot,
  maxLength,
  minLength,
  pattern,
  required,
} from "@angular/forms/signals";
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
import { ToastService } from "@logistics/shared/services";
import {
  Stack,
  UiButton,
  UiDateField,
  UiDialog,
  UiMultiSelectField,
  UiSelectField,
  UiTextField,
} from "@logistics/shared/ui";
import { UiFormField, ValidatedForm } from "@/shared/components";

interface DriverLicenseModel {
  licenseNumber: string;
  licenseClass: LicenseClass;
  issuingCountry: string;
  issuingRegion: string;
  issuedDate: Date | null;
  expiresAt: Date | null;
  medicalCertExpiresAt: Date | null;
  status: DriverLicenseStatus;
  endorsements: LicenseEndorsement[];
}

const EMPTY: DriverLicenseModel = {
  licenseNumber: "",
  licenseClass: "us_class_a",
  issuingCountry: "US",
  issuingRegion: "",
  issuedDate: null,
  expiresAt: null,
  medicalCertExpiresAt: null,
  status: "active",
  endorsements: [],
};

@Component({
  selector: "app-driver-license-edit-dialog",
  templateUrl: "./driver-license-edit-dialog.html",
  imports: [
    FormField,
    FormRoot,
    Stack,
    UiButton,
    UiDateField,
    UiDialog,
    UiFormField,
    UiMultiSelectField,
    UiSelectField,
    UiTextField,
    ValidatedForm,
  ],
})
export class DriverLicenseEditDialog {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  readonly visible = model<boolean>(false);
  readonly employeeId = input.required<string>();
  readonly license = input<DriverLicenseDto | null>(null);
  readonly saved = output<void>();

  protected readonly classOptions = licenseClassOptions;
  protected readonly statusOptions = driverLicenseStatusOptions;
  protected readonly endorsementOptions = licenseEndorsementOptions;

  protected readonly mode = computed<"create" | "update">(() =>
    this.license() ? "update" : "create",
  );
  protected readonly title = computed(() =>
    this.mode() === "create" ? "Add driver license" : "Edit driver license",
  );

  protected readonly model = signal<DriverLicenseModel>({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.licenseNumber, { message: "License number is required." });
      maxLength(p.licenseNumber, 64, {
        message: "License number cannot exceed 64 characters.",
      });
      // License number is immutable on update — disable the control declaratively.
      disabled(p.licenseNumber, { when: () => this.mode() === "update" });

      required(p.licenseClass, { message: "License class is required." });

      required(p.issuingCountry, { message: "Issuing country is required." });
      minLength(p.issuingCountry, 2, {
        message: "Issuing country must be a 2-letter code.",
      });
      maxLength(p.issuingCountry, 2, {
        message: "Issuing country must be a 2-letter code.",
      });
      pattern(p.issuingCountry, /^[A-Z]{2}$/, {
        message: "Issuing country must be a 2-letter ISO code (e.g. US, DE, PL).",
      });

      required(p.issuedDate, { message: "Issued date is required." });
      required(p.expiresAt, { message: "Expiry date is required." });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
          const endorsements = v.endorsements ?? [];

          try {
            const lic = this.license();
            if (lic?.id) {
              const command: UpdateDriverLicenseCommand = {
                licenseId: lic.id,
                licenseClass: v.licenseClass,
                endorsements,
                issuingRegion: v.issuingRegion || null,
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
                licenseNumber: v.licenseNumber,
                licenseClass: v.licenseClass,
                endorsements,
                issuingCountry: v.issuingCountry,
                issuingRegion: v.issuingRegion || null,
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
          }

          return undefined;
        },
      },
    },
  );

  constructor() {
    effect(() => {
      const lic = this.license();
      if (this.visible()) {
        // Reset untracked: reading `this.form()` reactively here would re-run this effect on every
        // edit and clobber user input. The effect must depend only on `license()` / `visible()`.
        untracked(() => this.form().reset(lic ? this.toModel(lic) : { ...EMPTY }));
      }
    });
  }

  close(): void {
    this.visible.set(false);
  }

  private toModel(lic: DriverLicenseDto): DriverLicenseModel {
    return {
      licenseNumber: lic.licenseNumber ?? "",
      licenseClass: lic.licenseClass ?? "us_class_a",
      issuingCountry: lic.issuingCountry ?? "US",
      issuingRegion: lic.issuingRegion ?? "",
      issuedDate: lic.issuedDate ? new Date(lic.issuedDate) : null,
      expiresAt: lic.expiresAt ? new Date(lic.expiresAt) : null,
      medicalCertExpiresAt: lic.medicalCertExpiresAt ? new Date(lic.medicalCertExpiresAt) : null,
      status: lic.status ?? "active",
      endorsements: lic.endorsements ?? [],
    };
  }
}
