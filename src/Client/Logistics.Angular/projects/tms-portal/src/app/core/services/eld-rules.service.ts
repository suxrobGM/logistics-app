import { computed, inject, Injectable, signal } from "@angular/core";
import { Api, getHosLimits, type HosLimitsDto } from "@logistics/shared/api";

const RULE_SET_LABELS: Record<string, string> = {
  FMCSA: "FMCSA",
  EU_561_2006: "EU 561/2006",
};

const DEFAULT_DRIVING_WARN_MINUTES = 60;
const DEFAULT_ON_DUTY_WARN_MINUTES = 120;
const DRIVING_WARN_PCT = 0.1;
const ON_DUTY_WARN_PCT = 0.15;

/**
 * Caches the regulatory HOS limits for the active tenant. Fetched once
 * per session; values are display-only (HOS counters themselves come
 * from the ELD / tachograph provider).
 */
@Injectable({ providedIn: "root" })
export class EldRulesService {
  private readonly api = inject(Api);
  private readonly _limits = signal<HosLimitsDto | null>(null);
  private fetchPromise: Promise<HosLimitsDto | null> | null = null;

  public readonly limits = this._limits.asReadonly();
  public readonly ruleSetCode = computed(() => this._limits()?.ruleSetCode ?? "FMCSA");
  public readonly ruleSetLabel = computed(() => {
    const code = this.ruleSetCode();
    return RULE_SET_LABELS[code] ?? code;
  });

  /**
   * "Running low" thresholds, derived from the tenant's own daily caps so an EU 561/2006 tenant
   * doesn't get FMCSA-shaped numbers. Fall back to the FMCSA figures until the limits load.
   */
  public readonly drivingWarnMinutes = computed(() => {
    const max = this._limits()?.maxDailyDrivingMinutes;
    return max ? Math.round(max * DRIVING_WARN_PCT) : DEFAULT_DRIVING_WARN_MINUTES;
  });

  public readonly onDutyWarnMinutes = computed(() => {
    const max = this._limits()?.maxDailyOnDutyMinutes;
    return max ? Math.round(max * ON_DUTY_WARN_PCT) : DEFAULT_ON_DUTY_WARN_MINUTES;
  });

  async load(): Promise<HosLimitsDto | null> {
    if (this._limits() !== null) {
      return this._limits();
    }
    this.fetchPromise ??= this.fetch();
    return this.fetchPromise;
  }

  private async fetch(): Promise<HosLimitsDto | null> {
    try {
      const data = await this.api.invoke(getHosLimits);
      this._limits.set(data ?? null);
      return data ?? null;
    } catch (err) {
      console.error("Failed to load HOS limits", err);
      this.fetchPromise = null;
      return null;
    }
  }
}
