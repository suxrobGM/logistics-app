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
  // <body> - so a faithful test has to send both, in that order.
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

  // (4) The row the trigger set is the row the command sees - not a stale one.
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

@Component({
  selector: "ui-test-header-host",
  imports: [UiMenu],
  template: `
    <button type="button" id="kebab" (click)="menu.toggle($event)">open</button>
    <ui-menu #menu [items]="items">
      <div menuHeader role="presentation" id="header">{{ name() }}</div>
    </ui-menu>
  `,
})
class HeaderHost {
  public readonly name = signal("Ada Lovelace");
  public readonly items: UiMenuItem[] = [{ label: "View", command: () => undefined }];
}

// (7)
describe("ui-menu with a projected header", () => {
  let fixture: ComponentFixture<HeaderHost>;
  let host: HeaderHost;

  const header = () => document.getElementById("header");

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HeaderHost],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(HeaderHost);
    host = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => fixture.destroy());

  const toggle = async () => {
    click("kebab");
    await fixture.whenStable();
  };

  it("renders the header inside the panel, above the items", async () => {
    await toggle();
    expect(header()).not.toBeNull();
    expect(panel()!.contains(header())).toBe(true);
    expect(header()!.compareDocumentPosition(itemsOf()[0]) & Node.DOCUMENT_POSITION_FOLLOWING).toBe(
      Node.DOCUMENT_POSITION_FOLLOWING,
    );
  });

  // The header is a plain div, so CdkMenu's key manager must not treat it as a focus target.
  it("keeps the header out of the menu items", async () => {
    await toggle();
    expect(itemsOf().length).toBe(1);
    expect(header()!.matches('[data-slot="dropdown-menu-item"]')).toBe(false);
  });

  it("still renders the header on a REOPEN, and re-renders on a change while closed", async () => {
    await toggle();
    expect(header()!.textContent!.trim()).toBe("Ada Lovelace");

    await toggle();
    expect(panel()).toBeNull();

    host.name.set("Grace Hopper");
    await fixture.whenStable();

    await toggle();
    expect(header()).not.toBeNull();
    expect(header()!.textContent!.trim()).toBe("Grace Hopper");
  });
});
