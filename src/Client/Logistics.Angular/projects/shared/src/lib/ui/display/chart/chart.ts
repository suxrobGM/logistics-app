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
// Must stay TYPE-ONLY. `UiChart` is reachable from the eager root component via the shared barrel,
// so any static value edge to chart.js gets hoisted into tms-portal's initial chunk (~179KB). The
// `await import()` in `loadChartJs` is what keeps it in its own lazily-fetched chunk.
import type { Chart, ChartData, ChartOptions } from "chart.js";

/** The four chart types this app draws. Anything else needs a new controller registered below. */
export type UiChartType = "bar" | "line" | "doughnut" | "pie";

/**
 * Bare `chart.js` registers nothing (unlike `chart.js/auto`, which pulls in every controller, scale
 * and plugin), so this is the opt-in list. `Filler` is not optional: the `fill: true` datasets in the
 * report pages silently stop shading the area under their lines without it.
 *
 * Registration lives in a function, not a top-level `Chart.register(...)`: a module-level side effect
 * would make this file unshakeable.
 */
let registered = false;

/**
 * Loads chart.js and registers the opt-in list exactly once. The dynamic `import()` is cached by the
 * module system, so later calls resolve from memory with no second fetch.
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
 * Canvas chart.
 *
 * `data` and `options` are `unknown` on purpose: the dataset builders and the options files are
 * loosely typed object literals whose tick `callback`s do not match chart.js's `Scale`-bound
 * signatures, so a `ChartData` / `ChartOptions` input would fail `strictTemplates` at every call
 * site. The cast happens once, here, at the boundary.
 *
 * Lifecycle:
 *   - `[data]` changes -> `chart.update()`, so the bars/lines animate.
 *   - `[type]` or `[options]` changes -> destroy + recreate. A Chart is built around its controller,
 *     so `config.type` cannot be mutated in place; and assigning `chart.options` wholesale skips
 *     chart.js's merge with its defaults, leaving scales/plugins half-initialised.
 *   - teardown -> `destroy()`, which is mandatory: in responsive mode chart.js attaches a
 *     `ResizeObserver` to the canvas's parent that otherwise keeps the chart and its data alive.
 *
 * Resize needs no code here — `responsive: true` makes chart.js own that `ResizeObserver` itself.
 */
@Component({
  selector: "ui-chart",
  templateUrl: "./chart.html",
  // chart.js sizes a responsive canvas from its parent, so the host IS the sizing container — call
  // sites give it a height via `style="height: 300px"` or `class="h-full w-full"`.
  host: { class: "block relative" },
})
export class UiChart {
  public readonly type = input<UiChartType>("bar");
  public readonly data = input<unknown>(null);
  public readonly options = input<unknown>(null);

  /**
   * Forced onto the resolved options. The `true` default is load-bearing: several options objects
   * (e.g. maintenance-report's) never say `responsive` and rely on inheriting it from here.
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
   * Guards the window in which `import("chart.js")` is in flight. `destroy()` and every `create()`
   * bump it; a `create()` whose token is stale when its await resolves has been superseded or
   * orphaned and must NOT touch the canvas — `viewChild.required` throws once the view is gone, and
   * a chart built after teardown leaks its ResizeObserver forever.
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
   * Reconcile the live chart with the current inputs. A no-op before `afterNextRender` has built the
   * chart — the effect's first run happens during change detection, when the canvas is not laid out
   * yet, and `create()` reads the same signals anyway.
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

    // Superseded by a newer create(), or destroyed, while the import() was in flight. Reading
    // `this.canvas()` after teardown throws; building a chart after teardown leaks.
    if (token !== this.generation) {
      return;
    }

    // Re-read the inputs AFTER the await — they may have moved on while chart.js was loading, and
    // `sync()` no-ops until `this.chart` exists, so this is the only place reconciliation lands.
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
   * Copy — never mutate. The options objects handed in are shared module-level constants (the
   * `*-chart.options.ts` files), so assigning `responsive` onto them would write into state other
   * charts read.
   */
  private resolveOptions(options: unknown, responsive: boolean): ChartOptions {
    return {
      ...((options as object | null) ?? {}),
      responsive,
    } as ChartOptions;
  }
}
