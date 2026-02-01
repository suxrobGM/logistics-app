import { isPlatformBrowser } from "@angular/common";
import { Injectable, PLATFORM_ID, inject, signal } from "@angular/core";

const DASHBOARD_SETTINGS_KEY = "tms-dashboard-settings";
const CURRENT_VERSION = 1;

export type PanelType =
  | "kpi-weekly-gross"
  | "kpi-billed-miles"
  | "kpi-rate-per-mile"
  | "kpi-today-gross"
  | "active-loads"
  | "recent-activity"
  | "fleet-map"
  | "daily-gross-chart";

export interface DashboardPanelConfig {
  id: PanelType;
  label: string;
  x: number;
  y: number;
  cols: number;
  rows: number;
  minItemCols?: number;
  minItemRows?: number;
}

interface DashboardSettings {
  version: number;
  panels: DashboardPanelConfig[];
}

const DEFAULT_PANELS: DashboardPanelConfig[] = [
  // KPI Cards Row (y=0)
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
  // Active Loads Table (y=2)
  {
    id: "active-loads",
    label: "Active Loads",
    x: 0,
    y: 2,
    cols: 6,
    rows: 5,
    minItemCols: 6,
    minItemRows: 3,
  },
  // Recent Activity (y=7)
  {
    id: "recent-activity",
    label: "Recent Activity",
    x: 0,
    y: 7,
    cols: 6,
    rows: 7,
    minItemCols: 3,
    minItemRows: 2,
  },
  // Fleet Map (y=2)
  {
    id: "fleet-map",
    label: "Fleet Map",
    x: 6,
    y: 2,
    cols: 6,
    rows: 12,
    minItemCols: 4,
    minItemRows: 2,
  },
  // Daily Gross Chart (y=14)
  {
    id: "daily-gross-chart",
    label: "Daily Gross Chart",
    x: 0,
    y: 14,
    cols: 12,
    rows: 5,
    minItemCols: 6,
    minItemRows: 2,
  },
];

@Injectable({ providedIn: "root" })
export class DashboardSettingsService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly _panels = signal<DashboardPanelConfig[]>(DEFAULT_PANELS);

  /** Current dashboard panels */
  public readonly panels = this._panels.asReadonly();

  constructor() {
    this.loadSettings();
  }

  /** Get a panel by ID */
  public getPanel(panelId: PanelType): DashboardPanelConfig | undefined {
    return this._panels().find((p) => p.id === panelId);
  }

  /** Update a panel's position and size (called by gridster on item change) */
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

  /** Update all panels at once (for batch updates) */
  public updateAllPanels(panels: DashboardPanelConfig[]): void {
    this._panels.set(panels);
    this.persistSettings();
  }

  /** Reset all settings to defaults */
  public resetToDefaults(): void {
    this._panels.set(DEFAULT_PANELS);
    this.persistSettings();
  }

  private loadSettings(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const stored = localStorage.getItem(DASHBOARD_SETTINGS_KEY);
    if (stored) {
      try {
        const parsed = JSON.parse(stored) as DashboardSettings;
        if (parsed.version === CURRENT_VERSION) {
          this._panels.set(this.mergeWithDefaults(parsed.panels));
        }
      } catch {
        this._panels.set(DEFAULT_PANELS);
      }
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

  /** Merge stored panels with defaults to handle new panels added in future versions */
  private mergeWithDefaults(storedPanels: DashboardPanelConfig[]): DashboardPanelConfig[] {
    const existingIds = new Set(storedPanels.map((p) => p.id));
    const newPanels = DEFAULT_PANELS.filter((p) => !existingIds.has(p.id));
    return [...storedPanels, ...newPanels];
  }
}
