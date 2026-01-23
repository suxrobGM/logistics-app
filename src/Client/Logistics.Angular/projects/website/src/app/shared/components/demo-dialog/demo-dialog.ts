import { Component, inject, model, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Api, createDemoRequest } from "@logistics/shared/api";
import { LabeledField, PhoneInput } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";

@Component({
  selector: "web-demo-dialog",
  templateUrl: "./demo-dialog.html",
  imports: [
    DialogModule,
    ReactiveFormsModule,
    InputTextModule,
    ButtonModule,
    SelectModule,
    TextareaModule,
    PhoneInput,
    LabeledField,
  ],
})
export class DemoDialog {
  private readonly api = inject(Api);

  public readonly visible = model(false);
  protected readonly isLoading = signal(false);
  protected readonly isSubmitted = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly fleetSizeOptions = [
    { label: "1-10 trucks", value: "1-10" },
    { label: "11-25 trucks", value: "11-25" },
    { label: "26-50 trucks", value: "26-50" },
    { label: "51-100 trucks", value: "51-100" },
    { label: "100+ trucks", value: "100+" },
  ];

  protected readonly form = new FormGroup({
    firstName: new FormControl("", { validators: [Validators.required], nonNullable: true }),
    lastName: new FormControl("", { validators: [Validators.required], nonNullable: true }),
    email: new FormControl("", {
      validators: [Validators.required, Validators.email],
      nonNullable: true,
    }),
    company: new FormControl("", { validators: [Validators.required], nonNullable: true }),
    phone: new FormControl("", { nonNullable: true }),
    fleetSize: new FormControl("", { nonNullable: true }),
    message: new FormControl("", { nonNullable: true }),
  });

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid || this.isLoading()) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      const formValue = this.form.getRawValue();

      await this.api.invoke(createDemoRequest, {
        body: {
          firstName: formValue.firstName,
          lastName: formValue.lastName,
          email: formValue.email,
          company: formValue.company,
          phone: formValue.phone,
          fleetSize: formValue.fleetSize,
          message: formValue.message,
        },
      });

      this.isSubmitted.set(true);
      this.form.reset();
    } catch (error) {
      console.error("Error submitting demo request:", error);
      this.errorMessage.set("Failed to submit your request. Please try again.");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected closeDialog(): void {
    this.visible.set(false);
    this.isSubmitted.set(false);
    this.errorMessage.set(null);
  }
}
