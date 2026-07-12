/**
 * `ui-badge`'s contract. Each item below would ship a visibly wrong chip while staying green through
 * the build, the lint and every other spec:
 *
 *   1. the tone table has a cell for every tone — a hole emits no CSS and no warning.
 *   2. the `severity` default is "info", not "primary". Nothing else asserts it.
 *   3. every cell paints BOTH a background and a foreground; a background with no text colour is
 *      unreadable, and it is the natural way to typo one of these.
 *   4. the call site's `class` beats the component's own classes (twMerge, not stylesheet order).
 *      Hence NAMED utilities, never arbitrary properties, which land in their own merge group and so
 *      survive alongside the call site's class instead of losing to it.
 *   5. `value` and projected content are alternatives.
 *   6. `rounded` is a real input, resolved through the shared radius tokens.
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import type { IconName } from "../../icons/icons";
import { Badge } from "./badge";
import { UI_BADGE_INTENTS, type UiBadgeIntent } from "./badge-intent";
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
  readonly severity = signal<UiBadgeIntent>("info");
  readonly size = signal<UiBadgeSize>("md");
  readonly rounded = signal(false);
  readonly icon = signal<IconName | null>(null);
  readonly projected = signal("");
}

/**
 * A STATIC `class=`, which is the shape the merge is specified for: `classes()` harvests the class
 * attribute via `HostAttributeToken` at construction and twMerges it, so a static class wins and the
 * size cell's font-size is dropped. A `[class]` binding that changes after first render is appended by
 * the MutationObserver WITHOUT a re-merge, so a test driven off a mutable signal would assert a shape
 * no call site has.
 *
 * `text-lg`, not `text-xs`: the md cell IS `text-xs`, so passing the same value could not tell "won
 * the merge" apart from "was ignored".
 */
@Component({
  selector: "ui-host-badge-class",
  imports: [Badge],
  template: `<ui-badge value="x" class="text-lg whitespace-nowrap" />`,
})
class HostBadgeClass {}

describe("ui-badge", () => {
  let fixture: ComponentFixture<HostBadge>;
  let host: HostBadge;

  const el = (): HTMLElement => fixture.nativeElement.querySelector("ui-badge");

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HostBadge],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(HostBadge);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  // 1 + 3. The table is total, and no cell is half-painted.
  describe("the tone table", () => {
    it("has a cell for every tone in the vocabulary", () => {
      for (const tone of UI_BADGE_INTENTS) {
        expect(TONE_CLASSES[tone], `no cell for tone "${tone}"`).toBeTruthy();
      }
      expect(Object.keys(TONE_CLASSES).sort()).toEqual([...UI_BADGE_INTENTS].sort());
    });

    it("paints a background AND a foreground in every cell", () => {
      for (const tone of UI_BADGE_INTENTS) {
        const cell = TONE_CLASSES[tone];
        expect(/(^|\s)bg-/.test(cell), `"${tone}" has no background`).toBe(true);
        expect(/(^|\s)text-/.test(cell), `"${tone}" has no text colour`).toBe(true);
      }
    });

    it("resolves every colour through a token — no hex, no rgb(), no named colour", () => {
      for (const tone of UI_BADGE_INTENTS) {
        expect(TONE_CLASSES[tone]).not.toMatch(/#[0-9a-f]{3,8}\b|rgba?\(|hsla?\(/i);
      }
    });

    /**
     * The merge trap, pinned. An arbitrary PROPERTY is its own tailwind-merge class group, so it does
     * NOT dedupe against a call site's `bg-*` — both survive and source order decides. Reintroducing
     * one would silently break the contract test 4 asserts.
     */
    it("spells every cell as a NAMED utility — never as an arbitrary property", () => {
      for (const tone of UI_BADGE_INTENTS) {
        expect(TONE_CLASSES[tone], `"${tone}" uses an arbitrary property`).not.toMatch(
          /\[(background-color|color):/,
        );
      }
    });
  });

  // 2.
  it('defaults `severity` to "info" — NOT "primary"', () => {
    fixture.detectChanges();
    expect(el().className).toContain("bg-info");
    expect(el().className).not.toContain("bg-primary");
  });

  it('paints "primary" when asked', () => {
    host.severity.set("primary");
    fixture.detectChanges();
    expect(el().className).toContain("bg-primary");
  });

  // 4. twMerge, not stylesheet order.
  it("lets the call site's `class` BEAT the size cell — the md font-size is dropped, not outranked", () => {
    const f = TestBed.createComponent(HostBadgeClass);
    f.detectChanges();
    const cls = (f.nativeElement.querySelector("ui-badge") as HTMLElement).className;
    expect(cls).toContain("text-lg");
    expect(cls).not.toContain("text-xs");
    // A non-conflicting class rides along, and the tone survives: its ink is a text-COLOR, a different
    // merge group from the font-size, so `text-lg` must not evict it.
    expect(cls).toContain("whitespace-nowrap");
    expect(cls).toContain("bg-info");
    expect(cls).toContain("text-[var(--inverse)]");
  });

  // 5. Value vs projection.
  it("renders `value` when set", () => {
    host.value.set("Delivered");
    fixture.detectChanges();
    expect(el().textContent?.trim()).toBe("Delivered");
  });

  it("falls back to projected content when `value` is null", () => {
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

  // 6. `rounded` resolves through `--ui-radius-pill`, not Tailwind's `rounded-xl` (a different size).
  it("rounds through the pill token when `rounded`, and to the content radius otherwise", () => {
    expect(el().className).toContain("rounded-[var(--ui-radius-content)]");
    host.rounded.set(true);
    fixture.detectChanges();
    expect(el().className).toContain("rounded-[var(--ui-radius-pill)]");
    expect(el().className).not.toContain("rounded-[var(--ui-radius-content)]");
    expect(el().className).not.toContain("rounded-xl");
  });

  it("renders `icon` as an <ng-icon>, not as a class", () => {
    host.icon.set("circle-check");
    fixture.detectChanges();
    expect(el().querySelector("ng-icon")).toBeTruthy();
    expect(el().className).not.toContain("pi-");
  });

  it("renders no icon element when `icon` is null", () => {
    expect(el().querySelector("ng-icon")).toBeNull();
  });

  it("applies the md metrics at the default size, and only there", () => {
    const md = uiBadgeClass({ tone: "info", size: "md", rounded: false });
    expect(md).toContain("px-2.5");
    expect(md).toContain("py-1");

    for (const size of ["sm", "lg"] as const) {
      expect(uiBadgeClass({ tone: "info", size, rounded: false })).not.toContain("px-2.5");
    }
  });
});
