import { isPlatformBrowser } from "@angular/common";
import { computed, inject, Injectable, PLATFORM_ID, signal } from "@angular/core";
import { UserRole } from "@logistics/shared";
import type { TenantFeature } from "@logistics/shared/api";
import { FeatureService } from "@logistics/shared/services";
import { AuthService } from "@/core/auth";

const DASHBOARD_SETTINGS_KEY = "tms-dashboard-settings";
const CURRENT_VERSION = 2;

export type PanelType =
  | "kpi-weekly-gross"
  | "kpi-billed-miles"
  | "kpi-rate-per-mile"
  | "kpi-today-gross"
  | "active-loads"
  | "recent-activity"
  | "fleet-map"
  | "daily-gross-chart"
  // Owner-only panels (added in v2)
  | "attention-panel"
  | "financial-health"
  | "top-performers"
  | "owner-kpi-row";

const OWNER_PANEL_IDS: ReadonlySet<PanelType> = new Set<PanelType>([
  "attention-panel",
  "financial-health",
  "top-performers",
  "owner-kpi-row",
]);

export interface DashboardPanelConfig {
  id: PanelType;
  label: string;
  x: number;
  y: number;
  cols: number;
  rows: number;
  minItemCols?: number;
  minItemRows?: number;
  /** Panel is shown only if the user's role is in this list. Absent = universal. */
  roles?: UserRole[];
  /** Panel is shown only if this tenant feature is enabled. Absent = universal. */
  feature?: TenantFeature;
}

interface DashboardSettings {
  version: number;
  panels: DashboardPanelConfig[];
}

const DEFAULT_PANELS: DashboardPanelConfig[] = [
  // KPI Cards Row (y=0) — universal panels
  {
    id: "kpi-weekly-gross",
    label: "Weekly Gross",
    x: 0,
    y: 0,
    cols: 3,
    rows: 2,
    minItemCols: 2,
    minItemRows: 1,
  },
  {
    id: "kpi-billed-miles",
    label: "Billed Miles",
    x: 3,
    y: 0,
    cols: 3,
    rows: 2,
    minItemCols: 2,
    minItemRows: 1,
  },
  {
    id: "kpi-rate-per-mile",
    label: "Rate Per Mile",
    x: 6,
    y: 0,
    cols: 3,
    rows: 2,
    minItemCols: 2,
    minItemRows: 1,
  },
  {
    id: "kpi-today-gross",
    label: "Today's Gross",
    x: 9,
    y: 0,
    cols: 3,
    rows: 2,
    minItemCols: 2,
    minItemRows: 1,
  },
  // Owner panels (y=2) — only visible to Owner with `dashboard` feature enabled
  {
    id: "attention-panel",
    label: "Attention",
    x: 0,
    y: 2,
    cols: 4,
    rows: 4,
    minItemCols: 3,
    minItemRows: 2,
    roles: [UserRole.Owner],
    feature: "dashboard",
  },
  {
    id: "financial-health",
    label: "Financial Health",
    x: 4,
    y: 2,
    cols: 4,
    rows: 4,
    minItemCols: 3,
    minItemRows: 2,
    roles: [UserRole.Owner],
    feature: "dashboard",
  },
  {
    id: "top-performers",
    label: "Top Performers",
    x: 8,
    y: 2,
    cols: 4,
    rows: 4,
    minItemCols: 3,
    minItemRows: 2,
    roles: [UserRole.Owner],
    feature: "dashboard",
  },
  // Active Loads Table (y=6 to leave room for owner row)
  {
    id: "active-loads",
    label: "Active Loads",
    x: 0,
    y: 6,
    cols: 6,
    rows: 5,
    minItemCols: 6,
    minItemRows: 3,
  },
  // Fleet Map (y=6)
  {
    id: "fleet-map",
    label: "Fleet Map",
    x: 6,
    y: 6,
    cols: 6,
    rows: 12,
    minItemCols: 4,
    minItemRows: 2,
  },
  // Recent Activity (y=11)
  {
    id: "recent-activity",
    label: "Recent Activity",
    x: 0,
    y: 11,
    cols: 6,
    rows: 7,
    minItemCols: 3,
    minItemRows: 2,
  },
  // Daily Gross Chart (y=18)
  {
    id: "daily-gross-chart",
    label: "Daily Gross Chart",
    x: 0,
    y: 18,
    cols: 12,
    rows: 5,
    minItemCols: 6,
    minItemRows: 2,
  },
];

@Injectable({ providedIn: "root" })
export class DashboardSettingsService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly authService = inject(AuthService);
  private readonly featureService = inject(FeatureService);

  private readonly _panels = signal<DashboardPanelConfig[]>(DEFAULT_PANELS);

  /** All panels (raw, unfiltered). Used by gridster `updatePanelLayout` callbacks. */
  public readonly panels = this._panels.asReadonly();

  /** Panels visible to the current user, filtered by role + feature gates. */
  public readonly visiblePanels = computed(() => {
    const role = this.authService.getUserData()?.role ?? null;
    return this._panels().filter((p) => this.passesGates(p, role));
  });

  /** Whether any owner-specific panels are currently visible (used to decide on getCompanyStats fetch). */
  public readonly hasOwnerPanels = computed(() =>
    this.visiblePanels().some((p) => OWNER_PANEL_IDS.has(p.id)),
  );

  constructor() {
    this.loadSettings();
  }

  public getPanel(panelId: PanelType): DashboardPanelConfig | undefined {
    return this._panels().find((p) => p.id === panelId);
  }

  public updatePanelLayout(
    panelId: PanelType,
    x: number,
    y: number,
    cols: number,
    rows: number,
  ): void {
    this._panels.update((panels) =>
      panels.map((p) => (p.id === panelId ? { ...p, x, y, cols, rows } : p)),
    );
    this.persistSettings();
  }

  public updateAllPanels(panels: DashboardPanelConfig[]): void {
    this._panels.set(panels);
    this.persistSettings();
  }

  public resetToDefaults(): void {
    this._panels.set(DEFAULT_PANELS);
    this.persistSettings();
  }

  /** Clear any persisted layout — call on logout to avoid leaking layout to next user. */
  public clearPersistedSettings(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(DASHBOARD_SETTINGS_KEY);
    }
    this._panels.set(DEFAULT_PANELS);
  }

  private passesGates(panel: DashboardPanelConfig, role: string | null): boolean {
    if (panel.roles && panel.roles.length > 0) {
      if (!role || !panel.roles.includes(role as UserRole)) return false;
    }
    if (panel.feature && !this.featureService.isEnabled(panel.feature)) {
      return false;
    }
    return true;
  }

  private loadSettings(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const stored = localStorage.getItem(DASHBOARD_SETTINGS_KEY);
    if (!stored) return;

    try {
      const parsed = JSON.parse(stored) as DashboardSettings;
      // Newer-than-current versions: leave DEFAULT_PANELS to avoid data we don't know how to read.
      if (parsed.version > CURRENT_VERSION) return;

      // Older versions upgrade implicitly: mergeWithDefaults appends any panel IDs introduced
      // since the stored version while preserving the stored positions of shared panels.
      this._panels.set(this.mergeWithDefaults(parsed.panels));
      if (parsed.version < CURRENT_VERSION) {
        this.persistSettings();
      }
    } catch {
      this._panels.set(DEFAULT_PANELS);
    }
  }

  private persistSettings(): void {
    if (isPlatformBrowser(this.platformId)) {
      const settings: DashboardSettings = {
        version: CURRENT_VERSION,
        panels: this._panels(),
      };
      localStorage.setItem(DASHBOARD_SETTINGS_KEY, JSON.stringify(settings));
    }
  }

  /** Append any new default panel IDs that aren't in the stored set, preserving stored positions. */
  private mergeWithDefaults(storedPanels: DashboardPanelConfig[]): DashboardPanelConfig[] {
    const existingIds = new Set(storedPanels.map((p) => p.id));
    const newPanels = DEFAULT_PANELS.filter((p) => !existingIds.has(p.id));
    // Re-apply gate metadata from defaults in case it changed since storage.
    const enriched = storedPanels.map((stored) => {
      const def = DEFAULT_PANELS.find((d) => d.id === stored.id);
      return def ? { ...stored, roles: def.roles, feature: def.feature, label: def.label } : stored;
    });
    return [...enriched, ...newPanels];
  }
}
