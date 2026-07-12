/**
 * `[uiTooltip]`'s contract. Each of these fails silently — build, lint and every other spec stay green
 * while the tooltip is broken or inaccessible:
 *
 *   1. Opens on keyboard focus through the `<ui-button>` wrapper (`focus` does not bubble, so a
 *      listener on the wrapper never fires for the inner `<button>`).
 *   2. `aria-describedby` lands on the inner `<button>` — a description on a non-focusable node is
 *      never read out.
 *   3. The accessible name survives; a tooltip is a description, not a name.
 *   4. It closes — on mouseleave, blur, Escape and scroll.
 *   5. Empty / whitespace / undefined text renders nothing, not an empty box.
 *   6. The default position is `right`.
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
    // Only setTimeout is faked — faking more interferes with zoneless change detection.
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

  /** The `<ui-button>` wrapper element — not the focusable node. */
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

  // (1)
  it("opens on keyboard FOCUS of the inner button, through the <ui-button> wrapper", () => {
    open(innerButton(), "focusin");
    expect(tooltip()).not.toBeNull();
    expect(tooltip()?.textContent).toContain("Delete load");
    innerButton().dispatchEvent(new Event("focusout", { bubbles: true }));
    fixture.detectChanges();
  });

  // (2)
  it("puts aria-describedby on the inner <button>, never on the wrapper", () => {
    open(innerButton(), "focusin");

    const describedBy = innerButton().getAttribute("aria-describedby");
    expect(describedBy).toBeTruthy();
    expect(wrapper().hasAttribute("aria-describedby")).toBe(false);
    expect(document.getElementById(describedBy!)).toBe(tooltip());

    innerButton().dispatchEvent(new Event("focusout", { bubbles: true }));
    fixture.detectChanges();
  });

  // (3)
  it("leaves the button's accessible name alone", () => {
    open(innerButton(), "focusin");
    expect(innerButton().getAttribute("aria-label")).toBe("Delete load");
    innerButton().dispatchEvent(new Event("focusout", { bubbles: true }));
    fixture.detectChanges();
  });

  // (4)
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
   * (4) Scroll events do not bubble, and this app never scrolls the window — pages scroll inside
   * `div.overflow-y-auto` or the sidebar nav. A window-scroll listener therefore never fires here, and
   * the overlay's `noop()` scroll strategy will not reposition either, so the tooltip is left stranded
   * away from its host. Hence: dispatch from a non-window element, as a real scroll container does.
   */
  it("closes on a scroll inside a container, not only on a window scroll", () => {
    const scroller = document.createElement("div");
    document.body.appendChild(scroller);

    open(wrapper(), "focusin");
    expect(tooltip()).not.toBeNull();

    // `bubbles: false` is the point — a real scroll event does not bubble, which is why a
    // bubble-phase or window listener misses it.
    scroller.dispatchEvent(new Event("scroll", { bubbles: false }));
    fixture.detectChanges();

    expect(tooltip()).toBeNull();
    expect(innerButton().hasAttribute("aria-describedby")).toBe(false);

    scroller.remove();
  });

  // (5)
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

  // (6)
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

  // The retarget is only for component hosts that render their own control. A <td> that merely
  // contains a button is not one: the tooltip describes the cell.
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
