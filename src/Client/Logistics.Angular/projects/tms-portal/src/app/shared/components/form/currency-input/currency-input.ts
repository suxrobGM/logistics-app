import { Component, input } from "@angular/core";
import { ReactiveFormsModule, type FormControl } from "@angular/forms";
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputNumberModule } from "primeng/inputnumber";

@Component({
  selector: "app-currency-input",
  templateUrl: "./currency-input.html",
  imports: [InputGroupModule, InputGroupAddonModule, InputNumberModule, ReactiveFormsModule],
})
export class CurrencyInput {
  public readonly control = input.required<FormControl<number | null>>();
  public readonly currency = input<string>("$");
  public readonly min = input<number>(0);
  public readonly max = input<number | null>(null);
  public readonly placeholder = input<string>("0.00");
}
