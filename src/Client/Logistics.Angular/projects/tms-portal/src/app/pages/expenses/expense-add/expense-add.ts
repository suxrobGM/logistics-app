import { Component, signal } from "@angular/core";
import { CardModule } from "primeng/card";
import { TabsModule } from "primeng/tabs";
import { Container, PageHeader } from "@/shared/components";
import { BodyShopExpenseForm, CompanyExpenseForm, TruckExpenseForm } from "../_components";

@Component({
  selector: "app-expense-add",
  templateUrl: "./expense-add.html",
  imports: [
    CardModule,
    TabsModule,
    PageHeader,
    CompanyExpenseForm,
    TruckExpenseForm,
    BodyShopExpenseForm,
    Container,
  ],
})
export class ExpenseAddPage {
  protected readonly activeTab = signal(0);

  protected onTabChange(index: string | number | undefined): void {
    if (typeof index === "number") this.activeTab.set(index);
  }
}
