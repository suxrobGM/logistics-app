import { Component, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { UiButton, UiDialog, UiTextareaField } from "@logistics/shared/ui";
import { ExpenseActionsService } from "../../services/expense-actions.service";

@Component({
  selector: "app-reject-expense-dialog",
  templateUrl: "./reject-expense-dialog.html",
  imports: [FormsModule, UiButton, UiDialog, UiTextareaField],
})
export class RejectExpenseDialog {
  protected readonly service = inject(ExpenseActionsService);
}
