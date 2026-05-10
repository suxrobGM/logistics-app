import { CommonModule, DatePipe } from "@angular/common";
import { Component, computed, effect, inject, input, signal } from "@angular/core";
import {
  Api,
  deleteDriverLicense,
  getDriverLicenses,
  type DriverLicenseDto,
} from "@logistics/shared/api";
import {
  driverLicenseStatusOptions,
  licenseClassOptions,
  licenseEndorsementOptions,
} from "@logistics/shared/api/enums";
import { Stack, Typography } from "@logistics/shared/components";
import { ToastService } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { ChipModule } from "primeng/chip";
import { TableModule } from "primeng/table";
import { Tag as PrimeTag, TagModule } from "primeng/tag";
import { DriverLicenseEditDialog } from "../driver-license-edit-dialog/driver-license-edit-dialog";

interface LicenseRowVm {
  license: DriverLicenseDto;
  classLabel: string;
  endorsementLabels: string[];
  expirySeverity: PrimeTag["severity"];
  expiryLabel: string;
}

@Component({
  selector: "app-driver-licenses-tab",
  templateUrl: "./driver-licenses-tab.html",
  imports: [
    CommonModule,
    DatePipe,
    TableModule,
    TagModule,
    ChipModule,
    ButtonModule,
    DriverLicenseEditDialog,
    Stack,
    Typography,
  ],
})
export class DriverLicensesTab {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  public readonly employeeId = input.required<string>();

  protected readonly licenses = signal<DriverLicenseDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly editDialogVisible = signal(false);
  protected readonly editingLicense = signal<DriverLicenseDto | null>(null);

  protected readonly rows = computed<LicenseRowVm[]>(() =>
    this.licenses().map((license) => this.toRow(license)),
  );

  protected readonly expiringSoonCount = computed(
    () => this.rows().filter((r) => r.expirySeverity !== "success").length,
  );

  constructor() {
    effect(() => {
      const id = this.employeeId();
      if (id) {
        void this.refresh();
      }
    });
  }

  async refresh(): Promise<void> {
    const id = this.employeeId();
    if (!id) return;

    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getDriverLicenses, {
        userId: id,
        IncludeRevoked: false,
      });
      this.licenses.set(result ?? []);
    } finally {
      this.isLoading.set(false);
    }
  }

  openCreateDialog(): void {
    this.editingLicense.set(null);
    this.editDialogVisible.set(true);
  }

  openEditDialog(license: DriverLicenseDto): void {
    this.editingLicense.set(license);
    this.editDialogVisible.set(true);
  }

  onSaved(): void {
    this.editDialogVisible.set(false);
    void this.refresh();
  }

  onDelete(license: DriverLicenseDto): void {
    if (!license.id) return;
    this.toast.confirm({
      header: "Revoke driver license",
      message:
        `Revoke license ${license.licenseNumber}? It will remain in history but won't count ` +
        `for dispatch eligibility.`,
      acceptLabel: "Revoke",
      rejectLabel: "Cancel",
      acceptButtonStyleClass: "p-button-danger",
      accept: async () => {
        try {
          await this.api.invoke(deleteDriverLicense, {
            userId: this.employeeId(),
            licenseId: license.id!,
          });
          this.toast.showSuccess("License revoked");
          await this.refresh();
        } catch {
          this.toast.showError("Failed to revoke license");
        }
      },
    });
  }

  private toRow(license: DriverLicenseDto): LicenseRowVm {
    const classLabel =
      licenseClassOptions.find((o) => o.value === license.licenseClass)?.label ??
      license.licenseClass ??
      "";

    const endorsements = new Set(license.endorsements ?? []);
    const endorsementLabels = licenseEndorsementOptions
      .filter((opt) => endorsements.has(opt.value))
      .map((opt) => opt.label);

    const status = license.status;
    const days = license.daysUntilExpiry ?? 0;

    let expirySeverity: PrimeTag["severity"];
    let expiryLabel: string;

    if (status === "revoked" || status === "suspended") {
      expirySeverity = "danger";
      expiryLabel = driverLicenseStatusOptions.find((o) => o.value === status)?.label ?? "Inactive";
    } else if (status === "expired" || days < 0) {
      expirySeverity = "danger";
      expiryLabel = "Expired";
    } else if (days <= 7) {
      expirySeverity = "danger";
      expiryLabel = `Expires in ${days}d`;
    } else if (days <= 60) {
      expirySeverity = "warn";
      expiryLabel = `Expires in ${days}d`;
    } else {
      expirySeverity = "success";
      expiryLabel = `Active`;
    }

    return { license, classLabel, endorsementLabels, expirySeverity, expiryLabel };
  }
}
