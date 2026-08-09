import { isPlatformBrowser } from "@angular/common";
import { computed, inject, Injectable, PLATFORM_ID, signal } from "@angular/core";
import type { OperatingMode } from "@logistics/shared/api";
import { FeatureService } from "@logistics/shared/services";
import { PermissionService } from "@/core/auth";
import { passesAccessGate } from "@/shared/layout/nav-menu";
import {
  DEFAULT_PANELS,
  STATS_BACKED_PANEL_IDS,
  type DashboardPanelConfig,
  type PanelType,
} from "./dashboard-panels";
import { TenantService } from "./tenant.service";

const DASHBOARD_SETTINGS_KEY = "tms-dashboard-settings";
const CURRENT_VERSION = 3;

interface DashboardSettings {
  version: number;
  panels: DashboardPanelConfig[];
}

/** The row a panel appended below `panels` should start on. */
function bottomOf(panels: readonly DashboardPanelConfig[]): number {
  return panels.reduce((max, p) => Math.max(max, p.y + p.rows), 0);
}

@Injectable({ providedIn: "root" })
export class DashboardSettingsService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly featureService = inject(FeatureService);
  private readonly permissionService = inject(PermissionService);
  private readonly tenantService = inject(TenantService);

  private readonly _panels = signal<DashboardPanelConfig[]>(DEFAULT_PANELS);

  /** Panels the current user may see, split by whether they are on the grid or in the Add menu. */
  private readonly gatedPanels = computed(() => {
    const mode = this.tenantService.operatingMode();
    const visible: DashboardPanelConfig[] = [];
    const available: DashboardPanelConfig[] = [];

    for (const panel of this._panels()) {
      if (!this.passesGates(panel, mode)) {
        continue;
      }
      (this.isHidden(panel, mode) ? available : visible).push(panel);
    }

    return { visible, available };
  });

  public readonly visiblePanels = computed(() => this.gatedPanels().visible);
  public readonly availablePanels = computed(() => this.gatedPanels().available);

  public readonly needsCompanyStats = computed(() =>
    this.visiblePanels().some((p) => STATS_BACKED_PANEL_IDS.has(p.id)),
  );

  constructor() {
    this.loadSettings();
  }

  public updatePanelLayout(
    panelId: PanelType,
    x: number,
    y: number,
    cols: number,
    rows: number,
  ): void {
    this.patchPanel(panelId, { x, y, cols, rows });
  }

  /** Lands below everything on screen - stored coordinates would drop it onto a rearranged grid. */
  public showPanel(panelId: PanelType): void {
    this.patchPanel(panelId, { hidden: false, x: 0, y: bottomOf(this.visiblePanels()) });
  }

  public hidePanel(panelId: PanelType): void {
    this.patchPanel(panelId, { hidden: true });
  }

  public resetToDefaults(): void {
    this._panels.set(DEFAULT_PANELS);
    this.persistSettings();
  }

  private patchPanel(panelId: PanelType, changes: Partial<DashboardPanelConfig>): void {
    this._panels.update((panels) =>
      panels.map((p) => (p.id === panelId ? { ...p, ...changes } : p)),
    );
    this.persistSettings();
  }

  private passesGates(panel: DashboardPanelConfig, mode: OperatingMode): boolean {
    if (panel.mode && panel.mode !== mode) return false;
    return passesAccessGate(panel, this.featureService, this.permissionService);
  }

  private isHidden(panel: DashboardPanelConfig, mode: OperatingMode): boolean {
    return panel.hidden ?? panel.hiddenByDefaultIn?.includes(mode) ?? false;
  }

  private loadSettings(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const stored = localStorage.getItem(DASHBOARD_SETTINGS_KEY);
    if (!stored) return;

    try {
      const parsed = JSON.parse(stored) as DashboardSettings;
      // A version we don't know how to read: keep the defaults rather than guess at the shape.
      if (parsed.version > CURRENT_VERSION) return;

      this._panels.set(this.mergeWithDefaults(parsed.panels));
      if (parsed.version < CURRENT_VERSION) {
        this.persistSettings();
      }
    } catch {
      // Corrupt payload - `_panels` still holds DEFAULT_PANELS.
    }
  }

  private persistSettings(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const settings: DashboardSettings = { version: CURRENT_VERSION, panels: this._panels() };
    localStorage.setItem(DASHBOARD_SETTINGS_KEY, JSON.stringify(settings));
  }

  /**
   * Drops stored ids that no longer ship - the template has no `@case`, so gridster would render a
   * silent empty tile. Only geometry and `hidden` survive, so gate metadata can change per version.
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

    // New panels stack underneath; their default coordinates would overlap a stored one. Hidden
    // panels keep stale coordinates and gridster never compacts, so they don't count toward it.
    const mode = this.tenantService.operatingMode();
    const keptIds = new Set(kept.map((p) => p.id));
    let nextY = bottomOf(kept.filter((p) => !this.isHidden(p, mode)));

    const added = DEFAULT_PANELS.filter((d) => !keptIds.has(d.id)).map((d) => {
      const placed = { ...d, x: 0, y: nextY };
      nextY += d.rows;
      return placed;
    });

    return [...kept, ...added];
  }
}
