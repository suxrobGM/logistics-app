import { Component, input } from "@angular/core";
import { FormField, type FieldTree } from "@angular/forms/signals";
import {
  Card,
  UiFormField,
  UiNumberField,
  UiTextareaField,
  UiTextField,
  UiToggleField,
} from "@logistics/shared/ui";
import { type AccidentInjuriesDamageModel } from "../accident.constants";

@Component({
  selector: "app-accident-injuries-damage-form",
  templateUrl: "./accident-injuries-damage-form.html",
  imports: [
    Card,
    FormField,
    UiFormField,
    UiNumberField,
    UiTextareaField,
    UiTextField,
    UiToggleField,
  ],
})
export class AccidentInjuriesDamageForm {
  public readonly field = input.required<FieldTree<AccidentInjuriesDamageModel>>();
}
