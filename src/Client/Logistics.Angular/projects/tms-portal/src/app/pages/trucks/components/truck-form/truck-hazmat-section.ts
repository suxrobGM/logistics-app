import { Component, input } from "@angular/core";
import { FormGroup, ReactiveFormsModule } from "@angular/forms";
import { hazmatClassOptions } from "@logistics/shared/api/enums";
import {
  UiCheckboxField,
  UiDateField,
  UiFormField,
  UiMultiSelectField,
  UiTextField,
} from "@logistics/shared/components";
import { Fieldset } from "primeng/fieldset";

@Component({
  selector: "app-truck-hazmat-section",
  templateUrl: "./truck-hazmat-section.html",
  imports: [
    ReactiveFormsModule,
    Fieldset,
    UiFormField,
    UiCheckboxField,
    UiDateField,
    UiMultiSelectField,
    UiTextField,
  ],
})
export class TruckHazmatSection {
  public readonly form = input.required<FormGroup>();

  protected readonly hazmatClassOptions = hazmatClassOptions;
}
