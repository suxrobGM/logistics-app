import { Component, input } from "@angular/core";

@Component({
  selector: "app-expense-detail-row",
  templateUrl: "./expense-detail-row.html",
})
export class ExpenseDetailRow {
  public readonly label = input.required<string>();
  public readonly stacked = input(false);
}
