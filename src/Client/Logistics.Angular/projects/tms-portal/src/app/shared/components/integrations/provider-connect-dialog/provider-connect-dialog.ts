import { Component, ElementRef, input, model, output, viewChild } from "@angular/core";
import { FormRoot, type FieldTree } from "@angular/forms/signals";
import { UiButton, UiDialog, ValidatedForm } from "@logistics/shared/ui";

/**
 * Dialog shell for connecting a third-party integration provider (ELD, load board, fuel cards).
 *
 * Owns everything those flows share - the dialog chrome, the `<form [formRoot]>` and its
 * `ValidatedForm` behaviour, the imperative submit, and the footer buttons. The fields differ per
 * integration and stay in the feature component, projected as content so they bind to that
 * component's typed form.
 *
 * The parent owns the async save: its form's submission action emits the command, and the parent
 * binds `saving` back for the button's loading state. It also handles `opened` to reset its own
 * model, which keeps the reset fully typed rather than routed through this generic.
 */
@Component({
  selector: "app-provider-connect-dialog",
  templateUrl: "./provider-connect-dialog.html",
  imports: [FormRoot, UiButton, UiDialog, ValidatedForm],
})
export class ProviderConnectDialog<T> {
  public readonly visible = model.required<boolean>();
  public readonly saving = input(false);
  public readonly header = input.required<string>();
  public readonly submitLabel = input("Add Provider");
  public readonly form = input.required<FieldTree<T>>();

  /** Fires when the dialog opens, so the feature can reset its model. */
  public readonly opened = output<void>();

  private readonly formEl = viewChild.required("formEl", { read: ElementRef });

  /** The footer buttons live outside the `<form>`, so submit it imperatively via a real submit event. */
  protected requestSubmit(): void {
    (this.formEl().nativeElement as HTMLFormElement).requestSubmit();
  }

  protected close(): void {
    this.visible.set(false);
  }
}
