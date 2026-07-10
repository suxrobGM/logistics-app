import { Component, input } from "@angular/core";
import { FormField, type FieldTree } from "@angular/forms/signals";
import {
  UiFormField,
  UiNumberField,
  UiTextareaField,
  UiTextField,
  UiToggleField,
} from "@logistics/shared/ui";
import { CardModule } from "primeng/card";
import { type AccidentInjuriesDamageModel } from "../accident.constants";

@Component({
  selector: "app-accident-injuries-damage-form",
  templateUrl: "./accident-injuries-damage-form.html",
  imports: [
    FormField,
    CardModule,
    UiNumberField,
    UiTextField,
    UiTextareaField,
    UiToggleField,
    UiFormField,
  ],
})
export class AccidentInjuriesDamageForm {
  public readonly field = input.required<FieldTree<AccidentInjuriesDamageModel>>();
}
