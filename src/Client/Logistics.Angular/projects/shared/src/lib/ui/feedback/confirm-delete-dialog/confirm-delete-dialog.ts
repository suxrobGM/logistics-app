import { LowerCasePipe } from "@angular/common";
import { Component, computed, effect, input, model, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";

/**
 * Reusable confirmation dialog that requires typing the entity name to confirm deletion.
 *
 * Usage:
 * ```html
 * <ui-confirm-delete-dialog
 *   entityLabel="Tenant"
 *   [entityName]="tenantName()"
 *   [(visible)]="deleteDialogVisible"
 *   [deleting]="isDeleting()"
 *   (confirm)="onDelete()"
 * />
 * ```
 */
@Component({
  selector: "ui-confirm-delete-dialog",
  templateUrl: "./confirm-delete-dialog.html",
  imports: [ButtonModule, DialogModule, InputTextModule, FormsModule, LowerCasePipe],
})
export class ConfirmDeleteDialog {
  public readonly entityLabel = input("Item");
  public readonly entityName = input.required<string>();
  public readonly visible = model(false);
  public readonly deleting = input(false);
  public readonly confirm = output<void>();

  protected readonly confirmInput = signal("");
  protected readonly isConfirmValid = computed(() => this.confirmInput() === this.entityName());

  constructor() {
    effect(() => {
      if (this.visible()) {
        this.confirmInput.set("");
      }
    });
  }

  protected onConfirm(): void {
    if (!this.isConfirmValid()) return;
    this.confirm.emit();
  }
}
