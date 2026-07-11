import { Component, inject, signal } from "@angular/core";
import { ToastService, type ConfirmOptions } from "@logistics/shared";
import { Typography, UiButton } from "@logistics/shared/ui";

/** One line in the outcome log. `at` disambiguates two identical outcomes in a row. */
interface Outcome {
  readonly at: string;
  readonly text: string;
}

/**
 * Toasts + the confirmation dialog, driven through the REAL `ToastService` (it is `providedIn: 'root'`,
 * so this is the same instance the whole app uses — no fixture, no stub).
 *
 * WHY THE OUTCOME LOG IS THE POINT: `confirm()`'s failure modes are invisible in a screenshot. A
 * dialog that opens but drops `accept` looks identical to one that works, and one that fires `accept`
 * on dismiss looks identical right up until it deletes something. So every button here logs which
 * callback actually ran, which makes the two dangerous assertions checkable in a browser:
 *   - dismiss the dialog  => the log must say REJECT, and must NOT say ACCEPT;
 *   - accept the dialog   => the log must say ACCEPT exactly once.
 * `data-lab` hooks let a probe read the log without depending on copy.
 */
@Component({
  selector: "app-ui-lab-notifications",
  templateUrl: "./notifications-section.html",
  imports: [Typography, UiButton],
})
export class UiLabNotificationsSection {
  private readonly toast = inject(ToastService);

  protected readonly outcomes = signal<readonly Outcome[]>([]);

  protected clear(): void {
    this.outcomes.set([]);
  }

  private log(text: string): void {
    const at = new Date().toISOString().slice(11, 23);
    this.outcomes.update((rows) => [{ at, text }, ...rows]);
  }

  protected showSuccess(): void {
    this.toast.showSuccess("Load LX-1042 was created.");
  }

  protected showError(): void {
    this.toast.showError("Could not reach the server.");
  }

  protected showWarning(): void {
    this.toast.showWarning("Driver is over hours.");
  }

  protected showInfo(): void {
    this.toast.showInfo("Sync finished.");
  }

  protected showCustomTitle(): void {
    this.toast.showSuccess("Payroll posted.", "Saved");
  }

  /** The `confirmDelete` path — the one behind ~200 call sites. Defaults: "Yes" / "No", danger accept. */
  protected confirmDelete(): void {
    this.toast.confirmDelete("truck", () => this.log("ACCEPT — confirmDelete('truck')"));
  }

  /** The bare `confirm()` shape `truck-form.askRemove()` uses: no icon, no severity, default labels. */
  protected confirmPlain(): void {
    this.open({
      message: "Are you sure you want to delete this truck?",
      accept: () => this.log("ACCEPT — plain confirm"),
      reject: () => this.log("REJECT — plain confirm"),
    });
  }

  /** Escapable, but NOT mask-dismissable — proves the two flags are independent. */
  protected confirmEscapable(): void {
    this.open({
      message: "Escape closes this one. Clicking the backdrop must NOT.",
      header: "Escape only",
      icon: "question",
      closeOnEscape: true,
      accept: () => this.log("ACCEPT — escapable"),
      reject: () => this.log("REJECT — escapable"),
    });
  }

  /** Mask-dismissable, but NOT escapable — the mirror image of the above. */
  protected confirmDismissable(): void {
    this.open({
      message: "The backdrop closes this one. Escape must NOT.",
      header: "Backdrop only",
      icon: "info",
      dismissableMask: true,
      accept: () => this.log("ACCEPT — dismissable"),
      reject: () => this.log("REJECT — dismissable"),
    });
  }

  /** Neither flag: the dialog is modal and the user MUST choose. This is what `confirmDelete` does. */
  protected confirmTrapped(): void {
    this.open({
      message: "Neither Escape nor the backdrop may close this. Only Yes or No.",
      header: "No way out",
      accept: () => this.log("ACCEPT — trapped"),
      reject: () => this.log("REJECT — trapped"),
    });
  }

  /** The exact option set `manage-subscription.ts` passes — every field of `ConfirmOptions` at once. */
  protected confirmEverything(): void {
    this.open({
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
      accept: () => this.log("ACCEPT — manage-subscription"),
      reject: () => this.log("REJECT — manage-subscription"),
    });
  }

  /** A multi-line message — the `whitespace-pre-line` the TMS shell used to carry on its dialog. */
  protected confirmMultiline(): void {
    this.open({
      message: "This load has 2 unpaid invoices.\n\nDelete it anyway?",
      header: "Confirm Delete",
      icon: "warning",
      severity: "danger",
      accept: () => this.log("ACCEPT — multiline"),
      reject: () => this.log("REJECT — multiline"),
    });
  }

  private open(options: ConfirmOptions): void {
    this.toast.confirm(options);
  }
}
