import { Permission } from "@logistics/shared";
import type { OperatingMode } from "@logistics/shared/api";
import type { IconName } from "@logistics/shared/ui";
import type { AccessGate } from "@/shared/layout/nav-menu";

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
export const STATS_BACKED_PANEL_IDS: ReadonlySet<PanelType> = new Set<PanelType>([
  "attention-panel",
  "financial-health",
  "top-performers",
]);

/** Lets `hiddenByDefaultIn` cover "hidden everywhere" without a second flag. */
const ALL_MODES: readonly OperatingMode[] = ["fleet", "solo_operator"];

export interface DashboardPanelConfig extends AccessGate {
  id: PanelType;
  label: string;
  icon: IconName;
  x: number;
  y: number;
  cols: number;
  rows: number;
  minItemCols?: number;
  minItemRows?: number;
  /** Panel is shown only in this operating mode. Absent = both modes. */
  mode?: OperatingMode;
  /** Modes where the panel starts in the "Add Panel" menu. Re-applied from the defaults on load. */
  hiddenByDefaultIn?: readonly OperatingMode[];
  /** The user's explicit choice. Overrides {@link hiddenByDefaultIn} once set. */
  hidden?: boolean;
  /** Content sizes to its parent (map, chart, table), so the stacked layout gives it a fixed box. */
  fillsParent?: boolean;
}

/**
 * 12 columns. The fleet-only panels (`attention-panel` x8/y2, `fleet-map` x0/y7) sit in exactly the
 * slots the solo-only ones fill, so moving one of those four means moving its counterpart.
 */
export const DEFAULT_PANELS: DashboardPanelConfig[] = [
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
    fillsParent: true,
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
    permission: Permission.Load.Manage,
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
    fillsParent: true,
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
    permission: Permission.Accounting.View,
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
    // Company setup steps - the same gate the tenant settings pages use, so Owner only.
    permission: Permission.Tenant.Manage,
    fillsParent: true,
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
    hiddenByDefaultIn: ALL_MODES,
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
    permission: Permission.Load.Manage,
    feature: "dashboard",
    mode: "fleet",
    hiddenByDefaultIn: ALL_MODES,
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
    hiddenByDefaultIn: ALL_MODES,
    fillsParent: true,
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
    hiddenByDefaultIn: ALL_MODES,
    fillsParent: true,
  },
];
