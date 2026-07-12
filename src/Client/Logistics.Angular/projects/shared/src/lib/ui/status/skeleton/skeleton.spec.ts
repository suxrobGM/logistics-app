/**
 * `ui-skeleton` / `ui-spinner` / `ui-avatar` / `ui-progress` / `ui-divider` — the small ones.
 * Grouped in one file because each has exactly one or two things worth pinning, and all of them are
 * of the same kind: a silent default, or a call-site class that has to win a merge.
 *
 * EVERY CLASS-MERGE TEST USES A STATIC `class=`, and that is the point rather than a shortcut.
 * `classes()` harvests the class attribute through `HostAttributeToken` at construction and twMerges
 * it, so a static class genuinely wins — the component's own conflicting utility is DROPPED. A
 * `[class]` binding whose value changes after first render is appended by the MutationObserver
 * without re-merging, so a test driven off a mutable signal asserts a shape no call site has and
 * "fails" on a merge bug that does not exist in the app. Not one p-skeleton / p-divider /
 * p-progressBar call site binds `[class]`; all of them write it statically.
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { Avatar } from "../../display/avatar/avatar";
import { Divider } from "../../layout/divider/divider";
import { Progress } from "../progress/progress";
import { Spinner } from "../spinner/spinner";
import { Skeleton } from "./skeleton";

describe("ui-skeleton", () => {
  @Component({
    selector: "ui-host-skeleton",
    imports: [Skeleton],
    template: `<ui-skeleton
      [width]="width()"
      [height]="height()"
      [size]="size()"
      [shape]="shape()"
    />`,
  })
  class Host {
    readonly width = signal<string | null>(null);
    readonly height = signal<string | null>(null);
    readonly size = signal<string | null>(null);
    readonly shape = signal<"rect" | "circle">("rect");
  }

  @Component({
    selector: "ui-host-skeleton-class",
    imports: [Skeleton],
    template: `<ui-skeleton class="bg-purple-500/20" />`,
  })
  class HostClass {}

  let fixture: ComponentFixture<Host>;
  let host: Host;
  const el = (): HTMLElement => fixture.nativeElement.querySelector("ui-skeleton");

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Host],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(Host);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("applies width/height as CSS lengths, not classes — call sites pass '3rem' and '120px'", () => {
    host.width.set("120px");
    host.height.set("3rem");
    fixture.detectChanges();
    expect(el().style.width).toBe("120px");
    expect(el().style.height).toBe("3rem");
  });

  it("`size` drives BOTH axes — a circle whose axes disagree is an ellipse", () => {
    host.size.set("3rem");
    host.shape.set("circle");
    fixture.detectChanges();
    expect(el().style.width).toBe("3rem");
    expect(el().style.height).toBe("3rem");
    expect(el().className).toContain("rounded-full");
  });

  it("lets a call site repaint the background — 4 sites pass bg-purple-500/20", () => {
    const f = TestBed.createComponent(HostClass);
    f.detectChanges();
    const cls = (f.nativeElement.querySelector("ui-skeleton") as HTMLElement).className;
    expect(cls).toContain("bg-purple-500/20");
    // twMerge must have DROPPED bg-muted, not merely been outranked in stylesheet order.
    expect(cls).not.toContain("bg-muted");
  });
});

describe("ui-spinner", () => {
  @Component({
    selector: "ui-host-spinner",
    imports: [Spinner],
    template: `<ui-spinner [size]="size()" />`,
  })
  class Host {
    readonly size = signal("100px");
  }

  let fixture: ComponentFixture<Host>;
  let host: Host;
  const el = (): HTMLElement => fixture.nativeElement.querySelector("ui-spinner");

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Host],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(Host);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  /**
   * 50 of 63 call sites pass NOTHING, so this default IS the spinner in fifty places; changing it
   * reflows all fifty.
   */
  it("defaults to p-progress-spinner's 100px box", () => {
    const spinner = TestBed.createComponent(Spinner);
    spinner.detectChanges();
    const style = (spinner.nativeElement as HTMLElement).style;
    expect(style.width).toBe("100px");
    expect(style.height).toBe("100px");
  });

  it("sizes the glyph off the host's font-size, so the box and the glyph never disagree", () => {
    host.size.set("24px");
    fixture.detectChanges();
    expect(el().style.width).toBe("24px");
    expect(el().style.fontSize).toBe("24px");
  });

  it("announces itself — role=status plus a name", () => {
    expect(el().getAttribute("role")).toBe("status");
    expect(el().getAttribute("aria-label")).toBe("Loading");
  });

  it("spins", () => {
    expect(el().querySelector("ng-icon")?.className).toContain("animate-spin");
  });
});

describe("ui-avatar", () => {
  @Component({
    selector: "ui-host-avatar",
    imports: [Avatar],
    template: `<ui-avatar [label]="label()" [size]="size()" [style]="style()" />`,
  })
  class Host {
    readonly label = signal("SI");
    readonly size = signal<"normal" | "large" | "xlarge">("normal");
    readonly style = signal<Record<string, string>>({});
  }

  let fixture: ComponentFixture<Host>;
  let host: Host;
  const el = (): HTMLElement => fixture.nativeElement.querySelector("ui-avatar");

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Host],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(Host);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("renders initials in a circle", () => {
    expect(el().textContent?.trim()).toBe("SI");
    expect(el().className).toContain("rounded-full");
  });

  it("maps the three named sizes to 2 / 3 / 4rem", () => {
    expect(el().className).toContain("size-8");
    host.size.set("large");
    fixture.detectChanges();
    expect(el().className).toContain("size-12");
    host.size.set("xlarge");
    fixture.detectChanges();
    expect(el().className).toContain("size-16");
  });

  /** app-user-avatar tints its circle per user by binding `[style]`. The host IS the circle, so it works. */
  it("lets the call site paint the circle through [style] — app-user-avatar does exactly this", () => {
    host.style.set({ "background-color": "rgb(1, 2, 3)", color: "rgb(4, 5, 6)" });
    fixture.detectChanges();
    expect(el().style.backgroundColor).toBe("rgb(1, 2, 3)");
    expect(el().style.color).toBe("rgb(4, 5, 6)");
  });
});

describe("ui-progress", () => {
  @Component({
    selector: "ui-host-progress",
    imports: [Progress],
    template: `<ui-progress [value]="value()" [showValue]="showValue()" [color]="color()" />`,
  })
  class Host {
    readonly value = signal<number | null>(40);
    readonly showValue = signal(false);
    readonly color = signal<string | null>(null);
  }

  @Component({
    selector: "ui-host-progress-class",
    imports: [Progress],
    template: `<ui-progress [value]="40" class="h-1" />`,
  })
  class HostClass {}

  let fixture: ComponentFixture<Host>;
  let host: Host;
  const el = (): HTMLElement => fixture.nativeElement.querySelector("ui-progress");

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Host],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(Host);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  /** The reason this is Helm/Brain-backed at all. A hand-rolled div would have none of this. */
  it("is a real progressbar to a screen reader", () => {
    const track = el().querySelector('[role="progressbar"]');
    expect(track).toBeTruthy();
    expect(track?.getAttribute("aria-valuenow")).toBe("40");
    expect(track?.getAttribute("aria-valuemax")).toBe("100");
  });

  it("hides the label unless asked, and shows a rounded percentage when asked", () => {
    expect(el().textContent?.trim()).toBe("");
    host.value.set(42.4);
    host.showValue.set(true);
    fixture.detectChanges();
    expect(el().textContent?.trim()).toBe("42%");
  });

  it("paints the FILL with `color`, leaving the track alone", () => {
    host.color.set("rgb(9, 9, 9)");
    fixture.detectChanges();
    const fill = el().querySelector<HTMLElement>('[data-slot="progress-indicator"]');
    expect(fill?.style.backgroundColor).toBe("rgb(9, 9, 9)");
  });

  /**
   * The height default is a CLASS, not an inline style, precisely so this wins. As an inline style
   * it would have beaten the un-`!`-ed `h-1` and silently pinned that bar at 1.25rem.
   */
  it("lets a call site's height class beat the default height", () => {
    const f = TestBed.createComponent(HostClass);
    f.detectChanges();
    const cls = (f.nativeElement.querySelector("ui-progress") as HTMLElement).className;
    expect(cls).toContain("h-1");
    expect(cls).not.toContain("h-5");
  });
});

describe("ui-divider", () => {
  @Component({
    selector: "ui-host-divider",
    imports: [Divider],
    template: `<ui-divider />`,
  })
  class Host {}

  @Component({
    selector: "ui-host-divider-m0",
    imports: [Divider],
    template: `<ui-divider class="m-0" />`,
  })
  class HostM0 {}

  let fixture: ComponentFixture<Host>;
  const el = (): HTMLElement => fixture.nativeElement.querySelector("ui-divider");

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
  });

  it("carries p-divider's 1rem margin on the HOST", () => {
    expect(el().className).toContain("my-4");
  });

  /**
   * 15 of the 58 incoming `<p-divider>`s are `class="m-0"`. The margin has to be ON THE HOST for
   * that to mean anything — it used to sit on an inner div, where `m-0` cancelled nothing at all.
   */
  it('lets `class="m-0"` cancel it — 15 call sites do exactly that', () => {
    const f = TestBed.createComponent(HostM0);
    f.detectChanges();
    const cls = (f.nativeElement.querySelector("ui-divider") as HTMLElement).className;
    expect(cls).toContain("m-0");
    expect(cls).not.toContain("my-4");
  });

  /** A bare `<ui-divider />` drew a BROKEN line: the empty label span's px-3 punched a 1.5rem gap
   *  through the middle of the rule. 43 of the 58 incoming dividers are bare. */
  it("draws a continuous rule when it has no label", () => {
    const label = el().querySelector("span");
    expect(label?.textContent?.trim()).toBe("");
    expect(label?.className).toContain("empty:hidden");
  });

  it("is a separator to a screen reader", () => {
    expect(el().getAttribute("role")).toBe("separator");
  });
});
