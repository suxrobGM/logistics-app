import { Component, input } from "@angular/core";
import { FormField, type FieldTree } from "@angular/forms/signals";
import { hazmatClassOptions } from "@logistics/shared/api/enums";
import {
  UiCheckboxField,
  UiCollapsible,
  UiDateField,
  UiFormField,
  UiMultiSelectField,
  UiTextField,
} from "@logistics/shared/ui";
import type { TruckFormModel } from "./truck-form";

@Component({
  selector: "app-truck-hazmat-section",
  templateUrl: "./truck-hazmat-section.html",
  imports: [
    FormField,
    UiCheckboxField,
    UiCollapsible,
    UiDateField,
    UiFormField,
    UiMultiSelectField,
    UiTextField,
  ],
})
export class TruckHazmatSection {
  public readonly field = input.required<FieldTree<TruckFormModel>>();

  protected readonly hazmatClassOptions = hazmatClassOptions;
}
