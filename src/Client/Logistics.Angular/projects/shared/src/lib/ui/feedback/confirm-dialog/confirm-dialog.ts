import { DialogRef } from "@angular/cdk/dialog";
import { ChangeDetectionStrategy, Component, ElementRef, inject } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { BrnDialogRef, injectBrnDialogContext } from "@spartan-ng/brain/dialog";
import { UiButton } from "../../action/button/button";
import type { UiButtonIntent } from "../../action/button/button-variants";
import { Icon } from "../../content/icon/icon";
import type { IconName } from "../../icons/icons";
import { isTopmostOverlay } from "../../internal/overlay-stack";

/**
 * The dialog's close result. `confirm()` maps it back to the caller's callbacks, and the mapping is
 * deliberately NOT boolean-coercing: a dialog torn down some other way (the app shell being destroyed,
 * `Dialog.closeAll()`) closes with `undefined`, which must fire NEITHER callback. `!result` would have
 * turned that into a spurious reject, and — far worse — any future `close()` with a truthy result into
 * a delete nobody asked for.
 */
export const CONFIRM_ACCEPT = true;
export const CONFIRM_REJECT = false;

/**
 * What `ToastService.confirm()` hands the dialog. Every field is already RESOLVED — semantic
 * `ConfirmIcon` / `ConfirmSeverity` tokens are mapped to `IconName` / `UiButtonIntent` by the service,
 * so this component knows nothing about the call sites' vocabulary and holds no mapping table.
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
 * The confirmation dialog behind `ToastService.confirm()` / `.confirmDelete()` — ~200 of the call
 * sites behind it are deletes, so the asymmetry here is the whole point: a dialog that fails to open
 * makes "Delete" do nothing (annoying, recoverable), but an accept that fires without the user
 * accepting DESTROYS DATA. Every path below is written so that `CONFIRM_ACCEPT` is reachable from
 * exactly one place: the accept button's click handler.
 *
 * It is opened IMPERATIVELY (`HlmDialogService`), never placed in a template — hence no inputs and no
 * `ui-*` call sites. Options arrive through the brain dialog context (CDK's `DIALOG_DATA`).
 *
 * WHY THIS COMPONENT ARBITRATES ESCAPE AND BACKDROP ITSELF
 * `closeOnEscape` and `dismissableMask` are INDEPENDENT options in the public `ConfirmOptions` (and
 * `manage-subscription.ts` passes both), but brain cannot express them independently:
 *
 *     dismiss(reason) {
 *       if (!this.open || options.disableClose) return false;              // gates BOTH gestures
 *       if (reason === 'outside' && !options.closeOnOutsidePointerEvents) return false;
 *       this.close(); return true;                                          // 'backdrop' is NOT gated
 *     }
 *
 * `disableClose` gates escape and backdrop together, and `closeOnOutsidePointerEvents` gates only the
 * `'outside'` reason — never `'backdrop'`. So there is no option combination meaning "escape closes but
 * the backdrop does not". `ToastService` therefore opens with `disableClose: true` — brain never closes
 * the dialog on its own — and the two gestures are wired here, one flag each.
 */
@Component({
  selector: "ui-confirm-dialog",
  templateUrl: "./confirm-dialog.html",
  imports: [UiButton, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    // On `document`, not on the host: the dialog does not necessarily hold focus (autofocus lands on
    // the first tabbable child, and a user may have clicked the backdrop), and a keydown on an element
    // outside this component would never reach a host-scoped listener.
    "(document:keydown.escape)": "onEscape()",
  },
})
export class UiConfirmDialog {
  private readonly dialogRef = inject<BrnDialogRef<boolean>>(BrnDialogRef);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  protected readonly context = injectBrnDialogContext<UiConfirmDialogContext>();

  constructor() {
    // CDK's `DialogRef` (brain opens through `@angular/cdk/dialog`, and passes CDK `disableClose: true`
    // unconditionally, implementing its own dismissal on top). So this stream is the RAW backdrop click,
    // ungated by any of brain's options — which is exactly what we need in order to gate it ourselves.
    inject(DialogRef)
      .backdropClick.pipe(takeUntilDestroyed())
      .subscribe(() => {
        if (this.context.dismissableMask) {
          this.reject();
        }
      });
  }

  /**
   * `isTopmostOverlay` guards the same bug as in `ui-dialog`: a confirm opened ON TOP of a `ui-dialog`
   * (the customer edit dialog's Danger Zone → "Delete Customer" → confirm) shares this document-level
   * listener with the dialog underneath it, and an unguarded Escape closed BOTH — dismissing the
   * confirm and discarding the edit form behind it. See `internal/overlay-stack.ts`.
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
