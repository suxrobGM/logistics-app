import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import {
  email,
  form,
  FormField,
  FormRoot,
  max,
  maxLength,
  min,
  required,
} from "@angular/forms/signals";
import { AddressForm, PhoneField, regionAllowedCountries } from "@logistics/shared";
import {
  Api,
  getTenantById,
  updateTenant,
  type Address,
  type TenantDto,
  type TenantSettings,
  type UpdateTenantCommand,
} from "@logistics/shared/api";
import {
  Container,
  Icon,
  Spinner,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiNumberField,
  UiSelectField,
  UiTextField,
} from "@logistics/shared/ui";
import { TenantService, ToastService } from "@/core/services";
import { PageHeader, UiFormField, ValidatedForm } from "@/shared/components";

interface CompanySettingsModel {
  companyName: string;
  phoneNumber: string | null;
  billingEmail: string;
  dotNumber: string;
  mcNumber: string;
  vatNumber: string;
  eoriNumber: string;
  companyRegistrationNumber: string;
  taxResidencyCountry: string;
  companyAddress: Address | null;
  // Regional settings
  distanceUnit: string;
  weightUnit: string;
  dateFormat: string;
  timezone: string;
  // Dispatch settings
  minBrokerCreditScore: number | null;
}

const EMPTY: CompanySettingsModel = {
  companyName: "",
  phoneNumber: null,
  billingEmail: "",
  dotNumber: "",
  mcNumber: "",
  vatNumber: "",
  eoriNumber: "",
  companyRegistrationNumber: "",
  taxResidencyCountry: "",
  companyAddress: null,
  distanceUnit: "miles",
  weightUnit: "pounds",
  dateFormat: "us",
  timezone: "America/New_York",
  minBrokerCreditScore: null,
};

@Component({
  selector: "app-company-settings",
  templateUrl: "./company-settings.html",
  imports: [
    AddressForm,
    Container,
    FormField,
    FormRoot,
    Icon,
    PageHeader,
    PhoneField,
    Spinner,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextField,
    ValidatedForm,
  ],
})
export class CompanySettingsComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly isUploadingLogo = signal(false);
  protected readonly logoPreviewUrl = signal<string | null>(null);
  protected readonly tenant = signal<TenantDto | null>(null);
  protected readonly allowedCountries = computed(() =>
    regionAllowedCountries(this.tenantService.tenantData()?.settings?.region),
  );

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

  protected readonly model = signal<CompanySettingsModel>({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.companyName, { message: "Company name is required." });
      maxLength(p.companyName, 200, {
        message: "Company name must be 200 characters or fewer.",
      });
      required(p.billingEmail, { message: "Billing email is required." });
      email(p.billingEmail, { message: "Enter a valid email address." });
      required(p.companyAddress, { message: "Company address is required." });
      min(p.minBrokerCreditScore, 0, { message: "Score must be between 0 and 100." });
      max(p.minBrokerCreditScore, 100, { message: "Score must be between 0 and 100." });
    },
    {
      submission: {
        action: async () => {
          const tenantId = this.tenantService.getTenantId();
          if (!tenantId) {
            this.toastService.showError("Tenant ID not found");
            return undefined;
          }

          const value = this.model();
          const command: UpdateTenantCommand = {
            companyName: value.companyName,
            phoneNumber: value.phoneNumber,
            billingEmail: value.billingEmail,
            dotNumber: value.dotNumber,
            mcNumber: value.mcNumber || null,
            vatNumber: value.vatNumber || null,
            eoriNumber: value.eoriNumber || null,
            companyRegistrationNumber: value.companyRegistrationNumber || null,
            taxResidencyCountry: value.taxResidencyCountry || null,
            companyAddress: value.companyAddress ?? undefined,
            settings: {
              distanceUnit: value.distanceUnit,
              weightUnit: value.weightUnit,
              dateFormat: value.dateFormat,
              timezone: value.timezone,
              currency: "usd",
              minBrokerCreditScore: value.minBrokerCreditScore,
            } as TenantSettings,
          };

          try {
            await this.api.invoke(updateTenant, { id: tenantId, body: command });
            this.toastService.showSuccess("Company settings have been saved successfully");
            this.tenantService.refetchTenantData();
          } catch {
            this.toastService.showError("An error occurred while saving company settings");
          }
          return undefined;
        },
      },
    },
  );

  ngOnInit(): void {
    this.fetchTenantData();
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
        this.model.set({
          companyName: tenant.companyName ?? "",
          phoneNumber: tenant.phoneNumber ?? null,
          billingEmail: tenant.billingEmail ?? "",
          dotNumber: tenant.dotNumber ?? "",
          mcNumber: tenant.mcNumber ?? "",
          vatNumber: tenant.vatNumber ?? "",
          eoriNumber: tenant.eoriNumber ?? "",
          companyRegistrationNumber: tenant.companyRegistrationNumber ?? "",
          taxResidencyCountry: tenant.taxResidencyCountry ?? "",
          companyAddress: tenant.companyAddress ?? null,
          // Regional settings
          distanceUnit: tenant.settings?.distanceUnit ?? "miles",
          weightUnit: tenant.settings?.weightUnit ?? "pounds",
          dateFormat: tenant.settings?.dateFormat ?? "us",
          timezone: tenant.settings?.timezone ?? "America/New_York",
          minBrokerCreditScore: tenant.settings?.minBrokerCreditScore ?? null,
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
