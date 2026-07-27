import { isPlatformBrowser } from "@angular/common";
import { computed, inject, Injectable, PLATFORM_ID, signal } from "@angular/core";
import { UserRole } from "@logistics/shared";
import type { OperatingMode, TenantFeature } from "@logistics/shared/api";
import { FeatureService } from "@logistics/shared/services";
import type { IconName } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { TenantService } from "./tenant.service";

const DASHBOARD_SETTINGS_KEY = "tms-dashboard-settings";
const CURRENT_VERSION = 3;

export type PanelType =
  | "kpi-weekly-gross"
  | "kpi-billed-miles"
  | "kpi-rate-per-mile"
  | "kpi-today-gross"
  | "active-loads"
  | "recent-activity"
  | "fleet-map"
  | "daily-gross-chart"
  | "onboarding-checklist"
  | "hos-remaining"
  | "attention-panel"
  | "financial-health"
  | "top-performers";

/** Panels fed by `getCompanyStats` - the home page only issues that request when one is visible. */
const STATS_BACKED_PANEL_IDS: ReadonlySet<PanelType> = new Set<PanelType>([
  "attention-panel",
  "financial-health",
  "top-performers",
]);

export interface DashboardPanelConfig {
  id: PanelType;
  label: string;
  icon: IconName;
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
  /** Panel is shown only in this operating mode. Absent = both modes. */
  mode?: OperatingMode;
  /** Static metadata, re-applied from the defaults on every load - never the user's choice. */
  hiddenByDefault?: boolean;
  /** Narrower form of {@link hiddenByDefault}, for panels that only start hidden in some modes. */
  hiddenByDefaultIn?: readonly OperatingMode[];
  /** The user's explicit choice. Overrides both defaults once set. */
  hidden?: boolean;
}

interface DashboardSettings {
  version: number;
  panels: DashboardPanelConfig[];
}

/**
 * 12 columns. The fleet-only panels (`attention-panel` x8/y2, `fleet-map` x0/y7) sit in exactly the
 * slots the solo-only ones fill, so moving one of those four means moving its counterpart.
 */
const DEFAULT_PANELS: DashboardPanelConfig[] = [
  {
    id: "kpi-weekly-gross",
    label: "Weekly Gross",
    icon: "dollar-sign",
    x: 0,
    y: 0,
    cols: 4,
    rows: 2,
    minItemCols: 2,
    minItemRows: 1,
  },
  {
    id: "kpi-billed-miles",
    label: "Billed Miles",
    icon: "map",
    x: 4,
    y: 0,
    cols: 4,
    rows: 2,
    minItemCols: 2,
    minItemRows: 1,
  },
  {
    id: "kpi-rate-per-mile",
    label: "Rate Per Mile",
    icon: "chart-line",
    x: 8,
    y: 0,
    cols: 4,
    rows: 2,
    minItemCols: 2,
    minItemRows: 1,
  },
  {
    id: "active-loads",
    label: "Active Loads",
    icon: "box",
    x: 0,
    y: 2,
    cols: 8,
    rows: 5,
    minItemCols: 4,
    minItemRows: 3,
  },
  {
    id: "attention-panel",
    label: "Needs Attention",
    icon: "circle-alert",
    x: 8,
    y: 2,
    cols: 4,
    rows: 5,
    minItemCols: 3,
    minItemRows: 2,
    roles: [UserRole.Owner],
    feature: "dashboard",
    mode: "fleet",
  },
  {
    id: "hos-remaining",
    label: "Hours of Service",
    icon: "clock",
    x: 8,
    y: 2,
    cols: 4,
    rows: 5,
    minItemCols: 3,
    minItemRows: 2,
    feature: "eld",
    mode: "solo_operator",
  },
  {
    id: "fleet-map",
    label: "Fleet Map",
    icon: "map-pin",
    x: 0,
    y: 7,
    cols: 8,
    rows: 5,
    minItemCols: 4,
    minItemRows: 2,
    mode: "fleet",
  },
  {
    id: "financial-health",
    label: "Financial Health",
    icon: "dollar-sign",
    x: 0,
    y: 7,
    cols: 8,
    rows: 5,
    minItemCols: 3,
    minItemRows: 2,
    roles: [UserRole.Owner],
    feature: "dashboard",
    hiddenByDefaultIn: ["fleet"],
  },
  {
    id: "onboarding-checklist",
    label: "Getting Started",
    icon: "list",
    x: 8,
    y: 7,
    cols: 4,
    rows: 5,
    minItemCols: 3,
    minItemRows: 2,
    roles: [UserRole.Owner],
  },
  {
    id: "kpi-today-gross",
    label: "Today's Gross",
    icon: "calendar",
    x: 0,
    y: 12,
    cols: 4,
    rows: 2,
    minItemCols: 2,
    minItemRows: 1,
    hiddenByDefault: true,
  },
  {
    id: "top-performers",
    label: "Top Performers",
    icon: "trophy",
    x: 4,
    y: 12,
    cols: 4,
    rows: 5,
    minItemCols: 3,
    minItemRows: 2,
    roles: [UserRole.Owner],
    feature: "dashboard",
    mode: "fleet",
    hiddenByDefault: true,
  },
  {
    id: "recent-activity",
    label: "Recent Activity",
    icon: "history",
    x: 8,
    y: 12,
    cols: 4,
    rows: 5,
    minItemCols: 3,
    minItemRows: 2,
    hiddenByDefault: true,
  },
  {
    id: "daily-gross-chart",
    label: "Daily Gross Chart",
    icon: "chart-line",
    x: 0,
    y: 17,
    cols: 12,
    rows: 5,
    minItemCols: 6,
    minItemRows: 2,
    hiddenByDefault: true,
  },
];

@Injectable({ providedIn: "root" })
export class DashboardSettingsService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly authService = inject(AuthService);
  private readonly featureService = inject(FeatureService);
  private readonly tenantService = inject(TenantService);

  private readonly _panels = signal<DashboardPanelConfig[]>(DEFAULT_PANELS);

  /** All panels (raw, unfiltered). Used by gridster `updatePanelLayout` callbacks. */
  public readonly panels = this._panels.asReadonly();

  public readonly visiblePanels = computed(() =>
    this.gatedPanels()
      .filter(({ hidden }) => !hidden)
      .map(({ panel }) => panel),
  );

  public readonly availablePanels = computed(() =>
    this.gatedPanels()
      .filter(({ hidden }) => hidden)
      .map(({ panel }) => panel),
  );

  public readonly needsCompanyStats = computed(() =>
    this.visiblePanels().some((p) => STATS_BACKED_PANEL_IDS.has(p.id)),
  );

  private readonly gatedPanels = computed(() => {
    const role = this.authService.getUserData()?.role ?? null;
    const mode = this.tenantService.operatingMode();
    return this._panels()
      .filter((panel) => this.passesGates(panel, role, mode))
      .map((panel) => ({ panel, hidden: this.isHidden(panel, mode) }));
  });

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

  /**
   * Lands below everything on screen: reusing the stored coordinates would drop the panel on top of
   * whatever the user has since arranged there.
   */
  public showPanel(panelId: PanelType): void {
    const nextY = this.visiblePanels().reduce((max, p) => Math.max(max, p.y + p.rows), 0);
    this._panels.update((panels) =>
      panels.map((p) => (p.id === panelId ? { ...p, hidden: false, x: 0, y: nextY } : p)),
    );
    this.persistSettings();
  }

  public hidePanel(panelId: PanelType): void {
    this._panels.update((panels) =>
      panels.map((p) => (p.id === panelId ? { ...p, hidden: true } : p)),
    );
    this.persistSettings();
  }

  public resetToDefaults(): void {
    this._panels.set(DEFAULT_PANELS);
    this.persistSettings();
  }

  /** Clear any persisted layout - call on logout to avoid leaking layout to next user. */
  public clearPersistedSettings(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(DASHBOARD_SETTINGS_KEY);
    }
    this._panels.set(DEFAULT_PANELS);
  }

  private passesGates(
    panel: DashboardPanelConfig,
    role: string | null,
    mode: OperatingMode,
  ): boolean {
    if (panel.roles && panel.roles.length > 0) {
      if (!role || !panel.roles.includes(role as UserRole)) return false;
    }
    if (panel.feature && !this.featureService.isEnabled(panel.feature)) {
      return false;
    }
    if (panel.mode && panel.mode !== mode) {
      return false;
    }
    return true;
  }

  private isHidden(panel: DashboardPanelConfig, mode: OperatingMode): boolean {
    return (
      panel.hidden ?? panel.hiddenByDefault ?? panel.hiddenByDefaultIn?.includes(mode) ?? false
    );
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

  /**
   * Stored ids that no longer ship are dropped: the template has no `@case` for them, so gridster
   * would render a silent empty tile.
   */
  private mergeWithDefaults(storedPanels: DashboardPanelConfig[]): DashboardPanelConfig[] {
    const defaults = new Map(DEFAULT_PANELS.map((d) => [d.id, d]));
    const kept = storedPanels
      .filter((stored) => defaults.has(stored.id))
      .map((stored) => ({
        ...defaults.get(stored.id)!,
        x: stored.x,
        y: stored.y,
        cols: stored.cols,
        rows: stored.rows,
        hidden: stored.hidden,
      }));

    // Newly-introduced panels stack underneath rather than at their default coordinates, which are
    // laid out against the current grid and would land on top of the stored one.
    const keptIds = new Set(kept.map((p) => p.id));
    let nextY = kept.reduce((max, p) => Math.max(max, p.y + p.rows), 0);
    const added = DEFAULT_PANELS.filter((d) => !keptIds.has(d.id)).map((d) => {
      const placed = { ...d, x: 0, y: nextY };
      nextY += d.rows;
      return placed;
    });

    return [...kept, ...added];
  }
}
