import { Component, input } from "@angular/core";
import { type FormGroup, ReactiveFormsModule } from "@angular/forms";
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { DatePickerModule } from "primeng/datepicker";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { AddressAutocomplete } from "@/shared/components/maps";
import { SearchEmployee, SearchTruck } from "@/shared/components/search";
import { ACCIDENT_SEVERITY_OPTIONS, ACCIDENT_TYPE_OPTIONS } from "../accident.constants";

@Component({
  selector: "app-accident-incident-form",
  templateUrl: "./accident-incident-form.html",
  imports: [
    ReactiveFormsModule,
    DatePickerModule,
    InputTextModule,
    SelectModule,
    TextareaModule,
    LabeledField,
    ValidationSummary,
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
