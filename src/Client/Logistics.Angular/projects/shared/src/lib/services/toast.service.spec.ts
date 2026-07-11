/**
 * CONTRACT spec for `ToastService`. It is written to SURVIVE step S8, in which the service's
 * internals are swapped from PrimeNG (`MessageService` / `ConfirmationService`) to
 * `@spartan-ng/brain/sonner`'s imperative `toast` + a Helm-based `ui-confirm-dialog`.
 *
 * ~386 call sites across ~118 files must keep working with ZERO changes, so this spec asserts on
 * WHAT THE USER SEES AND WHAT GETS CALLED — never on `MessageService.add` having been invoked.
 * The failure this guards against is silent and asymmetric:
 *   - dialog never opens AND `accept` is dropped  => "Delete" silently does nothing;
 *   - `accept` fires with no dialog               => it deletes WITHOUT ASKING.
 *
 * ============================ HOW TO MIGRATE THIS FILE IN S8 ============================
 * Rendering happens in `<p-toast>` / `<p-confirm-dialog>`, which live in each app's `app.html`,
 * NOT in the shared lib. So the test host below re-creates those surfaces exactly as the apps do.
 *
 * In S8 you change ONLY the block marked `RENDERING-LAYER ADAPTER`. That block is the complete
 * list of things that know PrimeNG exists: the host component, the DI providers, and four tiny
 * readers that translate "what the rendering layer did" back into our own vocabulary.
 * Every `describe`/`it` body and every `expect(...)` below stays BYTE-IDENTICAL.
 *
 * Concretely, in S8 the adapter becomes: `<hlm-toaster />` + `<ui-confirm-dialog />` in the host,
 * no `MessageService`/`ConfirmationService` providers, and the four readers re-pointed at sonner's
 * `data-type` / the Helm button variant classes.
 * =======================================================================================
 *
 * NOT OBSERVABLE HERE (documented deliberately rather than asserted vacuously):
 *   - toast auto-dismiss after `life` ms — a real timer; out of scope for a contract spec.
 *   - toast stacking/position — pure CSS, invisible to jsdom.
 */
import { Component, provideZonelessChangeDetection } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { ConfirmationService, MessageService } from "primeng/api";
import { ConfirmDialog } from "primeng/confirmdialog";
import { Toast } from "primeng/toast";
import { ToastService, type ConfirmOptions } from "./toast.service";

/** Our own vocabulary — independent of any UI library. */
type Severity = "success" | "error" | "warning" | "info";
type Intent = "default" | "danger" | "warning" | "success";

// ═════════════════════════════ RENDERING-LAYER ADAPTER (PrimeNG today) ═════════════════════════
// ▼▼▼ S8: EVERYTHING BETWEEN THESE MARKERS IS THE ONLY PART THAT CHANGES ▼▼▼

/**
 * Mirrors what every app's `app.html` renders. The `key`s are load-bearing: `ToastService` tags
 * its messages with them, and a surface with the wrong key silently drops everything.
 * (S8: replace with `<hlm-toaster />` + `<ui-confirm-dialog />`.)
 */
@Component({
  selector: "ui-host-notification-surfaces",
  imports: [Toast, ConfirmDialog],
  template: `
    <p-toast key="notification" position="top-right" />
    <p-confirm-dialog key="confirmDialog" position="center" />
  `,
})
class HostNotificationSurfaces {}

/** (S8: drop these — brain/sonner is imperative and needs no providers.) */
const RENDERING_PROVIDERS = [MessageService, ConfirmationService];

/**
 * The accept/reject button labels the rendering layer falls back to when the caller supplies none.
 * `confirmDelete` — i.e. most of the ~386 call sites — relies on these.
 * (S8: if the Helm dialog defaults to something else, that is a deliberate, user-visible copy
 * change; update these two consts, not the tests.)
 */
const DEFAULT_ACCEPT_LABEL = "Yes";
const DEFAULT_REJECT_LABEL = "No";

/** Severity the rendering layer stamped onto a toast, normalized to OUR vocabulary. */
function renderedSeverity(toast: Element): Severity | null {
  const marker = toast.getAttribute("data-p") ?? ""; // S8 (sonner): `data-type`
  if (marker.includes("success")) return "success";
  if (marker.includes("error")) return "error";
  if (marker.includes("warn")) return "warning"; // PrimeNG says "warn", we say "warning"
  if (marker.includes("info")) return "info";
  return null;
}

/** Intent (accept/reject button styling) the rendering layer applied, normalized to OUR vocabulary. */
function renderedIntent(button: Element): Intent {
  const classes = button.className; // S8: the Helm button variant class
  if (classes.includes("p-button-danger")) return "danger";
  if (classes.includes("p-button-warning")) return "warning";
  if (classes.includes("p-button-success")) return "success";
  return "default";
}

/** Did an icon actually get rendered inside this element (excluding icons in nested buttons)? */
function rendersIcon(host: Element): boolean {
  // PrimeNG icons are `<i class="pi pi-*">`. S8: `<ui-icon>` / `<ng-icon>`.
  const icons = [...host.querySelectorAll("i[class*='pi-'], ui-icon, ng-icon")];
  return icons.some((icon) => icon.closest("button") === host.closest("button"));
}

/** The "click outside the dialog" gesture. Returns false if there is no backdrop to click. */
function clickBackdrop(): boolean {
  const backdrop = confirmSurface()?.parentElement; // PrimeNG nests the dialog inside its mask
  if (!backdrop) return false;
  backdrop.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
  return true;
}

// ▲▲▲ S8: END OF THE ONLY PART THAT CHANGES ▲▲▲
// ═══════════════════════════════════════════════════════════════════════════════════════════════

// ───────────────────────── Library-agnostic queries (must NOT change in S8) ─────────────────────
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
 * read. The visible-and-non-empty filter is NOT cosmetic — PrimeNG leaves an empty `<p-dialog>`
 * shell in the DOM (carrying a static `role="alertdialog"`) even while closed, and matching that
 * shell would make every "the dialog opened" assertion below pass vacuously.
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
 * it. Deliberately not `.p-confirm-dialog-accept`: a class query is implementation coupling and
 * would break in S8 for the wrong reason.
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

  // Surfaces may be portaled out of the fixture root (PrimeNG appends its dialog to <body>), so
  // tear the host down explicitly — a dialog left open by one test must not leak into the next.
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
     * CHARACTERIZATION of today's behaviour: `ToastService.confirm` forwards `closeOnEscape:
     * undefined`, which the rendering layer coerces to `false` — so a dialog that does not opt in
     * is NOT escapable. Most call sites (incl. every `confirmDelete`) are in that bucket.
     * If S8 deliberately makes every dialog escapable, this test is the one to change — and the
     * change is safe in the only direction that matters (Escape may reject; it must never accept).
     */
    it("without closeOnEscape: Escape does nothing — the dialog stays open, neither callback fires", async () => {
      const accept = vi.fn();
      const reject = vi.fn();

      await openConfirm({ message: "Delete it?", accept, reject });
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
