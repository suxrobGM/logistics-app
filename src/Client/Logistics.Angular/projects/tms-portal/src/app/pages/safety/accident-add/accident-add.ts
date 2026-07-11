import { Component, inject, signal } from "@angular/core";
import { form, min, required, submit } from "@angular/forms/signals";
import { Router } from "@angular/router";
import { Api, createAccidentReport, type CreateAccidentReportCommand } from "@logistics/shared/api";
import { Card, Spinner, UiButton, UiStepperImports } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { Converters } from "@/shared/utils";
import {
  AccidentIncidentForm,
  AccidentInjuriesDamageForm,
  AccidentReviewSummary,
  type AccidentIncidentModel,
  type AccidentInjuriesDamageModel,
} from "../_components";

@Component({
  selector: "app-accident-add",
  templateUrl: "./accident-add.html",
  imports: [
    AccidentIncidentForm,
    AccidentInjuriesDamageForm,
    AccidentReviewSummary,
    Card,
    PageHeader,
    Spinner,
    UiStepperImports,
    UiButton,
  ],
})
export class AccidentAddPage {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly activeStep = signal(1);

  // Step 1: Incident Details
  protected readonly step1Model = signal<AccidentIncidentModel>({
    accidentDateTime: new Date(),
    location: null,
    truck: null,
    driver: null,
    type: "collision",
    severity: "minor",
    description: "",
    weatherConditions: "",
    roadConditions: "",
  });

  protected readonly step1Form = form(this.step1Model, (p) => {
    required(p.accidentDateTime, { message: "Date and time is required." });
    required(p.location, { message: "Location is required." });
    required(p.truck, { message: "Truck is required." });
    required(p.driver, { message: "Driver is required." });
    required(p.type, { message: "Accident type is required." });
    required(p.severity, { message: "Severity is required." });
    required(p.description, { message: "Description is required." });
  });

  // Step 2: Injuries & Damage
  protected readonly step2Model = signal<AccidentInjuriesDamageModel>({
    injuriesReported: false,
    numberOfInjuries: null,
    injuryDescription: "",
    fatalitiesReported: false,
    numberOfFatalities: null,
    hazmatInvolved: false,
    hazmatDescription: "",
    estimatedDamage: null,
    damageDescription: "",
    vehicleTowed: false,
    towCompany: "",
  });

  protected readonly step2Form = form(this.step2Model, (p) => {
    min(p.numberOfInjuries, 0, { message: "Number of injuries cannot be negative." });
    min(p.numberOfFatalities, 0, { message: "Number of fatalities cannot be negative." });
  });

  protected async nextStep(): Promise<void> {
    // submit() marks the whole step's field tree touched (revealing inline errors) and
    // resolves false when invalid, so the step only advances once it validates.
    if (this.activeStep() === 1) {
      if (!(await submit(this.step1Form, async () => undefined))) {
        return;
      }
    } else if (this.activeStep() === 2) {
      if (!(await submit(this.step2Form, async () => undefined))) {
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
    const step1Valid = await submit(this.step1Form, async () => undefined);
    const step2Valid = await submit(this.step2Form, async () => undefined);
    if (!step1Valid || !step2Valid) {
      return;
    }

    this.isSaving.set(true);
    try {
      const step1 = this.step1Model();
      const step2 = this.step2Model();

      const command: CreateAccidentReportCommand = {
        accidentDateTime: step1.accidentDateTime.toISOString(),
        location: Converters.addressToString(step1.location),
        truckId: step1.truck?.id ?? "",
        driverId: step1.driver?.id ?? "",
        type: step1.type,
        severity: step1.severity,
        description: step1.description,
        weatherConditions: step1.weatherConditions || null,
        roadConditions: step1.roadConditions || null,
        injuriesReported: step2.injuriesReported,
        numberOfInjuries: step2.numberOfInjuries,
        injuryDescription: step2.injuryDescription || null,
        fatalitiesReported: step2.fatalitiesReported,
        numberOfFatalities: step2.numberOfFatalities,
        hazmatInvolved: step2.hazmatInvolved,
        hazmatDescription: step2.hazmatDescription || null,
        estimatedDamage: step2.estimatedDamage,
        damageDescription: step2.damageDescription || null,
        vehicleTowed: step2.vehicleTowed,
        towCompany: step2.towCompany || null,
      };

      const result = await this.api.invoke(createAccidentReport, { body: command });
      if (result) {
        this.toastService.showSuccess("Accident report created successfully");
        this.router.navigateByUrl(`/safety/accidents/${result.id}`);
      }
    } catch {
      this.toastService.showError("Failed to create accident report");
    } finally {
      this.isSaving.set(false);
    }
  }
}
