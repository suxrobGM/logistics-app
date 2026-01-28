import { isPlatformBrowser } from "@angular/common";
import { Injectable, PLATFORM_ID, computed, inject, signal } from "@angular/core";

const DASHBOARD_SETTINGS_KEY = "tms-dashboard-settings";
const CURRENT_VERSION = 1;

export type PanelSize = "small" | "medium" | "large" | "full";
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
  visible: boolean;
  order: number;
  size: PanelSize;
}

interface DashboardSettings {
  version: number;
  panels: DashboardPanelConfig[];
}

const DEFAULT_PANELS: DashboardPanelConfig[] = [
  { id: "kpi-weekly-gross", label: "Weekly Gross", visible: true, order: 0, size: "small" },
  { id: "kpi-billed-miles", label: "Billed Miles", visible: true, order: 1, size: "small" },
  { id: "kpi-rate-per-mile", label: "Rate Per Mile", visible: true, order: 2, size: "small" },
  { id: "kpi-today-gross", label: "Today's Gross", visible: true, order: 3, size: "small" },
  { id: "active-loads", label: "Active Loads", visible: true, order: 4, size: "full" },
  { id: "recent-activity", label: "Recent Activity", visible: true, order: 5, size: "small" },
  { id: "fleet-map", label: "Fleet Map", visible: true, order: 6, size: "large" },
  { id: "daily-gross-chart", label: "Daily Gross Chart", visible: true, order: 7, size: "full" },
];

const DEFAULT_SETTINGS: DashboardSettings = {
  version: CURRENT_VERSION,
  panels: DEFAULT_PANELS,
};

@Injectable({ providedIn: "root" })
export class DashboardSettingsService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly _settings = signal<DashboardSettings>(DEFAULT_SETTINGS);

  /** Current dashboard settings */
  public readonly settings = this._settings.asReadonly();

  /** Panels sorted by order */
  public readonly sortedPanels = computed(() =>
    [...this._settings().panels].sort((a, b) => a.order - b.order),
  );

  constructor() {
    this.loadSettings();
  }

  /** Check if a panel is visible */
  public isPanelVisible(panelId: PanelType): boolean {
    return this._settings().panels.find((p) => p.id === panelId)?.visible ?? true;
  }

  /** Get the CSS class for a panel based on its size */
  public getPanelClass(panelId: PanelType): string {
    const panel = this._settings().panels.find((p) => p.id === panelId);
    if (!panel?.visible) return "hidden";

    switch (panel.size) {
      case "small":
        return "col-span-12 sm:col-span-6 lg:col-span-3";
      case "medium":
        return "col-span-12 lg:col-span-6";
      case "large":
        return "col-span-12 lg:col-span-8";
      case "full":
        return "col-span-12";
      default:
        return "col-span-12 lg:col-span-4";
    }
  }

  /** Update a single panel's configuration */
  public updatePanel(panelId: PanelType, config: Partial<Omit<DashboardPanelConfig, "id">>): void {
    this._settings.update((s) => ({
      ...s,
      panels: s.panels.map((p) => (p.id === panelId ? { ...p, ...config } : p)),
    }));
    this.persistSettings();
  }

  /** Toggle a panel's visibility */
  public togglePanelVisibility(panelId: PanelType): void {
    const panel = this._settings().panels.find((p) => p.id === panelId);
    if (panel) {
      this.updatePanel(panelId, { visible: !panel.visible });
    }
  }

  /** Reorder panels based on new order array */
  public reorderPanels(panelIds: PanelType[]): void {
    this._settings.update((s) => ({
      ...s,
      panels: s.panels.map((p) => {
        const newOrder = panelIds.indexOf(p.id);
        return newOrder >= 0 ? { ...p, order: newOrder } : p;
      }),
    }));
    this.persistSettings();
  }

  /** Reset all settings to defaults */
  public resetToDefaults(): void {
    this._settings.set(DEFAULT_SETTINGS);
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
        this._settings.set(this.mergeWithDefaults(parsed));
      } catch {
        this._settings.set(DEFAULT_SETTINGS);
      }
    }
  }

  private persistSettings(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(DASHBOARD_SETTINGS_KEY, JSON.stringify(this._settings()));
    }
  }

  /** Merge stored settings with defaults to handle new panels added in future versions */
  private mergeWithDefaults(stored: DashboardSettings): DashboardSettings {
    const existingIds = new Set(stored.panels.map((p) => p.id));
    const newPanels = DEFAULT_PANELS.filter((p) => !existingIds.has(p.id)).map((p, i) => ({
      ...p,
      order: stored.panels.length + i,
    }));

    return {
      version: CURRENT_VERSION,
      panels: [...stored.panels, ...newPanels],
    };
  }
}
