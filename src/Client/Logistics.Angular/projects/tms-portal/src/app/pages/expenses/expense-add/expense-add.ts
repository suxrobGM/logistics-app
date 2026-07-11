import { Component, signal } from "@angular/core";
import { Card, UiTabsImports } from "@logistics/shared/ui";
import { Container, PageHeader } from "@/shared/components";
import { BodyShopExpenseForm, CompanyExpenseForm, TruckExpenseForm } from "../_components";

@Component({
  selector: "app-expense-add",
  templateUrl: "./expense-add.html",
  imports: [
    BodyShopExpenseForm,
    Card,
    CompanyExpenseForm,
    Container,
    PageHeader,
    TruckExpenseForm,
    UiTabsImports,
  ],
})
export class ExpenseAddPage {
  protected readonly activeTab = signal(0);

  protected onTabChange(index: string | number | undefined): void {
    if (typeof index === "number") this.activeTab.set(index);
  }
}
