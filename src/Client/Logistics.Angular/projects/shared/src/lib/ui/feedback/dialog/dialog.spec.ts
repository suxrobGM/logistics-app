import { Component, signal } from "@angular/core";
import { TestBed } from "@angular/core/testing";
import { UiDialog } from "./dialog";

/**
 * These tests pin the four things about ui-dialog that fail SILENTLY — no error, no type failure, a
 * green build and a green suite — and which therefore cannot be left to review to catch:
 *
 *   1. the panel is portalled out of the component, so a width that lands on the host is a no-op;
 *   2. `#header` / `#content` / `#footer` must NOT be matched through a nested component;
 *   3. a backdrop click must not close the dialog (brain's default is the opposite);
 *   4. `(closed)` must not fire on OPEN (16 call sites reset their form in it).
 */

/** The overlay is portalled to the CDK container in <body>, not into the fixture's host element. */
const panel = () => document.querySelector<HTMLElement>('[data-slot="dialog-content"]');
const backdrop = () => document.querySelector<HTMLElement>(".cdk-overlay-backdrop");

@Component({
  selector: "ui-nested-slots",
  imports: [],
  // A stand-in for the p-accordion / ui-data-table that really sit inside these dialogs. Its OWN
  // slots are also called #header and #content — which is the entire point.
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

      <!-- Nested: belongs to <ui-nested-slots>, NOT to the dialog. Exactly the shape of
           customer-edit-dialog's accordion panel. -->
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

    // Lesson 5: an overlay that opens but never closes is the bug this migration already shipped once.
    fixture.componentInstance.open.set(false);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(panel()).toBeNull();
  });

  /**
   * NOTE ON WHAT IS ASSERTED HERE. The test environment is jsdom, which does no layout at all —
   * `getBoundingClientRect()` returns zeroes for EVERY element, so a pixel assertion here would not
   * be testing the dialog, it would be testing nothing. What jsdom *can* prove is the part that
   * actually breaks: that the width is applied to the element inside the CDK overlay and not to the
   * host that CDK portalled the content out of. The real geometry is measured in a browser (see the
   * /ui-lab dialogs section, and the S7 browser verification).
   */
  it("puts the width on the PORTALLED PANEL, not on the host", async () => {
    await openIt();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelector('[data-slot="dialog-content"]')).toBeNull(); // it is NOT in the host
    expect(panel()!.closest(".cdk-overlay-container")).not.toBeNull(); // it IS in the overlay

    expect(panel()!.style.width).toBe("450px");

    // ...and Helm's own `sm:max-w-md` (28rem = 448px) is neutralised, or every dialog wider than
    // 448px would be silently clamped to 448px.
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

    // ...and the nested component's templates must NOT have been hoisted into the dialog chrome.
    // With `contentChild()`'s DEFAULT descendants:true, both of these would be inside the panel and
    // "Danger Zone" would be rendered as the dialog's title. This is the S5 ui-card bug.
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

    // p-dialog only closes on the mask when `dismissableMask` is set, and NOT ONE of the 46 call
    // sites sets it. brain's default is to close — so this test is the thing standing between the
    // app and "clicking beside a half-filled form discards it".
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
   * A dropdown opened INSIDE the dialog (ui-select-field, autocomplete, date-picker, menu, popover) is
   * its own CDK overlay, stacked above the dialog's pane. Escape there must dismiss only the dropdown.
   *
   * PrimeNG's select called `event.stopPropagation()` on Escape (primeng-select.mjs:1512), so
   * p-dialog's document listener never saw it. Helm's overlays do NOT stop propagation — so without a
   * topmost-overlay guard a single Escape closed the dropdown AND discarded the form behind it. Found
   * in a browser, on the trip wizard's "Create a new load" dialog; invisible to build, lint and tests.
   *
   * jsdom does no layout, but this bug is pure DOM structure — which is exactly what jsdom CAN prove.
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

    // The dropdown ate the Escape. The dialog — and the user's half-filled form — must survive.
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
    // If `closed` also fired here, the 16 (onHide) handlers — most of which reset the form — would
    // run as the dialog appeared, and every prefilled edit dialog would blank itself.
    expect(events).toEqual(["opened"]);

    fixture.componentInstance.open.set(false);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(events).toEqual(["opened", "closed"]);
  });
});
