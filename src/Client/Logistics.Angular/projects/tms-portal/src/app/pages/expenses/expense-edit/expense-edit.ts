import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import { Api, getExpenseById, type ExpenseDto } from "@logistics/shared/api";
import { Container, Stack, Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { PageHeader } from "@/shared/components";
import {
  BodyShopExpenseForm,
  CompanyExpenseForm,
  getExpenseTypeLabel,
  TruckExpenseForm,
} from "../_components";

@Component({
  selector: "app-expense-edit",
  templateUrl: "./expense-edit.html",
  imports: [
    CardModule,
    ProgressSpinnerModule,
    PageHeader,
    Container,
    Stack,
    Typography,
    CompanyExpenseForm,
    TruckExpenseForm,
    BodyShopExpenseForm,
  ],
})
export class ExpenseEditPage implements OnInit {
  private readonly api = inject(Api);

  protected readonly id = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly expense = signal<ExpenseDto | null>(null);
  protected readonly headerTitle = computed(
    () => `Edit ${getExpenseTypeLabel(this.expense()?.type)}`,
  );

  ngOnInit(): void {
    this.loadExpense();
  }

  private async loadExpense(): Promise<void> {
    this.isLoading.set(true);
    const result = await this.api.invoke(getExpenseById, { id: this.id() });
    if (result) this.expense.set(result);
    this.isLoading.set(false);
  }
}
