import { DatePipe } from "@angular/common";
import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { ProductLicenseService, ToastService } from "@logistics/shared";
import {
  Api,
  getProductLicenseStatus,
  setProductLicenseKey,
  type ProductLicenseStatusDto,
  type ProductLicenseTier,
} from "@logistics/shared/api";
import {
  Alert,
  Badge,
  Card,
  Grid,
  PageHeader,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiFormField,
  UiTextareaField,
  ValidatedForm,
} from "@logistics/shared/ui";

const TIER_LABELS: Record<ProductLicenseTier, string> = {
  internal_use: "Internal use",
  hosted: "Hosted / reseller",
  perpetual_source: "Perpetual source",
};

@Component({
  selector: "adm-product-license",
  templateUrl: "./product-license.html",
  imports: [
    Alert,
    Badge,
    Card,
    DatePipe,
    FormField,
    FormRoot,
    Grid,
    PageHeader,
    Spinner,
    Stack,
    Typography,
    UiButton,
    UiFormField,
    UiTextareaField,
    ValidatedForm,
  ],
})
export class ProductLicense implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly license = inject(ProductLicenseService);

  protected readonly isLoading = signal(true);
  protected readonly status = signal<ProductLicenseStatusDto | null>(null);

  protected readonly keyManagedByConfig = computed(() => this.status()?.source === "configuration");

  protected readonly tierLabel = computed(() => {
    const tier = this.status()?.tier;
    return tier ? TIER_LABELS[tier] : null;
  });

  protected readonly model = signal({ key: "" });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.key, { message: "Paste the license key you received." });
    },
    {
      submission: {
        action: async () => {
          try {
            const status = await this.api.invoke(setProductLicenseKey, {
              body: { key: this.model().key },
            });
            this.status.set(status);
            this.model.set({ key: "" });
            await this.license.refresh();
            this.toastService.showSuccess("License key installed");
          } catch (err) {
            this.toastService.showError(this.describe(err, "Failed to install the license key"));
          }
          return undefined;
        },
      },
    },
  );

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      this.status.set(await this.api.invoke(getProductLicenseStatus));
    } catch {
      this.toastService.showError("Failed to load the license status");
    } finally {
      this.isLoading.set(false);
    }
  }

  /** The API answers a rejected key with the reason in `error`; fall back to a generic line. */
  private describe(err: unknown, fallback: string): string {
    const message = (err as { error?: { error?: string } } | null)?.error?.error;
    return message ?? fallback;
  }
}
