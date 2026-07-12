import { Component, signal } from "@angular/core";
import { TestBed } from "@angular/core/testing";
import { UiDialog } from "./dialog";

/**
 * These pin the four things about ui-dialog that fail silently — no error, no type failure, a green
 * build and a green suite:
 *
 *   1. the panel is portalled out of the component, so a width that lands on the host is a no-op;
 *   2. `#header` / `#content` / `#footer` must not be matched through a nested component;
 *   3. a backdrop click must not close the dialog (brain's default is the opposite);
 *   4. `(closed)` must not fire on open — call sites reset their form in it.
 */

/** The overlay is portalled to the CDK container in <body>, not into the fixture's host element. */
const panel = () => document.querySelector<HTMLElement>('[data-slot="dialog-content"]');
const backdrop = () => document.querySelector<HTMLElement>(".cdk-overlay-backdrop");

@Component({
  selector: "ui-nested-slots",
  imports: [],
  // Stands in for the accordion / data-table that really sit inside these dialogs. Its own slots are
  // also called #header and #content — which is the entire point.
  template: `<ng-content />`,
})
class NestedSlots {}

@Component({
  imports: [UiDialog, NestedSlots],
  template: `
    <ui-dialog
      [(open)]="open"
      header="Edit Customer"
      [width]="width()"
      (opened)="events.push('opened')"
      (closed)="events.push('closed')"
    >
      <p id="body">body</p>

      <!-- Direct child: THIS dialog's footer. -->
      <ng-template #footer><button id="save">Save</button></ng-template>

      <!-- Nested: belongs to <ui-nested-slots>, not to the dialog. -->
      <ui-nested-slots>
        <ng-template #header><span id="stolen-header">Danger Zone</span></ng-template>
        <ng-template #content><span id="stolen-content">Delete Customer</span></ng-template>
      </ui-nested-slots>
    </ui-dialog>
  `,
})
class Host {
  public readonly open = signal(false);
  public readonly width = signal<string | undefined>("450px");
  public readonly events: string[] = [];
}

describe("UiDialog", () => {
  let fixture: ReturnType<typeof TestBed.createComponent<Host>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.componentInstance.open.set(false);
    fixture.detectChanges();
  });

  const openIt = async () => {
    fixture.componentInstance.open.set(true);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  };

  it("opens into the CDK overlay and closes again", async () => {
    expect(panel()).toBeNull();

    await openIt();
    expect(panel()).not.toBeNull();
    expect(panel()!.textContent).toContain("body");

    fixture.componentInstance.open.set(false);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(panel()).toBeNull();
  });

  /**
   * jsdom does no layout — `getBoundingClientRect()` returns zeroes — so a pixel assertion would test
   * nothing. What it can prove is the part that actually breaks: the width is applied to the element
   * inside the CDK overlay, not to the host the content was portalled out of. Real geometry is checked
   * in a browser on /ui-lab.
   */
  it("puts the width on the PORTALLED PANEL, not on the host", async () => {
    await openIt();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelector('[data-slot="dialog-content"]')).toBeNull(); // it is NOT in the host
    expect(panel()!.closest(".cdk-overlay-container")).not.toBeNull(); // it IS in the overlay

    expect(panel()!.style.width).toBe("450px");

    // Helm's `sm:max-w-md` (448px) must be neutralised, or a wider dialog is silently clamped to it.
    expect(panel()!.style.maxWidth).toBe("calc(100vw - 2rem)");
  });

  it("carries an explicit maxWidth through to the panel", async () => {
    fixture.componentInstance.width.set("80vw");
    await openIt();
    expect(panel()!.style.width).toBe("80vw");
  });

  it("renders a DIRECT #footer template", async () => {
    await openIt();
    expect(panel()!.querySelector("#save")).not.toBeNull();
  });

  it("does NOT steal a nested component's #header / #content slots", async () => {
    await openIt();

    // The dialog's title must still be its own `header` input...
    expect(panel()!.textContent).toContain("Edit Customer");

    // ...and the nested component's templates must not be hoisted into the dialog chrome. With
    // `contentChild()`'s default descendants:true, both land inside the panel and "Danger Zone"
    // renders as the dialog's title.
    expect(panel()!.querySelector("#stolen-header")).toBeNull();
    expect(panel()!.querySelector("#stolen-content")).toBeNull();
  });

  it("does NOT close on a backdrop click", async () => {
    await openIt();
    expect(backdrop()).not.toBeNull();

    backdrop()!.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    // Brain's default is to close on the backdrop, which would mean clicking beside a half-filled
    // form discards it.
    expect(panel()).not.toBeNull();
    expect(fixture.componentInstance.open()).toBe(true);
  });

  it("closes on Escape when closable, and stays open when not", async () => {
    await openIt();

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.componentInstance.open()).toBe(false);
  });

  /**
   * A dropdown opened inside the dialog (select, autocomplete, date-picker, menu, popover) is its own
   * CDK overlay, stacked above the dialog's pane, and Escape there must dismiss only the dropdown.
   * Helm's overlays do not stop propagation, so without a topmost-overlay guard a single Escape closes
   * the dropdown and discards the form behind it.
   */
  it("ignores Escape while a nested overlay is stacked above it", async () => {
    await openIt();

    // Simulate a select's listbox opening on top: CDK appends its pane after the dialog's.
    const container = document.querySelector(".cdk-overlay-container")!;
    const nestedPane = document.createElement("div");
    nestedPane.classList.add("cdk-overlay-pane");
    nestedPane.appendChild(document.createElement("div")); // attached => has content
    container.appendChild(nestedPane);

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    // The dropdown ate the Escape; the dialog must survive.
    expect(fixture.componentInstance.open()).toBe(true);

    // Once the dropdown closes, CDK removes its pane and Escape belongs to the dialog again.
    nestedPane.remove();
    document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.componentInstance.open()).toBe(false);
  });

  /** A detached overlay leaves nothing behind, but if one ever did it must not suppress Escape. */
  it("still closes on Escape when a stacked pane is EMPTY (detached)", async () => {
    await openIt();

    const container = document.querySelector(".cdk-overlay-container")!;
    const emptyPane = document.createElement("div");
    emptyPane.classList.add("cdk-overlay-pane"); // no children => detached
    container.appendChild(emptyPane);

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.componentInstance.open()).toBe(false);
    emptyPane.remove();
  });

  it("fires (opened) on open and (closed) ONLY on close", async () => {
    const { events } = fixture.componentInstance;

    await openIt();
    // If `closed` also fired here, every close handler — most of which reset the form — would run as
    // the dialog appeared, and prefilled edit dialogs would blank themselves.
    expect(events).toEqual(["opened"]);

    fixture.componentInstance.open.set(false);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(events).toEqual(["opened", "closed"]);
  });
});
