import { Component, inject, signal } from "@angular/core";
import { email, form, FormField, FormRoot, required } from "@angular/forms/signals";
import { Api, createContactSubmission, type ContactSubject } from "@logistics/shared/api";
import type { SelectOption } from "@logistics/shared/models";
import {
  Icon,
  UiButton,
  UiFormField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

const EMPTY = {
  firstName: "",
  lastName: "",
  email: "",
  phone: "",
  subject: "",
  message: "",
};

@Component({
  selector: "web-contact-form",
  templateUrl: "./contact-form.html",
  imports: [
    FormField,
    FormRoot,
    Icon,
    ScrollAnimateDirective,
    SectionContainer,
    UiButton,
    UiFormField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class ContactForm {
  private readonly api = inject(Api);

  protected readonly isSubmitted = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly subjectOptions: SelectOption<ContactSubject>[] = [
    { label: "General Inquiry", value: "general" },
    { label: "Sales", value: "sales" },
    { label: "Technical Support", value: "support" },
    { label: "Partnership", value: "partnership" },
    { label: "Press & Media", value: "press" },
  ];

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.firstName, { message: "First name is required." });
      required(p.lastName, { message: "Last name is required." });
      required(p.email, { message: "Email is required." });
      email(p.email, { message: "Enter a valid email address." });
      required(p.subject, { message: "Subject is required." });
      required(p.message, { message: "Message is required." });
    },
    {
      submission: {
        action: async () => {
          this.errorMessage.set(null);

          try {
            const value = this.model();

            await this.api.invoke(createContactSubmission, {
              body: {
                firstName: value.firstName,
                lastName: value.lastName,
                email: value.email,
                phone: value.phone,
                subject: value.subject as ContactSubject,
                message: value.message,
              },
            });

            this.isSubmitted.set(true);
            this.form().reset({ ...EMPTY });
          } catch (error) {
            console.error("Error submitting contact form:", error);
            this.errorMessage.set("Failed to send your message. Please try again.");
          }

          return undefined;
        },
      },
    },
  );

  protected resetForm(): void {
    this.isSubmitted.set(false);
    this.errorMessage.set(null);
  }
}
