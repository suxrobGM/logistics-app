import { Component, input } from "@angular/core";
import { type FormGroup, ReactiveFormsModule } from "@angular/forms";
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { TextareaModule } from "primeng/textarea";
import { ToggleSwitchModule } from "primeng/toggleswitch";

@Component({
  selector: "app-accident-injuries-damage-form",
  templateUrl: "./accident-injuries-damage-form.html",
  imports: [
    ReactiveFormsModule,
    CardModule,
    InputNumberModule,
    InputTextModule,
    TextareaModule,
    ToggleSwitchModule,
    LabeledField,
    ValidationSummary,
  ],
})
export class AccidentInjuriesDamageForm {
  public readonly form = input.required<FormGroup>();
}
