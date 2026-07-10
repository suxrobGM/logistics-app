import { Component, computed, inject, input } from "@angular/core";
import { LocalizationService } from "../../../services/localization.service";

/**
 * Renders a money cell augmented with a tax breakdown:
 *   "100.00 USD + 19% (19.00 USD) = 119.00 USD"
 *
 * When `tax` is zero (or `taxRate` <= 0), only the net is rendered. When
 * `mode="compact"` the rate and tax are dropped, leaving "Net (Gross)".
 */
@Component({
  selector: "ui-money-with-tax",
  templateUrl: "./money-with-tax.html",
})
export class MoneyWithTax {
  private readonly localization = inject(LocalizationService);

  public readonly net = input.required<number>();
  public readonly tax = input<number>(0);
  public readonly taxRate = input<number | null>(null);
  public readonly mode = input<"full" | "compact">("full");

  protected readonly hasTax = computed(() => (this.tax() ?? 0) > 0 || (this.taxRate() ?? 0) > 0);
  protected readonly netLabel = computed(() => this.localization.formatCurrency(this.net()));
  protected readonly taxLabel = computed(() => this.localization.formatCurrency(this.tax() ?? 0));
  protected readonly grossLabel = computed(() =>
    this.localization.formatCurrency(this.net() + (this.tax() ?? 0)),
  );
  protected readonly rateLabel = computed(() => this.localization.formatTaxRate(this.taxRate()));
}
