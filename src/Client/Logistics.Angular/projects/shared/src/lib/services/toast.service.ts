import { inject, Injectable } from "@angular/core";
import { toast } from "@spartan-ng/brain/sonner";
import type { UiButtonIntent } from "../ui/action/button/button-variants";
import {
  CONFIRM_ACCEPT,
  CONFIRM_REJECT,
  UiConfirmDialog,
  type UiConfirmDialogContext,
} from "../ui/feedback/confirm-dialog/confirm-dialog";
import type { IconName } from "../ui/icons/icon-registry.generated";
import { HlmDialogService } from "../ui/primitives/dialog";

/**
 * Semantic icon names for confirmation dialogs. Deliberately *not* icon-library names —
 * this is the seam that lets us swap primeicons for lucide without touching call sites.
 */
export type ConfirmIcon =
  | "warning"
  | "success"
  | "question"
  | "info"
  | "send"
  | "payment"
  | "refresh"
  | "hide"
  | "check"
  | "close";

/** Intent of a confirmation button. Drives its styling. */
export type ConfirmSeverity = "default" | "danger" | "warning" | "success";

/** Library-agnostic options for {@link ToastService.confirm}. */
export interface ConfirmOptions {
  /** Body text of the confirmation. */
  message: string;
  /** Dialog title. */
  header?: string;
  /** Dialog icon. Omit for no icon. */
  icon?: ConfirmIcon;
  /** Accept-button intent. Defaults to the library's neutral button. */
  severity?: ConfirmSeverity;
  /** Reject-button intent. Defaults to the library's neutral button. */
  rejectSeverity?: ConfirmSeverity;
  /** Accept button text. */
  acceptLabel?: string;
  /** Reject button text. */
  rejectLabel?: string;
  /** Icon rendered inside the accept button. */
  acceptIcon?: ConfirmIcon;
  /** Icon rendered inside the reject button. */
  rejectIcon?: ConfirmIcon;
  closeOnEscape?: boolean;
  dismissableMask?: boolean;
  accept: () => void;
  reject?: () => void;
}

/**
 * The seam, resolved. Every one of these ten names is in `BASE_ICON_NAMES` — the set `provideIcons`
 * registers in ALL FOUR portals — so no call site can ask for a glyph its app has not registered.
 * (An unregistered name does not throw; it renders the error glyph. Silent-ish, so: verified.)
 */
const ICONS: Record<ConfirmIcon, IconName> = {
  warning: "exclamation-triangle",
  success: "check-circle",
  question: "question-circle",
  info: "info-circle",
  send: "send",
  payment: "credit-card",
  refresh: "refresh",
  hide: "eye-slash",
  check: "check",
  close: "times",
};

/**
 * `ConfirmSeverity` -> `UiButtonIntent`. Note `warning` -> `warn`: the two vocabularies genuinely
 * differ (`UiButtonIntent` kept PrimeNG's severity spelling), and `"warning"` is not a member of
 * `UiButtonIntent`, so this is a compile error rather than an unstyled button if anyone "fixes" it.
 *
 * The accept button is SOLID and the reject button OUTLINED (see the template), which is what makes
 * the default pair read as a primary + a secondary action. Hence two tables: an intent-less accept is
 * the primary action, an intent-less reject is a neutral one.
 */
const ACCEPT_INTENT: Record<ConfirmSeverity, UiButtonIntent> = {
  default: "primary",
  danger: "danger",
  warning: "warn",
  success: "success",
};

const REJECT_INTENT: Record<ConfirmSeverity, UiButtonIntent> = {
  default: "secondary",
  danger: "danger",
  warning: "warn",
  success: "success",
};

/**
 * The accept/reject labels used when a call site supplies none. `confirmDelete()` — i.e. most of the
 * ~386 call sites — never supplies any, so these two strings are what the user actually reads on the
 * button they click to delete something. They match what PrimeNG's ConfirmDialog defaulted to.
 */
const DEFAULT_ACCEPT_LABEL = "Yes";
const DEFAULT_REJECT_LABEL = "No";

/**
 * Toast notifications and confirmation dialogs.
 *
 * This is the single seam between the app and whatever notification library backs it.
 * Nothing outside this file may reference the rendering library — call sites pass semantic
 * {@link ConfirmIcon} / {@link ConfirmSeverity} tokens, never icon names or CSS classes.
 */
@Injectable({ providedIn: "root" })
export class ToastService {
  private readonly dialogService = inject(HlmDialogService);

  /**
   * Displays a success message toast notification with the given message and title.
   * @param message The message to be displayed in the toast notification.
   * @param title The title of the toast notification. Defaults to "Notification".
   */
  showSuccess(message: string, title = "Notification") {
    toast.success(title, { description: message });
  }

  /**
   * Displays an error message toast notification with the given message.
   * @param message The message to be displayed in the toast notification.
   */
  showError(message: string) {
    toast.error("Error", { description: message });
  }

  /**
   * Displays a warning message toast notification with the given message and title.
   * @param message The message to be displayed in the toast notification.
   * @param title The title of the toast notification. Defaults to "Warning".
   */
  showWarning(message: string, title = "Warning") {
    toast.warning(title, { description: message });
  }

  /**
   * Displays an info message toast notification with the given message and title.
   * @param message The message to be displayed in the toast notification.
   * @param title The title of the toast notification. Defaults to "Information".
   */
  showInfo(message: string, title = "Information") {
    toast.info(title, { description: message });
  }

  /**
   * Displays a confirmation dialog.
   * @param options Message, title, icon/severity tokens and the accept/reject callbacks.
   */
  confirm(options: ConfirmOptions): void {
    const context: UiConfirmDialogContext = {
      message: options.message,
      header: options.header,
      icon: options.icon ? ICONS[options.icon] : undefined,
      acceptLabel: options.acceptLabel ?? DEFAULT_ACCEPT_LABEL,
      rejectLabel: options.rejectLabel ?? DEFAULT_REJECT_LABEL,
      acceptIcon: options.acceptIcon ? ICONS[options.acceptIcon] : undefined,
      rejectIcon: options.rejectIcon ? ICONS[options.rejectIcon] : undefined,
      acceptIntent: ACCEPT_INTENT[options.severity ?? "default"],
      rejectIntent: REJECT_INTENT[options.rejectSeverity ?? "default"],
      // Both default to FALSE, preserving today's behaviour exactly: `confirm()` used to forward
      // `undefined` for these, which the rendering layer coerced to false. So a dialog that has not
      // opted in is NOT escapable and NOT mask-dismissable — the only ways out are Yes and No.
      closeOnEscape: options.closeOnEscape ?? false,
      dismissableMask: options.dismissableMask ?? false,
    };

    const dialogRef = this.dialogService.open<boolean, UiConfirmDialogContext>(UiConfirmDialog, {
      context,
      role: "alertdialog",
      // Brain must never close this dialog by itself — `UiConfirmDialog` arbitrates escape and backdrop,
      // because brain gates both behind this one flag. See the docblock there.
      disableClose: true,
      // No "X". A close affordance that fires NEITHER callback is a cancel button that skips the
      // caller's `reject` cleanup, and `HlmDialogContent` renders one by default.
      showCloseButton: false,
      contentClass: "sm:max-w-lg",
    });

    dialogRef.closed$.subscribe((result) => {
      // Explicit equality, never truthiness: a dialog torn down without a decision closes with
      // `undefined` and must fire nothing at all. See CONFIRM_ACCEPT.
      if (result === CONFIRM_ACCEPT) {
        options.accept();
      } else if (result === CONFIRM_REJECT) {
        options.reject?.();
      }
    });
  }

  /**
   * Displays a delete confirmation dialog with a standard message.
   * @param entityName The name of the entity type being deleted (e.g., "customer", "truck").
   * @param onAccept The callback function to be executed when the delete is confirmed.
   */
  confirmDelete(entityName: string, onAccept: () => void) {
    this.confirm({
      message: `Are you sure that you want to delete this ${entityName}?`,
      header: "Confirm Delete",
      icon: "warning",
      severity: "danger",
      accept: onAccept,
    });
  }
}
