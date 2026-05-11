import { Component, input } from "@angular/core";
import { FormGroup, ReactiveFormsModule } from "@angular/forms";
import { hazmatClassOptions } from "@logistics/shared/api/enums";
import { FormField } from "@logistics/shared/components";
import { CheckboxModule } from "primeng/checkbox";
import { DatePickerModule } from "primeng/datepicker";
import { Fieldset } from "primeng/fieldset";
import { InputTextModule } from "primeng/inputtext";
import { MultiSelectModule } from "primeng/multiselect";

@Component({
  selector: "app-truck-hazmat-section",
  templateUrl: "./truck-hazmat-section.html",
  imports: [
    ReactiveFormsModule,
    CheckboxModule,
    DatePickerModule,
    Fieldset,
    InputTextModule,
    MultiSelectModule,
    FormField,
  ],
})
export class TruckHazmatSection {
  public readonly form = input.required<FormGroup>();

  protected readonly hazmatClassOptions = hazmatClassOptions;
}
