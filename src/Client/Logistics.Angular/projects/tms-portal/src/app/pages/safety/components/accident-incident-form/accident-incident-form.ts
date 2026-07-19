import { Component, input } from "@angular/core";
import { FormField, type FieldTree } from "@angular/forms/signals";
import {
  UiDateField,
  UiFormField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
} from "@logistics/shared/ui";
import { AddressAutocomplete } from "@/shared/components/maps";
import { SearchEmployee, SearchTruck } from "@/shared/components/search";
import {
  ACCIDENT_SEVERITY_OPTIONS,
  ACCIDENT_TYPE_OPTIONS,
  type AccidentIncidentModel,
} from "../accident.constants";

@Component({
  selector: "app-accident-incident-form",
  templateUrl: "./accident-incident-form.html",
  imports: [
    FormField,
    UiFormField,
    UiDateField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    SearchEmployee,
    SearchTruck,
    AddressAutocomplete,
  ],
})
export class AccidentIncidentForm {
  public readonly field = input.required<FieldTree<AccidentIncidentModel>>();

  protected readonly typeOptions = ACCIDENT_TYPE_OPTIONS;
  protected readonly severityOptions = ACCIDENT_SEVERITY_OPTIONS;
}
