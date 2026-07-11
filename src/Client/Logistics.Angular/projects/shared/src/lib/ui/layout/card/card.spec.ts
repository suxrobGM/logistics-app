/**
 * `ui-card`'s contract. The three things pinned here are the three that would ship broken:
 *
 *   1. THE 83 TEMPLATE SLOTS. `<ng-template #header>` x71, `#title` x8, `#subtitle` x4, `#footer` x3.
 *      The brief for this step asserted p-card was "100% plain content projection, ZERO templates" —
 *      it is not, because PrimeNG 21 renamed `pTemplate="header"` to a plain `#header` ref and a grep
 *      for the old spelling finds nothing. A card that ignores them drops the heading off 71 panels
 *      and still compiles, still lints, and still passes every other test.
 *   2. `header` (the STRING, 43 sites) renders in card-TITLE, not card-HEADER. They are different
 *      boxes with different padding — the header sits outside the body. Getting it backwards moves
 *      43 headings by 1.25rem and nothing complains.
 *   3. The call site's `class` reaches the host and beats our own (86 sites pass one).
 */
import { NgTemplateOutlet } from "@angular/common";
import {
  Component,
  contentChild,
  provideZonelessChangeDetection,
  signal,
  TemplateRef,
} from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { Card } from "./card";

/**
 * The class-merge hosts use a STATIC `class=`, which is what every one of the 86 call sites writes.
 * That is not incidental: `classes()` harvests the class attribute via `HostAttributeToken` at
 * construction and twMerges it, so a static class is genuinely merged (the card's own
 * `rounded-[…]` is DROPPED, not merely outranked). A `[class]` binding whose value CHANGES after
 * first render is appended by the MutationObserver without re-merging — so a test written against a
 * mutable signal would be testing a shape no call site has, and would report a merge failure that
 * does not exist in the app. No p-card / p-tag / p-skeleton / p-divider site binds `[class]`.
 */
@Component({
  selector: "ui-host-card-class",
  imports: [Card],
  template: `<ui-card class="mb-4 rounded-none">x</ui-card>`,
})
class HostCardClass {}

@Component({
  selector: "ui-host-card",
  imports: [Card],
  template: `
    <ui-card [header]="header()">
      @if (withHeaderTpl()) {
        <ng-template #header><h5 data-test="hdr">Header template</h5></ng-template>
      }
      @if (withTitleTpl()) {
        <ng-template #title><span data-test="ttl">Title template</span></ng-template>
      }
      @if (withSubtitleTpl()) {
        <ng-template #subtitle><span data-test="sub">Subtitle template</span></ng-template>
      }
      @if (withFooterTpl()) {
        <ng-template #footer><span data-test="ftr">Footer template</span></ng-template>
      }
      <p data-test="body">Body content</p>
    </ui-card>
  `,
})
class HostCard {
  readonly header = signal<string | null>(null);
  readonly withHeaderTpl = signal(false);
  readonly withTitleTpl = signal(false);
  readonly withSubtitleTpl = signal(false);
  readonly withFooterTpl = signal(false);
}

/**
 * A stand-in for `<ui-data-table>`: a CHILD COMPONENT that owns its own `#header` slot.
 *
 * This is the shape every one of the 48 list pages actually writes, and the one the tests above miss
 * — they only ever put `<ng-template #header>` as a DIRECT child of `<ui-card>`, which is the one
 * arrangement where a descending query and a non-descending query agree.
 */
@Component({
  selector: "ui-fake-table",
  template: `<div data-test="table-header-host">
    <ng-container *ngTemplateOutlet="headerTpl() ?? null" />
  </div>`,
  imports: [NgTemplateOutlet],
})
class FakeTable {
  readonly headerTpl = contentChild<TemplateRef<unknown>>("header", { descendants: false });
}

@Component({
  selector: "ui-host-nested",
  imports: [Card, FakeTable],
  template: `
    <ui-card header="Customers">
      <ui-fake-table>
        <ng-template #header><th data-test="table-th">Name</th></ng-template>
      </ui-fake-table>
    </ui-card>
  `,
})
class HostNested {}

describe("ui-card", () => {
  let fixture: ComponentFixture<HostCard>;
  let host: HostCard;

  const card = (): HTMLElement => fixture.nativeElement.querySelector("ui-card");
  const slot = (name: string): HTMLElement | null =>
    card().querySelector(`[data-slot="card-${name}"]`);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HostCard],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(HostCard);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("always projects its default content into card-content", () => {
    expect(slot("content")?.querySelector('[data-test="body"]')).toBeTruthy();
  });

  // 1. The slots the brief said did not exist.
  describe("the 83 template slots", () => {
    it("renders a #header template into card-header", () => {
      host.withHeaderTpl.set(true);
      fixture.detectChanges();
      expect(slot("header")?.querySelector('[data-test="hdr"]')).toBeTruthy();
    });

    it("renders a #title template into card-title", () => {
      host.withTitleTpl.set(true);
      fixture.detectChanges();
      expect(slot("title")?.querySelector('[data-test="ttl"]')).toBeTruthy();
    });

    it("renders a #subtitle template into card-subtitle", () => {
      host.withSubtitleTpl.set(true);
      fixture.detectChanges();
      expect(slot("subtitle")?.querySelector('[data-test="sub"]')).toBeTruthy();
    });

    it("renders a #footer template into card-footer", () => {
      host.withFooterTpl.set(true);
      fixture.detectChanges();
      expect(slot("footer")?.querySelector('[data-test="ftr"]')).toBeTruthy();
    });

    it("emits no header / title / subtitle / footer box when none is supplied", () => {
      for (const name of ["header", "title", "subtitle", "footer"]) {
        expect(slot(name), `card-${name} should not exist`).toBeNull();
      }
    });

    it("keeps card-header OUTSIDE card-body — they have different padding", () => {
      host.withHeaderTpl.set(true);
      fixture.detectChanges();
      expect(slot("body")?.contains(slot("header"))).toBe(false);
    });

    /**
     * THE REGRESSION THAT SHIPPED. `contentChild()` defaults to `descendants: true`; p-card declared
     * every slot `@ContentChild('header', { descendants: false })`. `#header` / `#title` / `#footer`
     * are ALSO `ui-data-table`'s slot names, and 48 pages nest a table inside a card — so a descending
     * card STEALS the table's header row and renders `<tr><th>` into card-header, orphaned outside any
     * `<table>`. Build green, lint green, all 236 tests green; /customers read
     * "Name↑↓StatusEmail↑↓Phone↑↓Created↑↓" above the card.
     *
     * The tests above could not catch it: they only place templates as DIRECT children, the one
     * arrangement in which both query kinds behave identically.
     */
    it("does NOT steal a #header belonging to a NESTED component (descendants: false)", async () => {
      const nested = TestBed.createComponent(HostNested);
      nested.detectChanges();
      const el: HTMLElement = nested.nativeElement;

      // The child component still gets its own header template...
      expect(
        el.querySelector('[data-test="table-header-host"] [data-test="table-th"]'),
      ).toBeTruthy();

      // ...and the card must NOT have rendered a card-header box from it.
      const cardHeader = el.querySelector('ui-card > [data-slot="card-header"]');
      expect(cardHeader, "ui-card hijacked a nested component's #header template").toBeNull();

      // The card's own `header` string still lands in card-title, untouched.
      expect(el.querySelector('[data-slot="card-title"]')?.textContent?.trim()).toBe("Customers");
    });
  });

  // 2. The 43 `header="…"` sites.
  describe("the `header` string input", () => {
    it("renders into card-TITLE, not card-header", () => {
      host.header.set("Quick Actions");
      fixture.detectChanges();
      expect(slot("title")?.textContent?.trim()).toBe("Quick Actions");
      expect(slot("header")).toBeNull();
    });

    it("yields to a #title template, as p-card did", () => {
      host.header.set("Quick Actions");
      host.withTitleTpl.set(true);
      fixture.detectChanges();
      expect(slot("title")?.textContent).toContain("Title template");
      expect(slot("title")?.textContent).not.toContain("Quick Actions");
    });
  });

  // 3. The 86 `class=` sites.
  describe("the host class", () => {
    it("carries the card's own surface and radius", () => {
      const cls = card().className;
      expect(cls).toContain("bg-card");
      expect(cls).toContain("rounded-[var(--ui-radius-card)]");
      expect(cls).toContain("shadow-[var(--ui-card-shadow)]");
    });

    it("lets a call-site class BEAT it — twMerge drops ours, it does not merely outrank it", () => {
      const f = TestBed.createComponent(HostCardClass);
      f.detectChanges();
      const cls = (f.nativeElement.querySelector("ui-card") as HTMLElement).className;
      expect(cls).toContain("rounded-none");
      expect(cls).not.toContain("rounded-[var(--ui-radius-card)]");
      // …while a non-conflicting class simply rides along.
      expect(cls).toContain("mb-4");
      expect(cls).toContain("bg-card");
    });
  });
});
