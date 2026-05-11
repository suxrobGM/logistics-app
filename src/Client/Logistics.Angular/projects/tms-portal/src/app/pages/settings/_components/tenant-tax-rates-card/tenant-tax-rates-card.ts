import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
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
import {
  Alert,
  FormField,
  Stack,
  Typography,
  ValidationSummary,
} from "@logistics/shared/components";
import { DateFormatPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePickerModule } from "primeng/datepicker";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { ToastService } from "@/core/services";

interface TaxRateForm {
  jurisdictionKey: FormControl<string | null>;
  ratePercent: FormControl<number | null>;
  description: FormControl<string | null>;
  effectiveFrom: FormControl<Date | null>;
  effectiveTo: FormControl<Date | null>;
}

@Component({
  selector: "app-tenant-tax-rates-card",
  templateUrl: "./tenant-tax-rates-card.html",
  imports: [
    ReactiveFormsModule,
    CardModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    DatePickerModule,
    TableModule,
    ProgressSpinnerModule,
    FormField,
    ValidationSummary,
    Alert,
    Stack,
    Typography,
    DateFormatPipe,
  ],
})
export class TenantTaxRatesCard implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
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

  protected readonly form = new FormGroup<TaxRateForm>({
    jurisdictionKey: new FormControl<string | null>(null, { validators: Validators.required }),
    ratePercent: new FormControl<number | null>(null, {
      validators: [Validators.required, Validators.min(0), Validators.max(100)],
    }),
    description: new FormControl<string | null>(null),
    effectiveFrom: new FormControl<Date | null>(null),
    effectiveTo: new FormControl<Date | null>(null),
  });

  ngOnInit(): void {
    this.loadAll();
  }

  protected openCreate(): void {
    this.editingId.set(null);
    this.form.reset({
      jurisdictionKey: null,
      ratePercent: null,
      description: null,
      effectiveFrom: null,
      effectiveTo: null,
    });
    this.dialogOpen.set(true);
  }

  protected openEdit(rate: TenantTaxRateDto): void {
    if (!rate.id) return;
    this.editingId.set(rate.id);
    this.form.reset({
      jurisdictionKey: this.jurisdictionKey({
        countryCode: rate.jurisdiction?.countryCode ?? null,
        region: rate.jurisdiction?.region ?? null,
      } as TaxJurisdictionInfoDto),
      ratePercent: rate.ratePercent ?? null,
      description: rate.description ?? null,
      effectiveFrom: rate.effectiveFrom ? new Date(rate.effectiveFrom) : null,
      effectiveTo: rate.effectiveTo ? new Date(rate.effectiveTo) : null,
    });
    // Editing the jurisdiction is unsupported (would create a different rate row).
    this.form.controls.jurisdictionKey.disable();
    this.dialogOpen.set(true);
  }

  /** Pre-fill the rate from the selected jurisdiction's static default. */
  protected onJurisdictionChange(key: string | null): void {
    if (!key || this.editingId()) return;
    const opt = this.jurisdictionOptions().find((o) => o.value === key);
    if (opt?.defaultRate != null && this.form.controls.ratePercent.value == null) {
      this.form.controls.ratePercent.setValue(opt.defaultRate);
    }
  }

  protected closeDialog(): void {
    this.dialogOpen.set(false);
    this.form.controls.jurisdictionKey.enable();
  }

  protected async save(): Promise<void> {
    if (this.form.invalid) return;

    const value = this.form.getRawValue();
    const editing = this.editingId();

    this.isSaving.set(true);
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
    } finally {
      this.isSaving.set(false);
    }
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
