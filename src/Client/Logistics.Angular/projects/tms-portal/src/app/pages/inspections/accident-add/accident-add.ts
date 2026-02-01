import { Component, inject, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { CurrencyPipe, DatePipe } from "@angular/common";
import { Api, createAccidentReport } from "@logistics/shared/api";
import type {
  AccidentSeverity,
  AccidentType,
  Address,
  CreateAccidentReportCommand,
  EmployeeDto,
  TruckDto,
} from "@logistics/shared/api";
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DatePickerModule } from "primeng/datepicker";
import { CardModule } from "primeng/card";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { StepperModule } from "primeng/stepper";
import { TagModule } from "primeng/tag";
import { TextareaModule } from "primeng/textarea";
import { ToggleSwitchModule } from "primeng/toggleswitch";
import { PageHeader } from "@/shared/components";
import { SearchEmployee, SearchTruck } from "@/shared/components/search";
import { AddressAutocomplete } from "@/shared/components/maps";
import { ToastService } from "@/core/services";
import { Converters } from "@/shared/utils";

const accidentTypeOptions = [
  { label: "Collision", value: "collision" },
  { label: "Rollover", value: "rollover" },
  { label: "Jackknife", value: "jackknife" },
  { label: "Run Off Road", value: "run_off_road" },
  { label: "Rear End", value: "rear_end" },
  { label: "Sideswipe", value: "sideswipe" },
  { label: "Head On", value: "head_on" },
  { label: "Hit and Run", value: "hit_and_run" },
  { label: "Pedestrian Involved", value: "pedestrian_involved" },
  { label: "Property Damage Only", value: "property_damage_only" },
  { label: "Cargo Spill", value: "cargo_spill" },
  { label: "Other", value: "other" },
];

const severityOptions = [
  { label: "Minor", value: "minor" },
  { label: "Moderate", value: "moderate" },
  { label: "Severe", value: "severe" },
  { label: "Fatal", value: "fatal" },
];

@Component({
  selector: "app-accident-add",
  templateUrl: "./accident-add.html",
  imports: [
    CurrencyPipe,
    DatePipe,
    ReactiveFormsModule,
    ButtonModule,
    DatePickerModule,
    CardModule,
    InputNumberModule,
    InputTextModule,
    ProgressSpinnerModule,
    SelectModule,
    StepperModule,
    TagModule,
    TextareaModule,
    ToggleSwitchModule,
    PageHeader,
    LabeledField,
    ValidationSummary,
    SearchEmployee,
    SearchTruck,
    AddressAutocomplete,
  ],
})
export class AccidentAddPage {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly activeStep = signal(1);

  protected readonly accidentTypeOptions = accidentTypeOptions;
  protected readonly severityOptions = severityOptions;

  // Step 1: Incident Details
  protected readonly step1Form = new FormGroup({
    accidentDateTime: new FormControl<Date>(new Date(), {
      validators: Validators.required,
      nonNullable: true,
    }),
    location: new FormControl<Address | null>(null, { validators: Validators.required }),
    truck: new FormControl<TruckDto | null>(null, { validators: Validators.required }),
    driver: new FormControl<EmployeeDto | null>(null, { validators: Validators.required }),
    type: new FormControl<AccidentType>("collision", {
      validators: Validators.required,
      nonNullable: true,
    }),
    severity: new FormControl<AccidentSeverity>("minor", {
      validators: Validators.required,
      nonNullable: true,
    }),
    description: new FormControl<string>("", { validators: Validators.required, nonNullable: true }),
    weatherConditions: new FormControl<string | null>(null),
    roadConditions: new FormControl<string | null>(null),
  });

  // Step 2: Injuries & Damage
  protected readonly step2Form = new FormGroup({
    injuriesReported: new FormControl<boolean>(false, { nonNullable: true }),
    numberOfInjuries: new FormControl<number | null>(null),
    injuryDescription: new FormControl<string | null>(null),
    fatalitiesReported: new FormControl<boolean>(false, { nonNullable: true }),
    numberOfFatalities: new FormControl<number | null>(null),
    hazmatInvolved: new FormControl<boolean>(false, { nonNullable: true }),
    hazmatDescription: new FormControl<string | null>(null),
    estimatedDamage: new FormControl<number | null>(null),
    damageDescription: new FormControl<string | null>(null),
    vehicleTowed: new FormControl<boolean>(false, { nonNullable: true }),
    towCompany: new FormControl<string | null>(null),
  });

  protected nextStep(): void {
    if (this.activeStep() === 1) {
      if (this.step1Form.invalid) {
        this.step1Form.markAllAsTouched();
        return;
      }
    } else if (this.activeStep() === 2) {
      if (this.step2Form.invalid) {
        this.step2Form.markAllAsTouched();
        return;
      }
    }
    this.activeStep.set(this.activeStep() + 1);
  }

  protected prevStep(): void {
    this.activeStep.set(this.activeStep() - 1);
  }

  protected goToStep(step: number): void {
    // Only allow going back, not forward (validation needed)
    if (step < this.activeStep()) {
      this.activeStep.set(step);
    }
  }

  protected async submit(): Promise<void> {
    this.isSaving.set(true);
    try {
      const step1 = this.step1Form.getRawValue();
      const step2 = this.step2Form.getRawValue();

      const command: CreateAccidentReportCommand = {
        accidentDateTime: step1.accidentDateTime.toISOString(),
        location: Converters.addressToString(step1.location),
        truckId: step1.truck?.id ?? "",
        driverId: step1.driver?.id ?? "",
        type: step1.type,
        severity: step1.severity,
        description: step1.description,
        weatherConditions: step1.weatherConditions,
        roadConditions: step1.roadConditions,
        injuriesReported: step2.injuriesReported,
        numberOfInjuries: step2.numberOfInjuries,
        injuryDescription: step2.injuryDescription,
        fatalitiesReported: step2.fatalitiesReported,
        numberOfFatalities: step2.numberOfFatalities,
        hazmatInvolved: step2.hazmatInvolved,
        hazmatDescription: step2.hazmatDescription,
        estimatedDamage: step2.estimatedDamage,
        damageDescription: step2.damageDescription,
        vehicleTowed: step2.vehicleTowed,
        towCompany: step2.towCompany,
      };

      const result = await this.api.invoke(createAccidentReport, { body: command });
      if (result) {
        this.toastService.showSuccess("Accident report created successfully");
        this.router.navigateByUrl(`/inspections/accidents/${result.id}`);
      }
    } catch {
      this.toastService.showError("Failed to create accident report");
    } finally {
      this.isSaving.set(false);
    }
  }

  protected getSelectedTruck(): TruckDto | null {
    return this.step1Form.get("truck")?.value ?? null;
  }

  protected getSelectedDriver(): EmployeeDto | null {
    return this.step1Form.get("driver")?.value ?? null;
  }

  protected getLocationString(): string {
    const address = this.step1Form.get("location")?.value ?? null;
    return Converters.addressToString(address) ?? "-";
  }

  protected getTypeLabel(type: AccidentType): string {
    return accidentTypeOptions.find((o) => o.value === type)?.label ?? type;
  }

  protected getSeverityLabel(severity: AccidentSeverity): string {
    return severityOptions.find((o) => o.value === severity)?.label ?? severity;
  }
}
