/**
 * `ui-card`'s contract. Three things, each of which breaks silently — build, lint and every other
 * spec stay green:
 *
 *   1. the `#header` / `#title` / `#subtitle` / `#footer` template slots are rendered at all;
 *   2. the `header` STRING renders in card-title, not card-header (different boxes, different
 *      padding — the header sits outside the body);
 *   3. a call site's `class` reaches the host and beats the card's own.
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
 * The class-merge host must use a STATIC `class=` — the shape every call site writes. `classes()`
 * harvests the class attribute via `HostAttributeToken` at construction and twMerges it, so a static
 * class is genuinely merged (the card's own `rounded-[…]` is DROPPED, not merely outranked). A
 * `[class]` binding whose value changes after first render is appended by the MutationObserver
 * without re-merging, so a test driven from a mutable signal would report a merge failure that does
 * not exist in the app.
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
 * A stand-in for `<ui-data-table>`: a child component that owns its own `#header` slot. This is the
 * shape the list pages write, and the only one that can catch a descending `contentChild` — the
 * other tests place templates as DIRECT children, where both query kinds behave identically.
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
     * `contentChild()` defaults to `descendants: true`, and `#header` / `#title` / `#footer` are also
     * `ui-data-table`'s slot names. With the default, a card containing a table steals the table's
     * header row and renders `<tr><th>` into card-header, orphaned outside any `<table>` — while the
     * build, the lint and every other test stay green.
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

  describe("the `header` string input", () => {
    it("renders into card-TITLE, not card-header", () => {
      host.header.set("Quick Actions");
      fixture.detectChanges();
      expect(slot("title")?.textContent?.trim()).toBe("Quick Actions");
      expect(slot("header")).toBeNull();
    });

    it("yields to a #title template", () => {
      host.header.set("Quick Actions");
      host.withTitleTpl.set(true);
      fixture.detectChanges();
      expect(slot("title")?.textContent).toContain("Title template");
      expect(slot("title")?.textContent).not.toContain("Quick Actions");
    });
  });

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
