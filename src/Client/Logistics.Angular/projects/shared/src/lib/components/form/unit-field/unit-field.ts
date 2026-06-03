import { Component, input } from "@angular/core";
import { ReactiveFormsModule, type FormControl } from "@angular/forms";
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputTextModule } from "primeng/inputtext";

@Component({
  selector: "ui-unit-field",
  templateUrl: "./unit-field.html",
  imports: [InputGroupModule, InputGroupAddonModule, InputTextModule, ReactiveFormsModule],
})
export class UnitField {
  public readonly control = input.required<FormControl<number | string | null>>();
  public readonly unit = input.required<string>();
  public readonly type = input<"number" | "text">("number");
  public readonly min = input<number | null>(null);
  public readonly max = input<number | null>(null);
  public readonly placeholder = input<string>("");
}
