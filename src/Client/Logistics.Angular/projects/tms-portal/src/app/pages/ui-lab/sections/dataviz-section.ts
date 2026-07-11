import { Component, computed, inject, signal } from "@angular/core";
import { ThemeService } from "@logistics/shared";
import {
  Icon,
  PhoneField,
  Stack,
  Typography,
  UiButton,
  UiChart,
  UiEditor,
  UiFileUpload,
  UiFormField,
  UiLightbox,
  UiToggleGroup,
  type UiChartType,
  type UiLightboxImage,
  type UiToggleOption,
} from "@logistics/shared/ui";
import { getLoadsPieOptions, getLoadsTypeChartOptions } from "@/shared/constants";

type Density = "compact" | "cosy";

/**
 * S10b: `ui-chart`, `ui-editor`, `ui-lightbox`, `ui-file-upload`, `ui-toggle-group`, and the
 * hand-rolled phone mask.
 *
 * Each row below pins a failure that a build, the type-checker and the whole test suite are all
 * blind to:
 *
 *   1. THE CHART RENDERS AT ALL. `chart.js` is registered piecemeal here (no `chart.js/auto`), so a
 *      missing controller/element/scale is not a compile error — it is an empty canvas, or a thrown
 *      "…is not a registered controller" at runtime. All four types are drawn.
 *   2. THE FILLED LINE. `fill: true` needs the `Filler` plugin. Without it the line still draws and
 *      nothing errors — the AREA UNDER IT just silently disappears. That is why the line row below
 *      is filled: a bare stroke means Filler got dropped from the registration list.
 *   3. `[type]` SWITCHING RECREATES THE CHART. A chart.js Chart is built around its controller and
 *      cannot be mutated into another type. Two real pages (maintenance-report, safety-report) flip
 *      bar <-> line on a data-shape change. Hit the toggle and watch it actually change.
 *   4. DARK MODE. The options carry the palette (`get*ChartOptions(isDark)`), so a chart that
 *      ignores the theme shows up as grey-on-grey axes. Flip the page's theme toggle.
 *   5. THE TOGGLE GROUP CANNOT BE DESELECTED. `p-selectButton` (allowEmpty=true) and brain's
 *      `BrnToggleGroup` (nullable=true) BOTH default to letting a click on the active segment clear
 *      the value to `null`. Three call sites are typed non-nullable. Click the active segment: the
 *      selection must stay put.
 *   6. THE MASK. Type digits, backspace over a literal (`)` / `-`), paste `+1 (555) 123-4567`.
 */
@Component({
  selector: "app-ui-lab-dataviz",
  templateUrl: "./dataviz-section.html",
  imports: [
    Icon,
    Stack,
    Typography,
    UiButton,
    UiChart,
    UiEditor,
    UiFileUpload,
    UiFormField,
    UiLightbox,
    PhoneField,
    UiToggleGroup,
  ],
})
export class UiLabDatavizSection {
  private readonly theme = inject(ThemeService);

  // ---- charts ----------------------------------------------------------------------------------

  private readonly labels = ["Mon", "Tue", "Wed", "Thu", "Fri"];

  protected readonly barData = {
    labels: this.labels,
    datasets: [{ label: "Loads", data: [12, 19, 7, 15, 9], backgroundColor: "#3b82f6" }],
  };

  /** `fill: true` — the Filler-plugin canary. A bare stroke here means Filler was not registered. */
  protected readonly lineData = {
    labels: this.labels,
    datasets: [
      {
        label: "Revenue",
        data: [1200, 1900, 700, 1500, 900],
        borderColor: "#06b6d4",
        backgroundColor: "rgba(6, 182, 212, 0.2)",
        fill: true,
        tension: 0.4,
      },
    ],
  };

  protected readonly doughnutData = {
    labels: ["Delivered", "In transit", "Cancelled"],
    datasets: [{ data: [18, 7, 3], backgroundColor: ["#22c55e", "#f59e0b", "#ef4444"] }],
  };

  protected readonly pieData = {
    labels: ["Intermodal", "Vehicle", "Freight"],
    datasets: [{ data: [9, 5, 12], backgroundColor: ["#3b82f6", "#8b5cf6", "#06b6d4"] }],
  };

  /**
   * The REAL production options, from `loads-chart.options.ts` — the same functions the loads report
   * calls. This is the whole dark-mode contract: a theme flip produces a NEW options object, and
   * `ui-chart` rebuilds on an options-identity change. Hand-rolled static options here would have
   * tested nothing — the axes would keep whatever colour they were built with, and this row would
   * look fine while production went grey-on-grey.
   */
  protected readonly chartOptions = computed(() => getLoadsTypeChartOptions(this.theme.isDark()));

  /** Pie/doughnut: no cartesian axes, so the report uses a separate options builder. */
  protected readonly radialOptions = computed(() => getLoadsPieOptions(this.theme.isDark()));

  /** Row 3: the dynamic `[type]`, which must DESTROY and rebuild the chart, not mutate it. */
  protected readonly dynamicType = signal<UiChartType>("bar");

  protected readonly dynamicTypeOptions: UiToggleOption<UiChartType>[] = [
    { label: "Bar", value: "bar" },
    { label: "Line", value: "line" },
    { label: "Doughnut", value: "doughnut" },
    { label: "Pie", value: "pie" },
  ];

  protected toggleType(type: UiChartType): void {
    this.dynamicType.set(type);
  }

  /** Row: `[data]` changes take the `update()` path (animated), not a rebuild. */
  protected readonly liveData = signal(this.barData);

  protected randomise(): void {
    this.liveData.set({
      labels: this.labels,
      datasets: [
        {
          label: "Loads",
          data: this.labels.map(() => Math.round(Math.random() * 20) + 1),
          backgroundColor: "#3b82f6",
        },
      ],
    });
  }

  // ---- toggle group ----------------------------------------------------------------------------

  protected readonly density = signal<Density>("cosy");

  protected readonly densityOptions: UiToggleOption<Density>[] = [
    { label: "Compact", value: "compact", icon: "list" },
    { label: "Cosy", value: "cosy", icon: "table" },
  ];

  /** Icon-only variant — the map layer switcher's shape (label moves to tooltip + aria-label). */
  protected readonly iconOnlyOptions: UiToggleOption<Density>[] = [
    { label: "Compact", value: "compact", icon: "list" },
    { label: "Cosy", value: "cosy", icon: "table" },
  ];

  // ---- lightbox --------------------------------------------------------------------------------

  protected readonly galleryOpen = signal(false);
  protected readonly galleryIndex = signal(0);

  /** Inline SVG data URIs — the lab makes no network requests. */
  protected readonly photos: UiLightboxImage[] = [1, 2, 3, 4].map((n) => ({
    src: this.swatch(n),
    thumbnailSrc: this.swatch(n),
    alt: `Condition photo ${n}`,
  }));

  protected openGallery(index: number): void {
    this.galleryIndex.set(index);
    this.galleryOpen.set(true);
  }

  private swatch(n: number): string {
    const colors = ["#3b82f6", "#22c55e", "#f59e0b", "#8b5cf6"];
    const svg =
      `<svg xmlns="http://www.w3.org/2000/svg" width="480" height="320">` +
      `<rect width="480" height="320" fill="${colors[n - 1]}"/>` +
      `<text x="240" y="170" font-size="48" fill="#fff" text-anchor="middle">${n}</text></svg>`;
    return `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`;
  }

  // ---- file upload -----------------------------------------------------------------------------

  protected readonly picked = signal<string[]>([]);
  protected readonly refused = signal<string | null>(null);

  protected onFiles(files: File[]): void {
    this.refused.set(null);
    this.picked.set(files.map((f) => `${f.name} (${Math.round(f.size / 1024)} KB)`));
  }

  protected onRejected(reason: string): void {
    this.refused.set(reason);
  }

  // ---- editor + phone --------------------------------------------------------------------------

  protected readonly content = signal(
    "<p>Rich text — <strong>bold</strong> and <em>italic</em>.</p>",
  );
  protected readonly phone = signal<string | null>("+15551234567");

  protected readonly phoneEcho = computed(() => this.phone() ?? "(null)");
  protected readonly contentEcho = computed(() => this.content());
}
