import { Component, input } from "@angular/core";
import { ReactiveFormsModule, type FormGroup } from "@angular/forms";
import { UiDateField, UiFormField, UiSelectField, UiTextField } from "@logistics/shared/components";
import { TextareaModule } from "primeng/textarea";
import { AddressAutocomplete } from "@/shared/components/maps";
import { SearchEmployee, SearchTruck } from "@/shared/components/search";
import { ACCIDENT_SEVERITY_OPTIONS, ACCIDENT_TYPE_OPTIONS } from "../accident.constants";

@Component({
  selector: "app-accident-incident-form",
  templateUrl: "./accident-incident-form.html",
  imports: [
    ReactiveFormsModule,
    TextareaModule,
    UiFormField,
    UiDateField,
    UiSelectField,
    UiTextField,
    SearchEmployee,
    SearchTruck,
    AddressAutocomplete,
  ],
})
export class AccidentIncidentForm {
  public readonly form = input.required<FormGroup>();

  protected readonly typeOptions = ACCIDENT_TYPE_OPTIONS;
  protected readonly severityOptions = ACCIDENT_SEVERITY_OPTIONS;
}
