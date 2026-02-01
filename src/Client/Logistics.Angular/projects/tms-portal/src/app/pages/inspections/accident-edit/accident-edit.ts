import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { Api, getAccidentReportById, updateAccidentReport } from "@logistics/shared/api";
import type {
  AccidentReportDto,
  AccidentSeverity,
  AccidentType,
  Address,
  EmployeeDto,
  TruckDto,
  UpdateAccidentReportCommand,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { StepperModule } from "primeng/stepper";
import { PageHeader } from "@/shared/components";
import { ToastService } from "@/core/services";
import { Converters } from "@/shared/utils";
import {
  AccidentIncidentForm,
  AccidentInjuriesDamageForm,
  AccidentReviewSummary,
} from "../components";

@Component({
  selector: "app-accident-edit",
  templateUrl: "./accident-edit.html",
  imports: [
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
    StepperModule,
    PageHeader,
    AccidentIncidentForm,
    AccidentInjuriesDamageForm,
    AccidentReviewSummary,
  ],
})
export class AccidentEditPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly activeStep = signal(1);
  protected readonly report = signal<AccidentReportDto | null>(null);

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

  async ngOnInit(): Promise<void> {
    await this.loadReport();
  }

  private async loadReport(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getAccidentReportById, { id: this.id() });
      if (result) {
        this.report.set(result);
        this.populateForm(result);
      } else {
        this.toastService.showError("Accident report not found");
        this.router.navigateByUrl("/inspections/accidents");
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  private populateForm(report: AccidentReportDto): void {
    this.step1Form.patchValue({
      accidentDateTime: report.accidentDateTime ? new Date(report.accidentDateTime) : new Date(),
      location: report.location
        ? ({ formattedAddress: report.location } as unknown as Address)
        : null,
      truck: report.truckId
        ? ({ id: report.truckId, number: report.truckNumber } as TruckDto)
        : null,
      driver: report.driverId
        ? ({ id: report.driverId, fullName: report.driverName } as EmployeeDto)
        : null,
      type: report.type ?? "collision",
      severity: report.severity ?? "minor",
      description: report.description ?? "",
      weatherConditions: report.weatherConditions,
      roadConditions: report.roadConditions,
    });

    this.step2Form.patchValue({
      injuriesReported: report.injuriesReported ?? false,
      numberOfInjuries: report.numberOfInjuries,
      injuryDescription: report.injuryDescription,
      fatalitiesReported: report.fatalitiesReported ?? false,
      numberOfFatalities: report.numberOfFatalities,
      hazmatInvolved: report.hazmatInvolved ?? false,
      hazmatDescription: report.hazmatDescription,
      estimatedDamage: report.estimatedDamage,
      damageDescription: report.damageDescription,
      vehicleTowed: report.vehicleTowed ?? false,
      towCompany: report.towCompany,
    });
  }

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
    if (step < this.activeStep()) {
      this.activeStep.set(step);
    }
  }

  protected async submit(): Promise<void> {
    this.isSaving.set(true);
    try {
      const step1 = this.step1Form.getRawValue();
      const step2 = this.step2Form.getRawValue();

      const command: UpdateAccidentReportCommand = {
        id: this.id(),
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

      const result = await this.api.invoke(updateAccidentReport, { id: this.id(), body: command });
      if (result) {
        this.toastService.showSuccess("Accident report updated successfully");
        this.router.navigateByUrl(`/inspections/accidents/${result.id}`);
      }
    } catch {
      this.toastService.showError("Failed to update accident report");
    } finally {
      this.isSaving.set(false);
    }
  }
}
