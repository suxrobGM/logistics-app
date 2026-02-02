import { Component, type OnInit, inject, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { AddressForm, PhoneInput } from "@logistics/shared";
import { Api, getTenantById, updateTenant } from "@logistics/shared/api";
import type {
  Address,
  TenantDto,
  TenantSettings,
  UpdateTenantCommand,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { ToastModule } from "primeng/toast";
import { TenantService, ToastService } from "@/core/services";
import { LabeledField, ValidationSummary } from "@/shared/components";

@Component({
  selector: "app-company-settings",
  templateUrl: "./company-settings.html",
  imports: [
    ToastModule,
    CardModule,
    ProgressSpinnerModule,
    ButtonModule,
    ReactiveFormsModule,
    ValidationSummary,
    LabeledField,
    InputTextModule,
    AddressForm,
    PhoneInput,
    SelectModule,
  ],
})
export class CompanySettingsComponent implements OnInit {
  protected readonly form: FormGroup<CompanySettingsForm>;

  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly isUploadingLogo = signal(false);
  protected readonly logoPreviewUrl = signal<string | null>(null);
  protected readonly tenant = signal<TenantDto | null>(null);

  // Regional settings options
  protected readonly distanceUnitOptions = [
    { label: "Miles", value: "miles" },
    { label: "Kilometers", value: "kilometers" },
  ];

  protected readonly weightUnitOptions = [
    { label: "Pounds (lbs)", value: "pounds" },
    { label: "Kilograms (kg)", value: "kilograms" },
  ];

  protected readonly dateFormatOptions = [
    { label: "MM/DD/YYYY (US)", value: "us" },
    { label: "DD/MM/YYYY (European)", value: "european" },
    { label: "YYYY-MM-DD (ISO)", value: "iso" },
  ];

  protected readonly timezoneOptions = [
    { label: "America/New_York (Eastern)", value: "America/New_York" },
    { label: "America/Chicago (Central)", value: "America/Chicago" },
    { label: "America/Denver (Mountain)", value: "America/Denver" },
    { label: "America/Los_Angeles (Pacific)", value: "America/Los_Angeles" },
    { label: "America/Phoenix (Arizona)", value: "America/Phoenix" },
    { label: "America/Anchorage (Alaska)", value: "America/Anchorage" },
    { label: "Pacific/Honolulu (Hawaii)", value: "Pacific/Honolulu" },
    { label: "America/Toronto (Eastern Canada)", value: "America/Toronto" },
    { label: "America/Vancouver (Pacific Canada)", value: "America/Vancouver" },
    { label: "America/Mexico_City (Mexico Central)", value: "America/Mexico_City" },
    { label: "Europe/London (UK)", value: "Europe/London" },
    { label: "Europe/Paris (Central Europe)", value: "Europe/Paris" },
    { label: "Europe/Berlin (Germany)", value: "Europe/Berlin" },
    { label: "Australia/Sydney (Australia Eastern)", value: "Australia/Sydney" },
  ];

  constructor() {
    this.form = new FormGroup<CompanySettingsForm>({
      companyName: new FormControl("", {
        validators: [Validators.required, Validators.maxLength(200)],
        nonNullable: true,
      }),
      phoneNumber: new FormControl<string | null>(null),
      billingEmail: new FormControl("", {
        validators: [Validators.required, Validators.email],
        nonNullable: true,
      }),
      dotNumber: new FormControl("", { nonNullable: true }),
      companyAddress: new FormControl<Address | null>(null, {
        validators: Validators.required,
      }),
      // Regional settings
      distanceUnit: new FormControl("miles", { nonNullable: true }),
      weightUnit: new FormControl("pounds", { nonNullable: true }),
      dateFormat: new FormControl("us", { nonNullable: true }),
      timezone: new FormControl("America/New_York", { nonNullable: true }),
    });
  }

  ngOnInit(): void {
    this.fetchTenantData();
  }

  async save(): Promise<void> {
    if (!this.form.valid) {
      return;
    }

    const tenantId = this.tenantService.getTenantId();
    if (!tenantId) {
      this.toastService.showError("Tenant ID not found");
      return;
    }

    const command: UpdateTenantCommand = {
      companyName: this.form.value.companyName,
      phoneNumber: this.form.value.phoneNumber || null,
      billingEmail: this.form.value.billingEmail,
      dotNumber: this.form.value.dotNumber || null,
      companyAddress: this.form.value.companyAddress ?? undefined,
      settings: {
        distanceUnit: this.form.value.distanceUnit,
        weightUnit: this.form.value.weightUnit,
        dateFormat: this.form.value.dateFormat,
        timezone: this.form.value.timezone,
        currency: "usd",
      } as TenantSettings,
    };

    this.isSaving.set(true);
    try {
      await this.api.invoke(updateTenant, {
        id: tenantId,
        body: command,
      });
      this.toastService.showSuccess("Company settings have been saved successfully");
      this.tenantService.refetchTenantData();
    } catch {
      this.toastService.showError("An error occurred while saving company settings");
    } finally {
      this.isSaving.set(false);
    }
  }

  async onLogoSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    // Validate file type
    if (!file.type.startsWith("image/")) {
      this.toastService.showError("Please select an image file");
      return;
    }

    // Validate file size (5MB max)
    const maxSize = 5 * 1024 * 1024;
    if (file.size > maxSize) {
      this.toastService.showError("File size exceeds 5MB limit");
      return;
    }

    const tenantId = this.tenantService.getTenantId();
    if (!tenantId) {
      this.toastService.showError("Tenant ID not found");
      return;
    }

    this.isUploadingLogo.set(true);
    try {
      const formData = new FormData();
      formData.append("file", file);

      const response = await fetch(`/api/tenants/${tenantId}/logo`, {
        method: "POST",
        body: formData,
        headers: {
          "X-Tenant": tenantId,
        },
        credentials: "include",
      });

      if (!response.ok) {
        throw new Error("Upload failed");
      }

      const logoPath = await response.text();
      this.logoPreviewUrl.set(this.getLogoUrl(logoPath));
      this.toastService.showSuccess("Logo uploaded successfully");
      this.tenantService.refetchTenantData();
    } catch {
      this.toastService.showError("Failed to upload logo");
    } finally {
      this.isUploadingLogo.set(false);
      // Reset input to allow re-selecting the same file
      input.value = "";
    }
  }

  private async fetchTenantData(): Promise<void> {
    const tenantId = this.tenantService.getTenantId();
    if (!tenantId) {
      return;
    }

    this.isLoading.set(true);
    try {
      const tenant = await this.api.invoke(getTenantById, { identifier: tenantId });
      if (tenant) {
        this.tenant.set(tenant);
        this.form.patchValue({
          companyName: tenant.companyName ?? "",
          phoneNumber: tenant.phoneNumber ?? null,
          billingEmail: tenant.billingEmail ?? "",
          dotNumber: tenant.dotNumber ?? "",
          companyAddress: tenant.companyAddress ?? null,
          // Regional settings
          distanceUnit: tenant.settings?.distanceUnit ?? "miles",
          weightUnit: tenant.settings?.weightUnit ?? "pounds",
          dateFormat: tenant.settings?.dateFormat ?? "us",
          timezone: tenant.settings?.timezone ?? "America/New_York",
        });

        if (tenant.logoUrl) {
          this.logoPreviewUrl.set(this.getLogoUrl(tenant.logoUrl));
        }
      }
    } catch {
      this.toastService.showError("Failed to load company settings");
    } finally {
      this.isLoading.set(false);
    }
  }

  private getLogoUrl(logoPath: string): string {
    // If it's already a full URL, return as-is
    if (logoPath.startsWith("http")) {
      return logoPath;
    }
    // Otherwise, construct the URL for local file storage
    const tenantId = this.tenantService.getTenantId();
    return `/uploads/${tenantId}/logos/${logoPath}`;
  }
}

interface CompanySettingsForm {
  companyName: FormControl<string>;
  phoneNumber: FormControl<string | null>;
  billingEmail: FormControl<string>;
  dotNumber: FormControl<string>;
  companyAddress: FormControl<Address | null>;
  // Regional settings
  distanceUnit: FormControl<string>;
  weightUnit: FormControl<string>;
  dateFormat: FormControl<string>;
  timezone: FormControl<string>;
}
