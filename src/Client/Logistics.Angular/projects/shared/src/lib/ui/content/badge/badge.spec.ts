/**
 * `ui-badge`'s contract. Everything pinned here is something that (a) 134 incoming `<p-tag>` call
 * sites depend on and (b) NO other gate would catch — each one stays green through `build:all`,
 * `lint` and every other spec while shipping a visibly wrong chip:
 *
 *   1. the 7-cell tone table — a hole emits no CSS and no warning, so `severity="danger"` would
 *      render as a transparent, unstyled chip. Silent by construction.
 *   2. the `severity` default is "info" and NOT "primary" — the wrapper-default-drift bug that has
 *      bitten this migration four times. Nothing else asserts it.
 *   3. every cell paints BOTH a background and a foreground. A cell with a background and no text
 *      colour is unreadable, not merely wrong, and it is the natural way to typo one of these.
 *   4. the call site's `class` BEATS the component's own classes (twMerge, not stylesheet order) —
 *      13 sites pass one, and `class="text-xs"` losing to the size cell would be invisible in review.
 *   5. `value` and projected content are alternatives — 4 `<p-tag>…</p-tag>` sites project.
 *   6. `rounded` is a real input now. It used to be `variant="outlined"`, which rendered a rounded
 *      SOLID tag; 8 sites bind `[rounded]` and would otherwise silently square off.
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { provideIcons } from "@ng-icons/core";
import { BASE_NG_ICONS, type IconName } from "../../icons/icon-registry.generated";
import { Badge } from "./badge";
import { UI_BADGE_TONES, type UiBadgeTone } from "./badge-intent";
import { TONE_CLASSES, uiBadgeClass, type UiBadgeSize } from "./badge-variants";

@Component({
  selector: "ui-host-badge",
  imports: [Badge],
  template: `
    <ui-badge
      [value]="value()"
      [severity]="severity()"
      [size]="size()"
      [rounded]="rounded()"
      [icon]="icon()"
      >{{ projected() }}</ui-badge
    >
  `,
})
class HostBadge {
  readonly value = signal<string | number | null>(null);
  readonly severity = signal<UiBadgeTone>("info");
  readonly size = signal<UiBadgeSize>("md");
  readonly rounded = signal(false);
  readonly icon = signal<IconName | null>(null);
  readonly projected = signal("");
}

/**
 * A STATIC `class=`, which is what all 13 `<p-tag class="…">` call sites write — and the shape the
 * merge is actually specified for. `classes()` harvests the class attribute via `HostAttributeToken`
 * at construction and twMerges it, so a static class genuinely WINS (our `text-[length:…]` is
 * dropped). A `[class]` binding whose value changes after first render is appended by the
 * MutationObserver without a re-merge — so a test driven off a mutable signal would be asserting a
 * shape no call site has. No p-tag / p-card / p-skeleton / p-divider site binds `[class]`.
 */
@Component({
  selector: "ui-host-badge-class",
  imports: [Badge],
  template: `<ui-badge value="x" class="text-xs whitespace-nowrap" />`,
})
class HostBadgeClass {}

describe("ui-badge", () => {
  let fixture: ComponentFixture<HostBadge>;
  let host: HostBadge;

  const el = (): HTMLElement => fixture.nativeElement.querySelector("ui-badge");

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HostBadge],
      providers: [provideZonelessChangeDetection(), provideIcons(BASE_NG_ICONS)],
    }).compileComponents();
    fixture = TestBed.createComponent(HostBadge);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  // 1 + 3. The table is total, and no cell is half-painted.
  describe("the tone table", () => {
    it("has a cell for every tone in the vocabulary", () => {
      for (const tone of UI_BADGE_TONES) {
        expect(TONE_CLASSES[tone], `no cell for tone "${tone}"`).toBeTruthy();
      }
      expect(Object.keys(TONE_CLASSES).sort()).toEqual([...UI_BADGE_TONES].sort());
    });

    it("paints a background AND a foreground in every cell", () => {
      for (const tone of UI_BADGE_TONES) {
        const cell = TONE_CLASSES[tone];
        expect(/(^|\s)(bg-|\[background-color:)/.test(cell), `"${tone}" has no background`).toBe(
          true,
        );
        expect(/(^|\s)(text-|\[color:)/.test(cell), `"${tone}" has no text colour`).toBe(true);
      }
    });

    it("resolves every colour through a token — no hex, no rgb(), no named colour", () => {
      for (const tone of UI_BADGE_TONES) {
        expect(TONE_CLASSES[tone]).not.toMatch(/#[0-9a-f]{3,8}\b|rgba?\(|hsla?\(/i);
      }
    });
  });

  // 2. The default that four previous bugs were made of.
  it('defaults `severity` to "info" — NOT "primary" (a bare PrimeNG p-tag was primary; ui-badge is not)', () => {
    fixture.detectChanges();
    expect(el().className).toContain("--ui-badge-info-bg");
    expect(el().className).not.toContain("--ui-badge-primary-bg");
  });

  it('paints "primary" when asked — the one severity-less p-tag in the repo needed it', () => {
    host.severity.set("primary");
    fixture.detectChanges();
    expect(el().className).toContain("--ui-badge-primary-bg");
  });

  // 4. twMerge, not stylesheet roulette.
  it("lets the call site's `class` BEAT the size cell — the md font-size is dropped, not outranked", () => {
    const f = TestBed.createComponent(HostBadgeClass);
    f.detectChanges();
    const cls = (f.nativeElement.querySelector("ui-badge") as HTMLElement).className;
    expect(cls).toContain("text-xs");
    expect(cls).not.toContain("--ui-badge-font-size");
    // …while a non-conflicting class rides along, and the tone survives untouched.
    expect(cls).toContain("whitespace-nowrap");
    expect(cls).toContain("--ui-badge-info-bg");
  });

  // 5. Value vs projection.
  it("renders `value` when set", () => {
    host.value.set("Delivered");
    fixture.detectChanges();
    expect(el().textContent?.trim()).toBe("Delivered");
  });

  it("falls back to projected content when `value` is null — 4 content-projecting p-tag sites relied on it", () => {
    host.value.set(null);
    host.projected.set("Cancels at period end");
    fixture.detectChanges();
    expect(el().textContent?.trim()).toBe("Cancels at period end");
  });

  it("renders a numeric zero rather than treating it as empty", () => {
    host.value.set(0);
    host.projected.set("should not appear");
    fixture.detectChanges();
    expect(el().textContent?.trim()).toBe("0");
  });

  /**
   * 6. `rounded` is a real input now (8 sites bind it), and it resolves through `--ui-radius-pill`
   * rather than Tailwind's `rounded-xl` — that utility is 16px in TMS and 12px in the other three
   * apps (variables.css re-points `--radius-xl`), which would have invented a 4px fork where
   * PrimeNG's `tag.roundedBorderRadius` (12px) has none.
   */
  it("rounds through the pill token when `rounded`, and to the content radius otherwise", () => {
    expect(el().className).toContain("rounded-[var(--ui-radius-content)]");
    host.rounded.set(true);
    fixture.detectChanges();
    expect(el().className).toContain("rounded-[var(--ui-radius-pill)]");
    expect(el().className).not.toContain("rounded-[var(--ui-radius-content)]");
    expect(el().className).not.toContain("rounded-xl");
  });

  // The icon is a real glyph now, not a `pi pi-*` class string.
  it("renders `icon` as an <ng-icon>, not as a class", () => {
    host.icon.set("check-circle");
    fixture.detectChanges();
    expect(el().querySelector("ng-icon")).toBeTruthy();
    expect(el().className).not.toContain("pi-");
  });

  it("renders no icon element when `icon` is null", () => {
    expect(el().querySelector("ng-icon")).toBeNull();
  });

  // The sizing knobs the TMS preset fork rides on.
  it("reads the per-app sizing variables at the default size, and only there", () => {
    const md = uiBadgeClass({ tone: "info", size: "md", rounded: false });
    expect(md).toContain("--ui-badge-font-size");
    expect(md).toContain("--ui-badge-px");

    for (const size of ["sm", "lg"] as const) {
      expect(uiBadgeClass({ tone: "info", size, rounded: false })).not.toContain("--ui-badge-px");
    }
  });
});
