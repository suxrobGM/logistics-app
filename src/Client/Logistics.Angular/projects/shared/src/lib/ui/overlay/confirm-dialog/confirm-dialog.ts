import { DialogRef } from "@angular/cdk/dialog";
import { ChangeDetectionStrategy, Component, ElementRef, inject } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { BrnDialogRef, injectBrnDialogContext } from "@spartan-ng/brain/dialog";
import { UiButton } from "../../action/button/button";
import type { UiButtonIntent } from "../../action/button/button-variants";
import { Icon } from "../../icons/icon/icon";
import type { IconName } from "../../icons/icons";
import { isTopmostOverlay } from "../../internal/overlay-stack";

/**
 * The dialog's close result. `confirm()` maps it back to the caller's callbacks by comparing against
 * these, never by boolean-coercing: a dialog torn down some other way (`Dialog.closeAll()`, the app
 * shell being destroyed) closes with `undefined`, which must fire neither callback.
 */
export const CONFIRM_ACCEPT = true;
export const CONFIRM_REJECT = false;

/**
 * What `ToastService.confirm()` hands the dialog. Every field is already resolved — the service maps
 * the semantic `ConfirmIcon` / `ConfirmSeverity` tokens to `IconName` / `UiButtonIntent` — so this
 * component holds no mapping table.
 */
export interface UiConfirmDialogContext {
  readonly message: string;
  readonly header?: string;
  readonly icon?: IconName;
  readonly acceptLabel: string;
  readonly rejectLabel: string;
  readonly acceptIcon?: IconName;
  readonly rejectIcon?: IconName;
  readonly acceptIntent: UiButtonIntent;
  readonly rejectIntent: UiButtonIntent;
  readonly closeOnEscape: boolean;
  readonly dismissableMask: boolean;
}

/**
 * The confirmation dialog behind `ToastService.confirm()` / `.confirmDelete()`. Most call sites are
 * deletes, so `CONFIRM_ACCEPT` is reachable from exactly one place — the accept button's click
 * handler. A confirm that fails to open makes "Delete" do nothing; an accept that fires on its own
 * destroys data.
 *
 * Opened imperatively (`HlmDialogService`), never placed in a template; options arrive through the
 * brain dialog context.
 *
 * `ToastService` opens it with `disableClose: true` and Escape / backdrop are gated here, one flag
 * each, because brain cannot express them independently (see `ui-dialog`).
 */
@Component({
  selector: "ui-confirm-dialog",
  templateUrl: "./confirm-dialog.html",
  imports: [UiButton, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    // On `document`: the dialog does not necessarily hold focus, so a keydown on an element outside
    // this component would never reach a host-scoped listener.
    "(document:keydown.escape)": "onEscape()",
  },
})
export class UiConfirmDialog {
  private readonly dialogRef = inject<BrnDialogRef<boolean>>(BrnDialogRef);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  protected readonly context = injectBrnDialogContext<UiConfirmDialogContext>();

  constructor() {
    // Brain opens through `@angular/cdk/dialog` with CDK `disableClose: true`, so this is the raw
    // backdrop click, ungated by brain's options — which is what lets us gate it ourselves.
    inject(DialogRef)
      .backdropClick.pipe(takeUntilDestroyed())
      .subscribe(() => {
        if (this.context.dismissableMask) {
          this.reject();
        }
      });
  }

  /**
   * `isTopmostOverlay` guards the same bug as in `ui-dialog`: a confirm stacked on top of a dialog
   * shares this document listener with it, and an unguarded Escape would close both.
   */
  protected onEscape(): void {
    if (!this.context.closeOnEscape) return;
    if (!isTopmostOverlay(this.host.nativeElement)) return;

    this.reject();
  }

  protected accept(): void {
    this.dialogRef.close(CONFIRM_ACCEPT);
  }

  protected reject(): void {
    this.dialogRef.close(CONFIRM_REJECT);
  }
}
