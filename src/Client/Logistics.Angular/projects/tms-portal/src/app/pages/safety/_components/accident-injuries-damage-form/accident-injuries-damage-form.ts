import { Component, input } from "@angular/core";
import { ReactiveFormsModule, type FormGroup } from "@angular/forms";
import {
  UiFormField,
  UiNumberField,
  UiTextareaField,
  UiTextField,
  UiToggleField,
} from "@logistics/shared/components";
import { CardModule } from "primeng/card";

@Component({
  selector: "app-accident-injuries-damage-form",
  templateUrl: "./accident-injuries-damage-form.html",
  imports: [
    ReactiveFormsModule,
    CardModule,
    UiNumberField,
    UiTextField,
    UiTextareaField,
    UiToggleField,
    UiFormField,
  ],
})
export class AccidentInjuriesDamageForm {
  public readonly form = input.required<FormGroup>();
}
