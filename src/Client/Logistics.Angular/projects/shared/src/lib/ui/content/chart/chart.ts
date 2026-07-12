import {
  afterNextRender,
  Component,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  input,
  untracked,
  viewChild,
} from "@angular/core";
// TYPE-ONLY. A value import here is what put 179KB of chart.js in tms-portal's INITIAL bundle —
// see `loadChartJs` below. Types are erased, so this creates no runtime edge.
import type { Chart, ChartData, ChartOptions } from "chart.js";

/** The four chart types this app actually draws. Anything else needs a new controller registered. */
export type UiChartType = "bar" | "line" | "doughnut" | "pie";

/**
 * chart.js ships two entry points. `chart.js/auto` self-registers EVERY controller, element, scale
 * and plugin (radar, polar, bubble, scatter, time scale, …), which is ~60KB min+gz of code this app
 * never draws. `chart.js` registers nothing and makes you opt in.
 *
 * This is the opt-in list, and it is derived from what the option objects and dataset builders
 * ACTUALLY use — not from a guess:
 *   - controllers: bar (10 sites), line (7), doughnut (5), pie (3)
 *   - elements:    Arc (doughnut/pie), Bar, Line, Point (line)
 *   - scales:      Category (implicit `x` on bar/line) + Linear (implicit `y`, and the explicit
 *                  `type: "linear"` y/y1 axes in loads/drivers/financials/payroll options)
 *   - plugins:     Legend + Tooltip (every options file configures both) and Filler — required by
 *                  the `fill: true` / `fill: !isBarChart` datasets in expense-analytics,
 *                  daily-gross, truck-gross, payroll, maintenance and safety. Without Filler the
 *                  area under those lines silently stops rendering.
 *
 * Deliberately NOT registered: the `Colors` plugin. `chart.js/auto` includes it, but it only ever
 * assigns a palette to datasets that define no colours — and every dataset in this app supplies its
 * own `backgroundColor`/`borderColor` (see `chart-palette.ts`). Registering it would change nothing
 * and cost bytes.
 *
 * Registration is a function rather than a top-level `Chart.register(...)` call on purpose: a
 * module-level side effect would make this file unshakeable.
 *
 * =================================================================================================
 * WHY chart.js IS LOADED WITH `await import()` AND NOT A STATIC IMPORT
 * =================================================================================================
 * Avoiding the module-level side effect was NECESSARY BUT NOT SUFFICIENT, and the shortfall was
 * measured, not guessed. With a static `import { Chart } from "chart.js"`, chart.js landed in
 * **tms-portal's INITIAL chunk** — 179KB, eagerly, on every cold load of every page:
 *
 *     main-*.js   chart.js  179 kB   <== EAGER
 *
 * The side-effect-free reasoning holds for customer-portal / admin-portal / website: they draw no
 * chart, nothing references `UiChart`, and chart.js is correctly absent from their bundles entirely.
 * TMS is different, and the difference is not about side effects at all:
 *
 *   `app.ts` (EAGER, the root component) imports `{ Spinner, UiToaster }` from the
 *   `@logistics/shared/ui` barrel -> `ui/index.ts` -> `content/index.ts` -> `chart/chart.ts`.
 *   Ten LAZY routes then use `<ui-chart>` (home, trucks, expenses, and seven report pages). A module
 *   reachable from the eager root AND shared by many lazy chunks gets hoisted by esbuild into the
 *   COMMON chunk — which is `main`. So the component, and with it all of chart.js, is pulled forward
 *   into the initial bundle.
 *
 * `await import("chart.js")` makes the eager/lazy question moot: there is no static edge for esbuild
 * to hoist, chart.js gets its own chunk, and it is fetched the first time a chart actually renders.
 * This is the same pattern `ui-editor` already uses for Quill (~43KB) — see `editor.ts`.
 */
let registered = false;

/**
 * Loads chart.js and registers the opt-in list exactly once. The dynamic `import()` is cached by the
 * module system, so every call after the first resolves from memory with no second network fetch.
 */
async function loadChartJs(): Promise<typeof import("chart.js")> {
  const m = await import("chart.js");

  if (!registered) {
    registered = true;

    m.Chart.register(
      m.BarController,
      m.LineController,
      m.DoughnutController,
      m.PieController,
      m.ArcElement,
      m.BarElement,
      m.LineElement,
      m.PointElement,
      m.CategoryScale,
      m.LinearScale,
      m.Filler,
      m.Legend,
      m.Tooltip,
    );
  }

  return m;
}

/**
 * Canvas chart. Replaces `<p-chart>` (27 call sites) with the same binding surface, so the
 * migration was a tag rename: `type` / `[type]`, `[data]`, `[options]`, plus `class` / `style`.
 *
 * `data` and `options` are typed `unknown` on purpose. `p-chart` typed both `any`, and the call
 * sites lean on that: dataset builders return `unknown` (`expense-analytics.utils.ts`) or
 * `Record<string, unknown>` (every report page), and the options files are plain object literals
 * whose tick `callback`s do not match chart.js's `Scale`-bound signatures. Under `strictTemplates`
 * a `ChartData` / `ChartOptions` input would fail to compile at all 27 sites. The cast happens once,
 * here, at the boundary.
 *
 * Lifecycle — the part that bites:
 *   - `[data]` changes  -> `chart.update()`, so the bars/lines animate to their new values.
 *   - `[type]` changes  -> full destroy + recreate. A chart.js Chart is built around its controller;
 *     you cannot mutate `config.type` in place. Two pages do this (maintenance-report and
 *     safety-report flip bar <-> line when a trend has <= 3 points).
 *   - `[options]` changes -> also a recreate. Assigning `chart.options` wholesale skips chart.js's
 *     merge with its own defaults, so scales/plugins configured by the new object can come out
 *     half-initialised. Options identity only changes on a theme flip or a type flip (they are
 *     module constants or `computed()`s), so this is rare and the recreate is invisible.
 *   - teardown -> `destroy()`. chart.js attaches a `ResizeObserver` to the canvas's parent in
 *     responsive mode; skipping `destroy()` leaks it, and the observer keeps the whole chart (and
 *     its data) alive for the life of the page.
 *
 * Resize needs no code here: `responsive: true` makes chart.js own that `ResizeObserver` itself.
 */
@Component({
  selector: "ui-chart",
  templateUrl: "./chart.html",
  // Matches `p-chart`'s host box exactly (`display: block; position: relative`). chart.js sizes a
  // responsive canvas from its parent element, so the host IS the sizing container — the call sites
  // hand it a height via `style="height: 300px"` or `class="h-full w-full"`.
  host: { class: "block relative" },
})
export class UiChart {
  public readonly type = input<UiChartType>("bar");
  public readonly data = input<unknown>(null);
  public readonly options = input<unknown>(null);

  /**
   * `p-chart` defaulted this to `true` and then force-assigned it onto the options object it was
   * given. That default is load-bearing: `maintenance-report`'s `pieChartOptions` / `barChartOptions`
   * set only `maintainAspectRatio: false` and never say `responsive`, so they inherited it from here.
   * (chart.js's own default is also `true`, so this is belt-and-braces — but it is what the call
   * sites were written against.)
   */
  public readonly responsive = input(true);

  /** A `<canvas role="img">` with no accessible name is opaque to a screen reader. */
  public readonly ariaLabel = input<string>("");

  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>("canvas");

  private chart: Chart | null = null;
  /** The type/options the live chart was BUILT with — a change in either forces a recreate. */
  private builtType: UiChartType | null = null;
  private builtOptions: unknown = null;

  /**
   * Guards the window in which `import("chart.js")` is in flight. `destroy()` and every new
   * `create()` bump it; a `create()` whose token is stale when its await resolves has been
   * superseded (a type/options flip) or orphaned (the component was destroyed mid-import) and must
   * NOT touch the canvas — `viewChild.required` throws once the view is gone, and a chart built
   * after teardown leaks its ResizeObserver forever.
   */
  private generation = 0;

  constructor() {
    afterNextRender(() => void this.create());

    effect(() => {
      const type = this.type();
      const data = this.data();
      const options = this.options();
      const responsive = this.responsive();

      untracked(() => this.sync(type, data, options, responsive));
    });

    inject(DestroyRef).onDestroy(() => this.destroy());
  }

  /**
   * Reconcile the live chart with the current inputs. A no-op before `afterNextRender` has built
   * the chart — the effect's first run happens during change detection, when the canvas is not
   * laid out yet, and `create()` reads the same signals anyway.
   */
  private sync(type: UiChartType, data: unknown, options: unknown, responsive: boolean): void {
    const chart = this.chart;
    if (!chart) {
      return;
    }

    if (type !== this.builtType || options !== this.builtOptions) {
      this.destroy();
      void this.create();
      return;
    }

    chart.data = (data ?? { labels: [], datasets: [] }) as ChartData;
    chart.options = this.resolveOptions(options, responsive);
    chart.update();
  }

  private async create(): Promise<void> {
    const token = ++this.generation;

    const { Chart } = await loadChartJs();

    // Superseded by a newer create(), or the component was destroyed, while the import() was in
    // flight. Reading `this.canvas()` after teardown throws; building a chart after teardown leaks.
    if (token !== this.generation) {
      return;
    }

    // Re-read the inputs AFTER the await — they may have moved on while chart.js was loading, and
    // `sync()` no-ops until `this.chart` exists, so this is the only place that reconciliation lands.
    const type = this.type();
    const options = this.options();

    this.chart = new Chart(this.canvas().nativeElement, {
      type,
      data: (this.data() ?? { labels: [], datasets: [] }) as ChartData,
      options: this.resolveOptions(options, this.responsive()),
    });

    this.builtType = type;
    this.builtOptions = options;
  }

  private destroy(): void {
    // Invalidates any in-flight create() — see `generation`.
    this.generation++;
    this.chart?.destroy();
    this.chart = null;
    this.builtType = null;
    this.builtOptions = null;
  }

  /**
   * Copy — never mutate. `p-chart` did `opts.responsive = this.responsive` straight onto the object
   * it was handed, which meant it wrote into the shared module-level constants exported from
   * `*-chart.options.ts` and `expense-analytics.utils.ts`. Harmless there only because the value it
   * wrote matched what those objects already said.
   */
  private resolveOptions(options: unknown, responsive: boolean): ChartOptions {
    return {
      ...((options as object | null) ?? {}),
      responsive,
    } as ChartOptions;
  }
}
