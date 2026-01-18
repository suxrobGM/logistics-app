import { Component, input } from "@angular/core";
import { type FormControl, ReactiveFormsModule } from "@angular/forms";
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputTextModule } from "primeng/inputtext";

@Component({
  selector: "shared-unit-input",
  templateUrl: "./unit-input.html",
  imports: [InputGroupModule, InputGroupAddonModule, InputTextModule, ReactiveFormsModule],
})
export class UnitInput {
  public readonly control = input.required<FormControl<number | string | null>>();
  public readonly unit = input.required<string>();
  public readonly type = input<"number" | "text">("number");
  public readonly min = input<number | null>(null);
  public readonly max = input<number | null>(null);
  public readonly placeholder = input<string>("");
}
