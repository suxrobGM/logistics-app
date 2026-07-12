import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { disabled, form, FormField, FormRoot, max, min, required } from "@angular/forms/signals";
import {
  Api,
  createTenantTaxRate,
  deleteTenantTaxRate,
  getTaxJurisdictions,
  getTenantTaxRates,
  updateTenantTaxRate,
  type CreateTenantTaxRateCommand,
  type TaxJurisdictionInfoDto,
  type TenantTaxRateDto,
  type UpdateTenantTaxRateCommand,
} from "@logistics/shared/api";
import { DateFormatPipe } from "@logistics/shared/pipes";
import {
  Alert,
  Card,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiDataTable,
  UiDateField,
  UiDialog,
  UiFormField,
  UiNumberField,
  UiSelectField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";

interface TaxRateModel {
  jurisdictionKey: string | null;
  ratePercent: number | null;
  description: string;
  effectiveFrom: Date | null;
  effectiveTo: Date | null;
}

const EMPTY: TaxRateModel = {
  jurisdictionKey: null,
  ratePercent: null,
  description: "",
  effectiveFrom: null,
  effectiveTo: null,
};

@Component({
  selector: "app-tenant-tax-rates-card",
  templateUrl: "./tenant-tax-rates-card.html",
  imports: [
    Alert,
    Card,
    DateFormatPipe,
    FormField,
    FormRoot,
    Spinner,
    Stack,
    Typography,
    UiButton,
    UiDataTable,
    UiDateField,
    UiDialog,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextField,
    ValidatedForm,
  ],
})
export class TenantTaxRatesCard implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly rates = signal<TenantTaxRateDto[]>([]);
  protected readonly jurisdictions = signal<TaxJurisdictionInfoDto[]>([]);
  protected readonly editingId = signal<string | null>(null);
  protected readonly dialogOpen = signal(false);

  protected readonly jurisdictionOptions = computed(() =>
    this.jurisdictions().map((j) => ({
      label: this.formatJurisdiction(j),
      value: this.jurisdictionKey(j),
      defaultRate: j.defaultRatePercent ?? null,
      countryCode: j.countryCode,
      region: j.region,
    })),
  );

  protected readonly model = signal<TaxRateModel>({ ...EMPTY });

  /**
   * The jurisdiction is disabled while editing (editing it would create a different rate row),
   * driven declaratively off `editingId()`.
   */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.jurisdictionKey, { message: "Jurisdiction is required." });
      required(p.ratePercent, { message: "Rate is required." });
      min(p.ratePercent, 0, { message: "Rate must be at least 0." });
      max(p.ratePercent, 100, { message: "Rate cannot exceed 100." });
      disabled(p.jurisdictionKey, { when: () => this.editingId() !== null });
    },
    {
      submission: {
        action: async () => {
          const value = this.model();
          const editing = this.editingId();
          try {
            if (editing) {
              const command: UpdateTenantTaxRateCommand = {
                ratePercent: value.ratePercent!,
                description: value.description || null,
                effectiveFrom: value.effectiveFrom?.toISOString() ?? null,
                effectiveTo: value.effectiveTo?.toISOString() ?? null,
              };

              await this.api.invoke(updateTenantTaxRate, { id: editing, body: command });
              this.toastService.showSuccess("Tax rate updated");
            } else {
              const [country, region] = (value.jurisdictionKey ?? "").split(":");
              const command: CreateTenantTaxRateCommand = {
                countryCode: country,
                region: region || null,
                ratePercent: value.ratePercent!,
                description: value.description || null,
                effectiveFrom: value.effectiveFrom?.toISOString() ?? null,
                effectiveTo: value.effectiveTo?.toISOString() ?? null,
              };

              await this.api.invoke(createTenantTaxRate, { body: command });
              this.toastService.showSuccess("Tax rate created");
            }
            this.closeDialog();
            await this.loadRates();
          } catch {
            this.toastService.showError("Failed to save tax rate");
          }
          return undefined;
        },
      },
    },
  );

  ngOnInit(): void {
    this.loadAll();
  }

  protected openCreate(): void {
    this.editingId.set(null);
    this.form().reset({ ...EMPTY });
    this.dialogOpen.set(true);
  }

  protected openEdit(rate: TenantTaxRateDto): void {
    if (!rate.id) return;
    this.editingId.set(rate.id);
    this.form().reset({
      jurisdictionKey: this.jurisdictionKey({
        countryCode: rate.jurisdiction?.countryCode ?? null,
        region: rate.jurisdiction?.region ?? null,
      } as TaxJurisdictionInfoDto),
      ratePercent: rate.ratePercent ?? null,
      description: rate.description ?? "",
      effectiveFrom: rate.effectiveFrom ? new Date(rate.effectiveFrom) : null,
      effectiveTo: rate.effectiveTo ? new Date(rate.effectiveTo) : null,
    });
    // Editing the jurisdiction is unsupported (would create a different rate row); the schema's
    // `disabled(p.jurisdictionKey, {when: editingId})` rule disables it now that `editingId` is set.
    this.dialogOpen.set(true);
  }

  /** Pre-fill the rate from the selected jurisdiction's static default. */
  protected onJurisdictionChange(key: string | null): void {
    if (!key || this.editingId()) return;
    const opt = this.jurisdictionOptions().find((o) => o.value === key);
    if (opt?.defaultRate != null && this.model().ratePercent == null) {
      this.model.update((v) => ({ ...v, ratePercent: opt.defaultRate }));
    }
  }

  protected closeDialog(): void {
    this.dialogOpen.set(false);
  }

  protected askDelete(rate: TenantTaxRateDto): void {
    if (!rate.id) return;
    this.toastService.confirm({
      message: `Delete the ${this.formatRate(rate)} rate?`,
      accept: () => this.delete(rate.id!),
    });
  }

  protected formatRate(rate: TenantTaxRateDto): string {
    const j = rate.jurisdiction;
    const where = j?.region ? `${j.countryCode}-${j.region}` : (j?.countryCode ?? "?");
    return `${rate.ratePercent ?? 0}% — ${where}`;
  }

  private async delete(id: string): Promise<void> {
    try {
      await this.api.invoke(deleteTenantTaxRate, { id });
      this.toastService.showSuccess("Tax rate deleted");
      await this.loadRates();
    } catch {
      this.toastService.showError("Failed to delete tax rate");
    }
  }

  private async loadAll(): Promise<void> {
    this.isLoading.set(true);
    try {
      await Promise.all([this.loadRates(), this.loadJurisdictions()]);
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadRates(): Promise<void> {
    const result = await this.api.invoke(getTenantTaxRates, {});
    this.rates.set(result ?? []);
  }

  private async loadJurisdictions(): Promise<void> {
    const result = await this.api.invoke(getTaxJurisdictions, {});
    this.jurisdictions.set(result ?? []);
  }

  private jurisdictionKey(j: { countryCode?: string | null; region?: string | null }): string {
    return `${j.countryCode ?? ""}:${j.region ?? ""}`;
  }

  private formatJurisdiction(j: TaxJurisdictionInfoDto): string {
    const base = j.displayName ?? j.countryCode ?? "?";
    if (j.defaultRatePercent != null) {
      return `${base} (default ${j.defaultRatePercent}%)`;
    }
    return base ?? "";
  }
}
