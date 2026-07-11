import { Component, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { UiButton } from "@logistics/shared/ui";
import { DialogModule } from "primeng/dialog";
import { TextareaModule } from "primeng/textarea";
import { ExpenseActionsService } from "../../services/expense-actions.service";

@Component({
  selector: "app-reject-expense-dialog",
  templateUrl: "./reject-expense-dialog.html",
  imports: [DialogModule, FormsModule, TextareaModule, UiButton],
})
export class RejectExpenseDialog {
  protected readonly service = inject(ExpenseActionsService);
}
