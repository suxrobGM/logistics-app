/**
 * `[uiTooltip]`'s contract. Everything pinned here is something that (a) the 124 call sites depend on
 * and (b) NO other gate would catch — each one stays green through `build:all`, `lint` and every
 * other spec while shipping a broken or inaccessible tooltip:
 *
 *   1. OPENS ON KEYBOARD FOCUS THROUGH THE `<ui-button>` WRAPPER. 82 of the 124 hosts are
 *      `<ui-button>`, which renders a real `<button>` INSIDE itself. `focus` does not bubble, so the
 *      obvious implementation (brain's, which listens for `focus` on its own host) never fires here.
 *      Nothing throws. The tooltip simply never appears for anyone using a keyboard.
 *   2. `aria-describedby` LANDS ON THE INNER `<button>`, not on the `<ui-button>` wrapper — a
 *      description on a non-focusable node is a description no screen reader ever reads.
 *   3. The accessible NAME survives. A tooltip is a description; if it ever became the name, the 82
 *      icon-only buttons would regress to being named by their tooltip text (or by nothing).
 *   4. It CLOSES — on mouseleave, on blur, and on Escape. A tooltip stuck open is worse than none,
 *      and PrimeNG closed on Escape (`hideOnEscape: true`), so dropping it is a silent regression.
 *   5. Empty / whitespace / undefined text renders NOTHING — not an empty box.
 *   6. The default position is `right`, which is PrimeNG's default and therefore what the 79 call
 *      sites that specify no position are laid out against.
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { UiButton } from "../../action/button/button";
import { UiTooltip } from "./tooltip";

/** The directive's default show delay (brain's `defaultOptions.showDelay`). */
const SHOW_DELAY = 150;

@Component({
  selector: "ui-test-host",
  imports: [UiButton, UiTooltip],
  template: `
    <ui-button
      icon="trash"
      ariaLabel="Delete load"
      [uiTooltip]="text()"
      [uiTooltipPosition]="position()"
    />
    <button type="button" uiTooltip="Direct host">plain</button>
    <td uiTooltip="Describes the cell"><button type="button" id="in-cell">nested</button></td>
  `,
})
class TestHost {
  public readonly text = signal<string | undefined>("Delete load");
  public readonly position = signal<"top" | "bottom" | "left" | "right">("right");
}

describe("UiTooltip", () => {
  let fixture: ComponentFixture<TestHost>;
  let host: TestHost;

  beforeEach(() => {
    // Only setTimeout is faked: the directive's show delay is the one thing we need to control, and
    // faking more would interfere with zoneless change detection.
    vi.useFakeTimers({ toFake: ["setTimeout", "clearTimeout"] });
    TestBed.configureTestingModule({
      imports: [TestHost],
      providers: [provideZonelessChangeDetection()],
    });
    fixture = TestBed.createComponent(TestHost);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  /** The `<ui-button>` wrapper element — NOT the focusable node. */
  const wrapper = (): HTMLElement => fixture.nativeElement.querySelector("ui-button");
  /** The real `<button>` the browser actually focuses, inside the wrapper. */
  const innerButton = (): HTMLElement => wrapper().querySelector("button")!;
  const tooltip = (): HTMLElement | null => document.body.querySelector('[role="tooltip"]');

  const open = (el: HTMLElement, event: "mouseenter" | "focusin") => {
    el.dispatchEvent(new Event(event, { bubbles: event === "focusin" }));
    vi.advanceTimersByTime(SHOW_DELAY);
    fixture.detectChanges();
  };

  it("opens on hover and shows the text", () => {
    open(wrapper(), "mouseenter");
    expect(tooltip()?.textContent).toContain("Delete load");
    wrapper().dispatchEvent(new Event("mouseleave"));
    fixture.detectChanges();
  });

  // (1) The whole reason this directive exists rather than wrapping BrnTooltip.
  it("opens on keyboard FOCUS of the inner button, through the <ui-button> wrapper", () => {
    open(innerButton(), "focusin");
    expect(tooltip()).not.toBeNull();
    expect(tooltip()?.textContent).toContain("Delete load");
    innerButton().dispatchEvent(new Event("focusout", { bubbles: true }));
    fixture.detectChanges();
  });

  // (2) A description on the wrapper is a description nobody hears.
  it("puts aria-describedby on the inner <button>, never on the wrapper", () => {
    open(innerButton(), "focusin");

    const describedBy = innerButton().getAttribute("aria-describedby");
    expect(describedBy).toBeTruthy();
    expect(wrapper().hasAttribute("aria-describedby")).toBe(false);
    expect(document.getElementById(describedBy!)).toBe(tooltip());

    innerButton().dispatchEvent(new Event("focusout", { bubbles: true }));
    fixture.detectChanges();
  });

  // (3) A tooltip is a DESCRIPTION, never a NAME.
  it("leaves the button's accessible name alone", () => {
    open(innerButton(), "focusin");
    expect(innerButton().getAttribute("aria-label")).toBe("Delete load");
    innerButton().dispatchEvent(new Event("focusout", { bubbles: true }));
    fixture.detectChanges();
  });

  // (4) Closing. Three ways in, three ways out.
  it("closes on mouseleave, and drops the aria-describedby with it", () => {
    open(wrapper(), "mouseenter");
    expect(tooltip()).not.toBeNull();

    wrapper().dispatchEvent(new Event("mouseleave"));
    fixture.detectChanges();

    expect(tooltip()).toBeNull();
    expect(innerButton().hasAttribute("aria-describedby")).toBe(false);
  });

  it("closes on blur (focusout)", () => {
    open(innerButton(), "focusin");
    expect(tooltip()).not.toBeNull();

    innerButton().dispatchEvent(new Event("focusout", { bubbles: true }));
    fixture.detectChanges();

    expect(tooltip()).toBeNull();
  });

  it("closes on Escape even when opened by hover", () => {
    open(wrapper(), "mouseenter");
    expect(tooltip()).not.toBeNull();

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape" }));
    fixture.detectChanges();

    expect(tooltip()).toBeNull();
    expect(innerButton().hasAttribute("aria-describedby")).toBe(false);
  });

  /**
   * (4) CLOSES ON A SCROLL IN ANY CONTAINER — not just on a window scroll.
   *
   * SCROLL EVENTS DO NOT BUBBLE, and this app NEVER SCROLLS THE WINDOW: every page scrolls inside
   * `div.overflow-y-auto` (the main content pane) or the sidebar nav. So a `window`-scroll listener
   * — the obvious implementation, and the one that shipped — never fires in the product at all, and
   * the overlay uses a `noop()` scroll strategy that will not reposition either. The tooltip is left
   * STRANDED hundreds of pixels from its host, pointing at nothing.
   *
   * Verified in a browser before this test was written: focus a tooltip'd icon button on /ui-lab,
   * scroll the content pane 400px, and the host moves -400px while the tooltip moves +56px.
   *
   * The hover path hid this by accident — scrolling drags the host out from under the cursor, so
   * `mouseleave` fires and closes it. Only KEYBOARD users, whom this directive exists to serve, ever
   * saw the stranded tooltip. Hence: dispatch from a non-window element, exactly as a real scroll
   * container does.
   */
  it("closes on a scroll inside a container, not only on a window scroll", () => {
    const scroller = document.createElement("div");
    document.body.appendChild(scroller);

    open(wrapper(), "focusin");
    expect(tooltip()).not.toBeNull();

    // A scroll on an inner element. `bubbles: false` is the whole point — this is what a real
    // scroll event is, and it is why a bubble-phase/window listener misses it.
    scroller.dispatchEvent(new Event("scroll", { bubbles: false }));
    fixture.detectChanges();

    expect(tooltip()).toBeNull();
    expect(innerButton().hasAttribute("aria-describedby")).toBe(false);

    scroller.remove();
  });

  // (5) An empty value is not an empty tooltip — it is no tooltip.
  it("renders nothing for empty, whitespace or undefined text", () => {
    for (const value of ["", "   ", undefined]) {
      host.text.set(value);
      fixture.detectChanges();

      open(wrapper(), "mouseenter");
      expect(tooltip()).toBeNull();
      expect(innerButton().hasAttribute("aria-describedby")).toBe(false);

      wrapper().dispatchEvent(new Event("mouseleave"));
      fixture.detectChanges();
    }
  });

  it("closes an open tooltip if its text goes empty underneath it", () => {
    open(wrapper(), "mouseenter");
    expect(tooltip()).not.toBeNull();

    host.text.set("");
    fixture.detectChanges();

    expect(tooltip()).toBeNull();
  });

  // (6) PrimeNG's default. 79 sites set no position and are laid out against it.
  it("defaults to the right side and honours an explicit side", () => {
    open(wrapper(), "mouseenter");
    expect(tooltip()?.getAttribute("data-side")).toBe("right");
    wrapper().dispatchEvent(new Event("mouseleave"));
    fixture.detectChanges();

    host.position.set("bottom");
    fixture.detectChanges();

    open(wrapper(), "mouseenter");
    expect(tooltip()?.getAttribute("data-side")).toBe("bottom");
    wrapper().dispatchEvent(new Event("mouseleave"));
    fixture.detectChanges();
  });

  it("works when the host IS the focusable element", () => {
    const plain: HTMLElement = fixture.nativeElement.querySelector("button[uiTooltip]");

    open(plain, "focusin");

    expect(tooltip()?.textContent).toContain("Direct host");
    expect(plain.getAttribute("aria-describedby")).toBe(tooltip()?.id);

    plain.dispatchEvent(new Event("focusout", { bubbles: true }));
    fixture.detectChanges();
    expect(tooltip()).toBeNull();
  });

  // The retarget is for COMPONENT hosts that render their own control. A <td> that merely contains a
  // button is not that: the tooltip describes the cell, and hoisting it onto the button would make a
  // screen reader announce the cell's hint as the button's.
  it("does NOT hoist the description onto a button that merely lives inside a native host", () => {
    const cell: HTMLElement = fixture.nativeElement.querySelector("td[uiTooltip]");
    const nested: HTMLElement = fixture.nativeElement.querySelector("#in-cell");

    open(cell, "mouseenter");

    expect(cell.getAttribute("aria-describedby")).toBeTruthy();
    expect(nested.hasAttribute("aria-describedby")).toBe(false);

    cell.dispatchEvent(new Event("mouseleave"));
    fixture.detectChanges();
  });

  it("does not leave the tooltip in the DOM when the host is destroyed", () => {
    open(wrapper(), "mouseenter");
    expect(tooltip()).not.toBeNull();

    fixture.destroy();

    expect(tooltip()).toBeNull();
  });
});
