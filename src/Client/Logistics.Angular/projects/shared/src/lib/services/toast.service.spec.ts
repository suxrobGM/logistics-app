/**
 * CONTRACT spec for `ToastService`, whose internals sit on `@spartan-ng/brain/sonner`'s imperative
 * `toast` and a Helm-based `ui-confirm-dialog`.
 *
 * ~386 call sites across ~118 files must keep working with ZERO changes, so this spec asserts on
 * WHAT THE USER SEES AND WHAT GETS CALLED — never on a rendering library's API having been invoked.
 * The failure this guards against is silent and asymmetric:
 *   - dialog never opens AND `accept` is dropped  => "Delete" silently does nothing;
 *   - `accept` fires with no dialog               => it deletes WITHOUT ASKING.
 *
 * ======================== WHY THIS SPEC SURVIVES A RENDERING SWAP ========================
 * Everything that knows which library renders these surfaces is quarantined in the block marked
 * `RENDERING-LAYER ADAPTER`: the host component, the DI providers, and four tiny readers that
 * translate "what the rendering layer did" back into our own vocabulary. A rendering-layer swap
 * should only ever need to touch that block — every `describe`/`it` body and every `expect(...)`
 * below asserts on OUR vocabulary, never the library's, which is the entire point.
 *
 * Keep it that way. If a future rendering swap seems to require changing an assertion, that is a
 * BEHAVIOUR CHANGE, not a test-maintenance chore: fix the code, or land the behaviour change
 * deliberately and say so out loud.
 * ===================================================================================
 *
 * NOT OBSERVABLE HERE (documented deliberately rather than asserted vacuously):
 *   - toast auto-dismiss after its duration — a real timer; out of scope for a contract spec.
 *   - toast stacking/position — pure CSS, invisible to jsdom.
 */
import { Dialog } from "@angular/cdk/dialog";
import {
  Component,
  DestroyRef,
  ElementRef,
  inject,
  provideZonelessChangeDetection,
} from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { toastState } from "@spartan-ng/brain/sonner";
import { UiToaster } from "../ui/overlay/toaster/toaster";
import { ToastService, type ConfirmOptions } from "./toast.service";

/** Our own vocabulary — independent of any UI library. */
type Severity = "success" | "error" | "warning" | "info";
type Intent = "default" | "danger" | "warning" | "success";

// ═════════════════════ RENDERING-LAYER ADAPTER (brain/sonner + Helm dialog) ═════════════════════
// ▼▼▼ EVERYTHING BETWEEN THESE MARKERS IS THE ONLY PART A RENDERING SWAP CHANGES ▼▼▼

/**
 * Mirrors what every app's `app.html` renders — which is now just the toaster. The confirm dialog is
 * no longer a declarative surface: `ToastService` opens `ui-confirm-dialog` imperatively through
 * `HlmDialogService`, so there is nothing for a shell to render and no `key` to get wrong.
 *
 * The teardown below is NOT belt-and-braces; without it the suite is wrong in both directions:
 *   - `toastState` (brain/sonner) is a MODULE-GLOBAL signal store, not a DI singleton. It survives
 *     `TestBed.resetTestingModule()`, so a toast raised in one test is still in the store when the
 *     NEXT test mounts its toaster — and "renders nothing before anything is shown" would fail.
 *   - the dialog is a CDK overlay portaled to <body>, OUTSIDE the fixture, so `fixture.destroy()`
 *     does not take it down — and `afterEach`'s `expect(confirmSurface()).toBeNull()` would fail.
 * The app needs neither (its shell outlives every toast, and a destroyed shell means the page is
 * gone); a test that reuses one document does. `closeAll()` closes with `undefined`, which by design
 * fires neither `accept` nor `reject` — so teardown can never forge a decision.
 */
@Component({
  selector: "ui-host-notification-surfaces",
  imports: [UiToaster],
  template: `<ui-toaster />`,
})
class HostNotificationSurfaces {
  constructor() {
    const dialog = inject(Dialog);
    const host = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;

    inject(DestroyRef).onDestroy(() => {
      dialog.closeAll();
      toastState.reset();

      // `fixture.destroy()` tears down the VIEW but leaves the root element's DOM in the document —
      // TestBed only strips root elements at `resetTestingModule()`, i.e. in the NEXT test's
      // `beforeEach`. Sonner renders in place and (rightly) leaves lifetime to the framework, so the
      // surface detaches itself here, or `afterEach` would find the previous test's toast still
      // hanging off <body>.
      //
      // This cannot mask a real failure: it only removes THIS host's subtree. A toast that failed to
      // render still fails `theToast()`, and the leaks that actually matter — the module-global
      // `toastState` and the body-portaled CDK overlay — are undone above, by reset() and closeAll().
      host.remove();
    });
  }
}

/** brain/sonner's `toast` is imperative and `HlmDialogService` is `providedIn: 'root'` — nothing to provide. */
const RENDERING_PROVIDERS: never[] = [];

/**
 * The accept/reject button labels the rendering layer falls back to when the caller supplies none.
 * `confirmDelete` — i.e. most of the ~386 call sites — relies on these: they are
 * `ToastService`'s own DEFAULT_ACCEPT_LABEL / DEFAULT_REJECT_LABEL.
 */
const DEFAULT_ACCEPT_LABEL = "Yes";
const DEFAULT_REJECT_LABEL = "No";

/** Severity the rendering layer stamped onto a toast, normalized to OUR vocabulary. */
function renderedSeverity(toast: Element): Severity | null {
  const marker = toast.getAttribute("data-type") ?? ""; // sonner stamps its toast type here
  if (marker.includes("success")) return "success";
  if (marker.includes("error")) return "error";
  if (marker.includes("warn")) return "warning"; // sonner says "warning", we say "warning"
  if (marker.includes("info")) return "info";
  return null;
}

/** Intent (accept/reject button styling) the rendering layer applied, normalized to OUR vocabulary. */
function renderedIntent(button: Element): Intent {
  // `ui-button`'s intent cells resolve to semantic TOKEN utilities (`bg-danger`, `border-success`,
  // `hover:bg-warning/10`, …). Matching the `-token` stem rather than a whole class name keeps this
  // reader indifferent to which appearance (solid/outlined) the dialog chose for each button.
  const classes = button.className;
  if (classes.includes("-danger")) return "danger";
  if (classes.includes("-warning")) return "warning"; // the `warn` INTENT paints with the `warning` TOKEN
  if (classes.includes("-success")) return "success";
  return "default";
}

/** Did an icon actually get rendered inside this element (excluding icons in nested buttons)? */
function rendersIcon(host: Element): boolean {
  // `<ui-icon>` renders an `<ng-icon>`; nothing here emits an icon font's `<i>` tag any more.
  const icons = [...host.querySelectorAll("ui-icon, ng-icon")];
  return icons.some((icon) => icon.closest("button") === host.closest("button"));
}

/** The "click outside the dialog" gesture. Returns false if there is no backdrop to click. */
function clickBackdrop(): boolean {
  // CDK's backdrop is a SIBLING of the overlay pane, not an ancestor of the dialog, and it listens
  // for `click` (not `mousedown`). It exists whenever `hasBackdrop` — i.e. always here — regardless
  // of whether the dialog will actually act on it, which is what the negative control relies on.
  const backdrop = document.querySelector<HTMLElement>(".cdk-overlay-backdrop");
  if (!backdrop) return false;
  backdrop.dispatchEvent(new MouseEvent("click", { bubbles: true }));
  return true;
}

// ▲▲▲ END OF THE ONLY PART A RENDERING SWAP CHANGES ▲▲▲
// ═══════════════════════════════════════════════════════════════════════════════════════════════

// ───────────────────────── Library-agnostic queries (must NOT change on a rendering swap) ───────
// Everything is rooted at `document.body` so it works whether the surface renders in place or is
// portaled into an overlay container.

/** Toast surfaces: any element a screen reader would announce as a live notification. */
function toasts(): HTMLElement[] {
  return [...document.body.querySelectorAll<HTMLElement>('[role="alert"], [role="status"]')];
}

/** The single visible toast. Fails loudly if there is not exactly one. */
function theToast(): HTMLElement {
  const visible = toasts().filter(isVisible);
  expect(visible).toHaveLength(1);
  return visible[0];
}

/**
 * The confirmation surface: an element carrying a dialog role that the user can actually see and
 * read. The visible-and-non-empty filter is NOT cosmetic — a dialog root can linger in the DOM
 * (hidden, or left behind by a test that didn't tear down cleanly) carrying a static
 * `role="alertdialog"`, and matching on role alone would make every "the dialog opened" assertion
 * below pass vacuously.
 */
function confirmSurface(): HTMLElement | null {
  const candidates = [
    ...document.body.querySelectorAll<HTMLElement>('[role="dialog"], [role="alertdialog"]'),
  ];
  return candidates.find((el) => isVisible(el) && text(el) !== "") ?? null;
}

/** The confirmation surface, asserted open. */
function theDialog(): HTMLElement {
  const dialog = confirmSurface();
  expect(dialog, "expected a confirmation dialog to be open and visible").not.toBeNull();
  return dialog!;
}

function text(element: Element): string {
  return (element.textContent ?? "").replace(/\s+/g, " ").trim();
}

/** An element's accessible name: `aria-label` wins, otherwise its text content. */
function accessibleName(element: Element): string {
  return (element.getAttribute("aria-label") ?? element.textContent ?? "")
    .replace(/\s+/g, " ")
    .trim();
}

/**
 * Find a button BY ACCESSIBLE NAME — the way a user (or a screen reader, or Testing Library) finds
 * it. Deliberately not a CSS class selector: a class query is implementation coupling and would
 * break the moment the rendering layer changes, for the wrong reason.
 */
function buttonNamed(root: Element, name: string): HTMLElement {
  const buttons = [...root.querySelectorAll<HTMLElement>("button, [role='button']")];
  const match = buttons.find((button) => accessibleName(button) === name);
  if (!match) {
    const found = buttons.map((button) => JSON.stringify(accessibleName(button))).join(", ");
    throw new Error(`No button named "${name}". Buttons present: ${found || "(none)"}`);
  }
  return match;
}

function isVisible(element: Element): boolean {
  let node: Element | null = element;
  while (node && node !== document.body) {
    const style = getComputedStyle(node);
    if (style.display === "none" || style.visibility === "hidden") return false;
    node = node.parentElement;
  }
  return true;
}

// ─────────────────────────────────────── Fixture plumbing ──────────────────────────────────────

let fixture: ComponentFixture<HostNotificationSurfaces>;
let toast: ToastService;

async function settle(): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

/** Open a confirmation and let it render. */
async function openConfirm(options: ConfirmOptions): Promise<void> {
  toast.confirm(options);
  await settle();
}

function pressEscape(): void {
  document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
}

describe("ToastService — the contract every call site depends on", () => {
  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), ...RENDERING_PROVIDERS],
    });
    fixture = TestBed.createComponent(HostNotificationSurfaces);
    toast = TestBed.inject(ToastService);
    await settle();
  });

  // Surfaces may be portaled out of the fixture root (the dialog portals into <body> via the CDK
  // overlay container), so tear the host down explicitly — a dialog left open by one test must
  // not leak into the next.
  afterEach(() => {
    fixture.destroy();
    expect(confirmSurface()).toBeNull();
    expect(toasts().filter(isVisible)).toHaveLength(0);
  });

  describe("toasts become visible, carrying their title, message and severity", () => {
    it("renders nothing before anything is shown", () => {
      expect(toasts().filter(isVisible)).toHaveLength(0);
    });

    it("showSuccess: default title 'Notification', at success severity", async () => {
      toast.showSuccess("Load created.");
      await settle();

      expect(text(theToast())).toContain("Notification");
      expect(text(theToast())).toContain("Load created.");
      expect(renderedSeverity(theToast())).toBe("success");
    });

    it("showSuccess: honours a custom title", async () => {
      toast.showSuccess("Load created.", "Saved");
      await settle();

      expect(text(theToast())).toContain("Saved");
      expect(text(theToast())).toContain("Load created.");
      expect(renderedSeverity(theToast())).toBe("success");
    });

    it("showError: title 'Error', at error severity", async () => {
      toast.showError("Could not reach the server.");
      await settle();

      expect(text(theToast())).toContain("Error");
      expect(text(theToast())).toContain("Could not reach the server.");
      expect(renderedSeverity(theToast())).toBe("error");
    });

    it("showWarning: default title 'Warning', at warning severity", async () => {
      toast.showWarning("Driver is over hours.");
      await settle();

      expect(text(theToast())).toContain("Warning");
      expect(text(theToast())).toContain("Driver is over hours.");
      expect(renderedSeverity(theToast())).toBe("warning");
    });

    it("showWarning: honours a custom title", async () => {
      toast.showWarning("Driver is over hours.", "Hours of Service");
      await settle();

      expect(text(theToast())).toContain("Hours of Service");
      expect(renderedSeverity(theToast())).toBe("warning");
    });

    it("showInfo: default title 'Information', at info severity", async () => {
      toast.showInfo("Sync finished.");
      await settle();

      expect(text(theToast())).toContain("Information");
      expect(text(theToast())).toContain("Sync finished.");
      expect(renderedSeverity(theToast())).toBe("info");
    });

    it("showInfo: honours a custom title", async () => {
      toast.showInfo("Sync finished.", "Background Job");
      await settle();

      expect(text(theToast())).toContain("Background Job");
      expect(renderedSeverity(theToast())).toBe("info");
    });
  });

  describe("confirm() — the accept callback must never fire without the user accepting", () => {
    it("opens a surface with a dialog role, showing the header and the message", async () => {
      expect(confirmSurface()).toBeNull();

      await openConfirm({
        message: "Send this invoice to the customer?",
        header: "Send Invoice",
        accept: vi.fn(),
      });

      const dialog = theDialog();
      expect(["dialog", "alertdialog"]).toContain(dialog.getAttribute("role"));
      expect(text(dialog)).toContain("Send Invoice");
      expect(text(dialog)).toContain("Send this invoice to the customer?");
    });

    it("does NOT call accept merely because the dialog opened", async () => {
      const accept = vi.fn();
      const reject = vi.fn();

      await openConfirm({ message: "Delete it?", header: "Confirm", accept, reject });

      theDialog(); // the dialog is up, waiting on the user...
      expect(accept).not.toHaveBeenCalled();
      expect(reject).not.toHaveBeenCalled();
    });

    it("accepting calls accept EXACTLY ONCE, and never reject", async () => {
      const accept = vi.fn();
      const reject = vi.fn();

      await openConfirm({
        message: "Delete it?",
        header: "Confirm",
        acceptLabel: "Delete",
        rejectLabel: "Keep",
        accept,
        reject,
      });
      buttonNamed(theDialog(), "Delete").click();
      await settle();

      expect(accept).toHaveBeenCalledTimes(1);
      expect(reject).not.toHaveBeenCalled();
      expect(confirmSurface()).toBeNull(); // and it closed
    });

    it("rejecting calls reject and never accept", async () => {
      const accept = vi.fn();
      const reject = vi.fn();

      await openConfirm({
        message: "Delete it?",
        header: "Confirm",
        acceptLabel: "Delete",
        rejectLabel: "Keep",
        accept,
        reject,
      });
      buttonNamed(theDialog(), "Keep").click();
      await settle();

      expect(reject).toHaveBeenCalledTimes(1);
      expect(accept).not.toHaveBeenCalled();
      expect(confirmSurface()).toBeNull();
    });

    it("rejecting with no reject callback closes cleanly and still never accepts", async () => {
      const accept = vi.fn();

      await openConfirm({ message: "Delete it?", header: "Confirm", accept });
      buttonNamed(theDialog(), DEFAULT_REJECT_LABEL).click();
      await settle();

      expect(accept).not.toHaveBeenCalled();
      expect(confirmSurface()).toBeNull();
    });

    it("acceptLabel / rejectLabel become the buttons' accessible names", async () => {
      await openConfirm({
        message: "Cancel your subscription?",
        acceptLabel: "Yes, Cancel",
        rejectLabel: "No, Keep",
        accept: vi.fn(),
      });

      const dialog = theDialog();
      expect(buttonNamed(dialog, "Yes, Cancel")).toBeTruthy();
      expect(buttonNamed(dialog, "No, Keep")).toBeTruthy();
    });

    it("falls back to the default button labels when none are supplied", async () => {
      await openConfirm({ message: "Proceed?", accept: vi.fn() });

      const dialog = theDialog();
      expect(buttonNamed(dialog, DEFAULT_ACCEPT_LABEL)).toBeTruthy();
      expect(buttonNamed(dialog, DEFAULT_REJECT_LABEL)).toBeTruthy();
    });

    it("severity / rejectSeverity reach the accept / reject buttons", async () => {
      await openConfirm({
        message: "Cancel your subscription?",
        acceptLabel: "Yes, Cancel",
        rejectLabel: "No, Keep",
        severity: "danger",
        rejectSeverity: "success",
        accept: vi.fn(),
      });

      const dialog = theDialog();
      expect(renderedIntent(buttonNamed(dialog, "Yes, Cancel"))).toBe("danger");
      expect(renderedIntent(buttonNamed(dialog, "No, Keep"))).toBe("success");
    });

    it("no severity => no intent styling (negative control for the assertion above)", async () => {
      await openConfirm({ message: "Proceed?", accept: vi.fn() });

      const dialog = theDialog();
      expect(renderedIntent(buttonNamed(dialog, DEFAULT_ACCEPT_LABEL))).toBe("default");
      expect(renderedIntent(buttonNamed(dialog, DEFAULT_REJECT_LABEL))).toBe("default");
    });

    it("icon reaches the dialog body — and is absent when not asked for", async () => {
      await openConfirm({ message: "Proceed?", icon: "warning", accept: vi.fn() });
      expect(rendersIcon(theDialog())).toBe(true);

      buttonNamed(theDialog(), DEFAULT_REJECT_LABEL).click();
      await settle();

      await openConfirm({ message: "Proceed?", accept: vi.fn() });
      expect(rendersIcon(theDialog())).toBe(false);
    });

    it("acceptIcon / rejectIcon reach their buttons — and are absent when not asked for", async () => {
      await openConfirm({
        message: "Cancel your subscription?",
        acceptLabel: "Yes, Cancel",
        rejectLabel: "No, Keep",
        acceptIcon: "check",
        rejectIcon: "close",
        accept: vi.fn(),
      });

      const dialog = theDialog();
      expect(rendersIcon(buttonNamed(dialog, "Yes, Cancel"))).toBe(true);
      expect(rendersIcon(buttonNamed(dialog, "No, Keep"))).toBe(true);

      buttonNamed(dialog, "No, Keep").click();
      await settle();

      await openConfirm({ message: "Proceed?", accept: vi.fn() });
      expect(rendersIcon(buttonNamed(theDialog(), DEFAULT_ACCEPT_LABEL))).toBe(false);
    });

    it("closeOnEscape: Escape rejects, never accepts, and closes the dialog", async () => {
      const accept = vi.fn();
      const reject = vi.fn();

      await openConfirm({
        message: "Cancel your subscription?",
        closeOnEscape: true,
        accept,
        reject,
      });
      theDialog();
      pressEscape();
      await settle();

      expect(accept).not.toHaveBeenCalled();
      expect(reject).toHaveBeenCalledTimes(1);
      expect(confirmSurface()).toBeNull();
    });

    /**
     * REGRESSION GUARD — this test used to assert the OPPOSITE, as a "characterization of today's
     * behaviour", and today's behaviour was a bug.
     *
     * `ToastService.confirm` used to forward `closeOnEscape ?? false` on the belief that an
     * unspecified value coerced to false. It did not: every confirm dialog in the app actually
     * resolved `closeOnEscape` to TRUE regardless of what a call site passed, so Escape cancelled a
     * delete confirmation everywhere — and `?? false` silently removed that from all 72
     * confirm()/confirmDelete() call sites.
     *
     * The default is now `true`, restoring parity. Escape REJECTS — never accepts.
     */
    it("by default Escape rejects and closes — closeOnEscape defaults to TRUE", async () => {
      const accept = vi.fn();
      const reject = vi.fn();

      await openConfirm({ message: "Delete it?", accept, reject });
      pressEscape();
      await settle();

      expect(accept).not.toHaveBeenCalled();
      expect(reject).toHaveBeenCalledTimes(1);
      expect(confirmSurface()).toBeNull();
    });

    /** The opt-OUT still works: a caller may explicitly refuse Escape. */
    it("closeOnEscape: false — Escape does nothing, the dialog stays open, neither callback fires", async () => {
      const accept = vi.fn();
      const reject = vi.fn();

      await openConfirm({ message: "Delete it?", closeOnEscape: false, accept, reject });
      pressEscape();
      await settle();

      expect(accept).not.toHaveBeenCalled();
      expect(reject).not.toHaveBeenCalled();
      expect(confirmSurface()).not.toBeNull();
    });

    it("dismissableMask: clicking the backdrop rejects, never accepts", async () => {
      const accept = vi.fn();
      const reject = vi.fn();

      await openConfirm({
        message: "Cancel your subscription?",
        dismissableMask: true,
        accept,
        reject,
      });
      theDialog();
      expect(clickBackdrop()).toBe(true);
      await settle();

      expect(accept).not.toHaveBeenCalled();
      expect(reject).toHaveBeenCalledTimes(1);
      expect(confirmSurface()).toBeNull();
    });

    it("without dismissableMask: clicking the backdrop does nothing", async () => {
      const accept = vi.fn();
      const reject = vi.fn();

      await openConfirm({ message: "Delete it?", accept, reject });
      expect(clickBackdrop()).toBe(true);
      await settle();

      expect(accept).not.toHaveBeenCalled();
      expect(reject).not.toHaveBeenCalled();
      expect(confirmSurface()).not.toBeNull();
    });

    /** The exact option set `manage-subscription.ts` passes — every field at once. */
    it("survives the full manage-subscription option set", async () => {
      const accept = vi.fn();

      await openConfirm({
        message:
          "Are you sure you want to cancel your subscription? Your subscription will be cancelled at the end of the billing cycle.",
        header: "Cancel Subscription",
        icon: "warning",
        acceptLabel: "Yes, Cancel",
        rejectLabel: "No, Keep",
        severity: "danger",
        rejectSeverity: "success",
        acceptIcon: "check",
        rejectIcon: "close",
        closeOnEscape: true,
        dismissableMask: true,
        accept,
      });

      const dialog = theDialog();
      const acceptButton = buttonNamed(dialog, "Yes, Cancel");
      const rejectButton = buttonNamed(dialog, "No, Keep");

      expect(text(dialog)).toContain("Cancel Subscription");
      expect(text(dialog)).toContain("cancelled at the end of the billing cycle");
      expect(rendersIcon(dialog)).toBe(true);
      expect(renderedIntent(acceptButton)).toBe("danger");
      expect(renderedIntent(rejectButton)).toBe("success");
      expect(rendersIcon(acceptButton)).toBe(true);
      expect(rendersIcon(rejectButton)).toBe(true);
      expect(accept).not.toHaveBeenCalled();

      acceptButton.click();
      await settle();
      expect(accept).toHaveBeenCalledTimes(1);
    });
  });

  describe("confirmDelete() — the ~200-call-site path where a silent failure destroys data", () => {
    it("names the entity in the message and headers the dialog 'Confirm Delete'", async () => {
      toast.confirmDelete("customer", vi.fn());
      await settle();

      const dialog = theDialog();
      expect(text(dialog)).toContain("customer");
      expect(text(dialog)).toContain("Confirm Delete");
    });

    it("does NOT call onAccept until the user accepts", async () => {
      const onAccept = vi.fn();

      toast.confirmDelete("truck", onAccept);
      await settle();

      theDialog();
      expect(onAccept).not.toHaveBeenCalled();
    });

    it("calls onAccept EXACTLY ONCE on acceptance", async () => {
      const onAccept = vi.fn();

      toast.confirmDelete("truck", onAccept);
      await settle();
      buttonNamed(theDialog(), DEFAULT_ACCEPT_LABEL).click();
      await settle();

      expect(onAccept).toHaveBeenCalledTimes(1);
      expect(confirmSurface()).toBeNull();
    });

    it("never calls onAccept when the user rejects", async () => {
      const onAccept = vi.fn();

      toast.confirmDelete("truck", onAccept);
      await settle();
      buttonNamed(theDialog(), DEFAULT_REJECT_LABEL).click();
      await settle();

      expect(onAccept).not.toHaveBeenCalled();
      expect(confirmSurface()).toBeNull();
    });
  });
});
