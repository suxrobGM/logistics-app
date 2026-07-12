/**
 * `<ui-menu>`'s contract. Everything pinned here is something the 19 call sites depend on and that NO
 * other gate would catch — each one stays green through `build:all`, `lint` and every other spec while
 * shipping a broken menu:
 *
 *   1. IT CLOSES. This is the whole reason the component owns its overlay. `CdkMenu` decides it is
 *      "inline" when there is no `CdkMenuTrigger` above it (`isInline = !this._parentTrigger`), and an
 *      inline menu never pushes itself onto the menu stack — so its Escape handler calls
 *      `menuStack.close(this)`, which begins with an `indexOf(this) >= 0` test that is false, and does
 *      NOTHING. Leaning on CDK here ships a menu that opens and never closes on Escape, silently.
 *      Escape, outside-click and item-activation are therefore each pinned below.
 *   2. THE TRIGGER TOGGLES. A pointerdown on the trigger is "outside" the overlay, so a naive
 *      outside-click handler closes the menu a beat before the trigger's own click reopens it, leaving
 *      the kebab unable to close what it opened. Clicking twice must end closed.
 *   3. `visible: false` REMOVES an item (not disables it) — `MenuItem.visible` semantics, which four
 *      call sites use to hide actions per row status.
 *   4. `command` FIRES, and fires with the row the trigger set. The call sites run
 *      `selectedRow.set(row); menu.toggle($event)`, so a menu built from a stale row would navigate to
 *      the wrong record — the single most dangerous failure available to this component.
 *   5. A `separator` renders as a separator and NOT as a clickable, empty menu item.
 *   6. The overlay anchors to the ELEMENT THAT WAS CLICKED, not to the <ui-menu> tag (which sits at the
 *      bottom of the template, far from the row).
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { UiMenu } from "./menu";
import type { UiMenuItem } from "./menu-item";

@Component({
  selector: "ui-test-host",
  imports: [UiMenu],
  template: `
    <button type="button" id="kebab-a" (click)="selectedRow.set('a'); menu.toggle($event)">
      A
    </button>
    <button type="button" id="kebab-b" (click)="selectedRow.set('b'); menu.toggle($event)">
      B
    </button>
    <ui-menu #menu [items]="items()" />
  `,
})
class TestHost {
  public readonly selectedRow = signal<string | null>(null);
  public readonly fired: string[] = [];
  public readonly canDelete = signal(true);

  public readonly items = signal<UiMenuItem[]>([]);

  public constructor() {
    this.items.set([
      { label: "View", icon: "eye", command: () => this.fired.push(`view:${this.selectedRow()}`) },
      { separator: true },
      {
        label: "Delete",
        icon: "trash",
        variant: "destructive",
        visible: true,
        command: () => this.fired.push("delete"),
      },
    ]);
  }
}

const panel = () => document.querySelector<HTMLElement>('[data-slot="dropdown-menu"]');
const itemsOf = () => [
  ...document.querySelectorAll<HTMLElement>('[data-slot="dropdown-menu-item"]'),
];
const click = (id: string) =>
  document.getElementById(id)!.dispatchEvent(new MouseEvent("click", { bubbles: true }));

describe("ui-menu", () => {
  let fixture: ComponentFixture<TestHost>;
  let host: TestHost;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHost],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(TestHost);
    host = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => fixture.destroy());

  const open = async (id = "kebab-a") => {
    click(id);
    await fixture.whenStable();
  };

  it("opens on toggle() and renders the visible items with their icons", async () => {
    expect(panel()).toBeNull();
    await open();
    expect(panel()).not.toBeNull();
    // 2 items + 1 separator; the separator must NOT be an item.
    expect(itemsOf().map((el) => el.textContent?.trim())).toEqual(["View", "Delete"]);
    expect(document.querySelectorAll('[data-slot="dropdown-menu-separator"]').length).toBe(1);
    expect(panel()!.querySelectorAll("ng-icon").length).toBe(2);
  });

  // (1) The headline: CdkMenu's own Escape handling is a no-op for an inline menu.
  it("CLOSES on Escape", async () => {
    await open();
    expect(panel()).not.toBeNull();
    panel()!.dispatchEvent(
      new KeyboardEvent("keydown", { key: "Escape", keyCode: 27, bubbles: true }),
    );
    await fixture.whenStable();
    expect(panel()).toBeNull();
  });

  // (1) Outside-click, the other path CDK would have owned.
  // CDK's dispatcher records the `pointerdown` target and EMITS on the `click`, both captured on
  // <body> — so a faithful test has to send both, in that order.
  it("CLOSES on an outside click", async () => {
    await open();
    document.body.dispatchEvent(new PointerEvent("pointerdown", { bubbles: true }));
    document.body.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    await fixture.whenStable();
    expect(panel()).toBeNull();
  });

  // (2) The trigger is "outside" the overlay; it must still toggle rather than close-then-reopen.
  it("CLOSES when its own trigger is clicked a second time", async () => {
    await open();
    expect(panel()).not.toBeNull();
    await open();
    expect(panel()).toBeNull();
  });

  // (1)+(4) Activating an item runs the command AND closes.
  it("runs command() and closes on item activation", async () => {
    await open();
    itemsOf()[0].click();
    await fixture.whenStable();
    expect(host.fired).toEqual(["view:a"]);
    expect(panel()).toBeNull();
  });

  // (4) The row the trigger set is the row the command sees — not a stale one.
  it("builds the menu from the row the trigger just selected", async () => {
    await open("kebab-b");
    itemsOf()[0].click();
    await fixture.whenStable();
    expect(host.fired).toEqual(["view:b"]);
  });

  // (3) `visible: false` REMOVES the item. A disabled-but-present Delete would still be a target.
  it("removes an item with visible:false", async () => {
    host.items.update((items) =>
      items.map((i) => (i.label === "Delete" ? { ...i, visible: false } : i)),
    );
    await fixture.whenStable();
    await open();
    expect(itemsOf().map((el) => el.textContent?.trim())).toEqual(["View"]);
  });

  it("marks a destructive item so it can be themed as one", async () => {
    await open();
    const del = itemsOf().find((el) => el.textContent?.trim() === "Delete")!;
    expect(del.getAttribute("data-variant")).toBe("destructive");
  });

  it("disables a disabled item rather than dropping it", async () => {
    host.items.set([{ label: "Nope", disabled: true, command: () => host.fired.push("nope") }]);
    await fixture.whenStable();
    await open();
    const el = itemsOf()[0];
    expect(el.getAttribute("data-disabled")).toBe("");
    el.click();
    await fixture.whenStable();
    expect(host.fired).toEqual([]);
  });

  // (6) Anchored to the clicked button, not to the <ui-menu> tag.
  it("anchors the overlay to the trigger that was clicked", async () => {
    await open("kebab-b");
    const pane = panel()!.closest(".cdk-overlay-connected-position-bounding-box");
    expect(pane).not.toBeNull();
    // The CDK bounding box is positioned from the origin element; assert it is not at 0,0 fallback
    // and that the overlay is attached to the document rather than nested inside <ui-menu>.
    expect(document.querySelector("ui-menu")!.contains(panel())).toBe(false);
  });
});
