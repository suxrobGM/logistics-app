import { inject, Injectable } from "@angular/core";
import { ConfirmationService, MessageService } from "primeng/api";

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

const ICONS: Record<ConfirmIcon, string> = {
  warning: "pi pi-exclamation-triangle",
  success: "pi pi-check-circle",
  question: "pi pi-question-circle",
  info: "pi pi-info-circle",
  send: "pi pi-send",
  payment: "pi pi-credit-card",
  refresh: "pi pi-refresh",
  hide: "pi pi-eye-slash",
  check: "pi pi-check",
  close: "pi pi-times",
};

const BUTTON_CLASS: Record<ConfirmSeverity, string | undefined> = {
  default: undefined,
  danger: "p-button-danger",
  warning: "p-button-warning",
  success: "p-button-success",
};

/**
 * Toast notifications and confirmation dialogs.
 *
 * This is the single seam between the app and whatever notification library backs it.
 * Nothing outside this file may reference `primeng/api` — call sites pass semantic
 * {@link ConfirmIcon} / {@link ConfirmSeverity} tokens, never icon names or CSS classes.
 */
@Injectable({ providedIn: "root" })
export class ToastService {
  private readonly messageService = inject(MessageService);
  private readonly confirmService = inject(ConfirmationService);

  /**
   * Displays a success message toast notification with the given message and title.
   * @param message The message to be displayed in the toast notification.
   * @param title The title of the toast notification. Defaults to "Notification".
   */
  showSuccess(message: string, title = "Notification") {
    this.messageService.add({
      key: "notification",
      severity: "success",
      summary: title,
      detail: message,
    });
  }

  /**
   * Displays an error message toast notification with the given message.
   * @param message The message to be displayed in the toast notification.
   */
  showError(message: string) {
    this.messageService.add({
      key: "notification",
      severity: "error",
      summary: "Error",
      detail: message,
    });
  }

  /**
   * Displays a warning message toast notification with the given message and title.
   * @param message The message to be displayed in the toast notification.
   * @param title The title of the toast notification. Defaults to "Warning".
   */
  showWarning(message: string, title = "Warning") {
    this.messageService.add({
      key: "notification",
      severity: "warn",
      summary: title,
      detail: message,
    });
  }

  /**
   * Displays an info message toast notification with the given message and title.
   * @param message The message to be displayed in the toast notification.
   * @param title The title of the toast notification. Defaults to "Information".
   */
  showInfo(message: string, title = "Information") {
    this.messageService.add({
      key: "notification",
      severity: "info",
      summary: title,
      detail: message,
    });
  }

  /**
   * Displays a confirmation dialog.
   * @param options Message, title, icon/severity tokens and the accept/reject callbacks.
   */
  confirm(options: ConfirmOptions): void {
    this.confirmService.confirm({
      key: "confirmDialog",
      message: options.message,
      header: options.header,
      icon: options.icon ? ICONS[options.icon] : undefined,
      acceptButtonStyleClass: options.severity ? BUTTON_CLASS[options.severity] : undefined,
      rejectButtonStyleClass: options.rejectSeverity
        ? BUTTON_CLASS[options.rejectSeverity]
        : undefined,
      acceptLabel: options.acceptLabel,
      rejectLabel: options.rejectLabel,
      acceptIcon: options.acceptIcon ? ICONS[options.acceptIcon] : undefined,
      rejectIcon: options.rejectIcon ? ICONS[options.rejectIcon] : undefined,
      closeOnEscape: options.closeOnEscape,
      dismissableMask: options.dismissableMask,
      accept: options.accept,
      reject: options.reject,
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
