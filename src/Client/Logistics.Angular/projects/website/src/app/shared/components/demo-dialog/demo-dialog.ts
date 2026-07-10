import { Component, inject, model, signal } from "@angular/core";
import { email, form, FormField, FormRoot, required } from "@angular/forms/signals";
import { Api, createDemoRequest } from "@logistics/shared/api";
import {
  PhoneField,
  UiFormField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";

const EMPTY = {
  firstName: "",
  lastName: "",
  email: "",
  company: "",
  phone: "",
  fleetSize: "",
  message: "",
};

@Component({
  selector: "web-demo-dialog",
  templateUrl: "./demo-dialog.html",
  imports: [
    ValidatedForm,
    DialogModule,
    FormRoot,
    FormField,
    ButtonModule,
    PhoneField,
    UiFormField,
    UiTextField,
    UiSelectField,
    UiTextareaField,
  ],
})
export class DemoDialog {
  private readonly api = inject(Api);

  public readonly visible = model(false);
  protected readonly isSubmitted = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly fleetSizeOptions = [
    { label: "1-10 trucks", value: "1-10" },
    { label: "11-25 trucks", value: "11-25" },
    { label: "26-50 trucks", value: "26-50" },
    { label: "51-100 trucks", value: "51-100" },
    { label: "100+ trucks", value: "100+" },
  ];

  protected readonly model = signal({ ...EMPTY });

  /**
   * `[formRoot]` runs `submission.action` on submit. It marks the whole tree touched first, skips
   * the action while invalid, and drives `form().submitting()` — so there is no `isLoading` signal
   * and no `markAllAsTouched()` call.
   */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.firstName, { message: "First name is required." });
      required(p.lastName, { message: "Last name is required." });
      required(p.email, { message: "Email is required." });
      email(p.email, { message: "Enter a valid email address." });
      required(p.company, { message: "Company name is required." });
    },
    {
      submission: {
        action: async () => {
          this.errorMessage.set(null);

          try {
            const value = this.model();

            await this.api.invoke(createDemoRequest, {
              body: {
                firstName: value.firstName,
                lastName: value.lastName,
                email: value.email,
                company: value.company,
                phone: value.phone,
                fleetSize: value.fleetSize,
                message: value.message,
              },
            });

            this.isSubmitted.set(true);
            this.form().reset({ ...EMPTY });
          } catch (error) {
            console.error("Error submitting demo request:", error);
            this.errorMessage.set("Failed to submit your request. Please try again.");
          }

          return undefined;
        },
      },
    },
  );

  protected closeDialog(): void {
    this.visible.set(false);
    this.isSubmitted.set(false);
    this.errorMessage.set(null);
  }
}
